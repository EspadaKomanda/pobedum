"""
Service containing media-related functions
"""
import os
import io
import uuid

import cv2
import numpy as np
import torch
from PIL import Image, ImageDraw, ImageFont
from gtts import gTTS
from pydub import AudioSegment
from moviepy.editor import (
    VideoClip,
    CompositeVideoClip,
    ImageClip,
    VideoFileClip,
    concatenate_videoclips,
    AudioFileClip,
)
from moviepy.video.fx.all import fadein, fadeout
from diffusers import DiffusionPipeline

class MergeService:
    """
    Service containing functions required to merge audio and photos
    to created animated videos.
    """

    def generate_zoom_frames(image, duration, size, zoom_start=1.0, zoom_end=1.5, fps=30):
        frames = []

        for t in np.linspace(0, duration, int(duration * fps)):
            scale = zoom_start + (zoom_end - zoom_start) * (t / duration)

            M = cv2.getRotationMatrix2D((size[0]/2, size[1]/2), 0, scale)
            frame = cv2.warpAffine(image, M, size,
                                 flags=cv2.INTER_LANCZOS4,
                                 borderMode=cv2.BORDER_REFLECT101)
            frames.append(frame)

        return frames

    def combine_videos2(video_paths, output_path="combined_video.mp4", enableFading = False, fadingType = None):
        clips = []

        # Загружаем все видео
        for path in video_paths:
            clip = VideoFileClip(path)
            clips.append(clip)

        # Склеиваем видео (можно изменить метод на "compose" для других эффектов)
        final_clip = concatenate_videoclips(clips, method="compose")

        # Сохраняем результат
        final_clip.write_videofile(
            output_path,
            codec="libx264",
            audio_codec="aac",
            fps=clips[0].fps,  # Сохраняем FPS первого видео
            threads=4
        )

        # Закрываем все клипы
        for clip in clips:
            clip.close()

        return output_path

    def create_transition3(clip1, clip2, transition_duration=1.0,):
        """Создаем плавный переход между клипами"""
        # Делаем fadeout для первого клипа и fadein для второго
        clip1_fading = clip1.subclip(clip1.duration - transition_duration).fx(fadeout, transition_duration)
        clip2_fading = clip2.subclip(0, transition_duration).fx(fadein, transition_duration)

        # Склеиваем переход
        transition = concatenate_videoclips([clip1_fading, clip2_fading])
        return transition

    def combine_videos3(video_paths, output_path, transition_duration=1.0, enableFading = False, fadingType = None):
        """Склеиваем видео с переходами без дублирования"""
        clips = [VideoFileClip(path) for path in video_paths]

        # Создаем список для финального видео
        final_clips = []

        # Добавляем первый клип (без перехода в начале)
        final_clips.append(clips[0].subclip(0, -transition_duration))

        # Добавляем переходы и остальные клипы
        for i in range(1, len(clips)):
            # Создаем переход между текущим и следующим клипом
            transition = create_transition(clips[i-1], clips[i], transition_duration)
            final_clips.append(transition)

            # Добавляем основной контент следующего клипа (без перехода)
            if i < len(clips) - 1:  # Для всех кроме последнего
                final_clips.append(clips[i].subclip(transition_duration, -transition_duration))
            else:  # Для последнего клипа
                final_clips.append(clips[i].subclip(transition_duration))

        # Склеиваем все части
        final_clip = concatenate_videoclips(final_clips)

        # Сохраняем результат
        final_clip.write_videofile(
            output_path,
            codec="libx264",
            audio_codec="aac",
            fps=clips[0].fps,
            threads=4,
            preset='medium',
            bitrate='8000k'
        )

        # Закрываем клипы
        for clip in clips:
            clip.close()

        return output_path

    def crossfadein(clip, duration):
        """Реализация crossfadein эффекта"""
        def effect(get_frame, t):
            if t >= duration:
                return get_frame(t)

            frame = get_frame(t)
            opacity = t / duration
            faded_frame = (frame * opacity).astype(frame.dtype)
            return faded_frame

        return clip.fl(effect)

    def crossfadeout(clip, duration):
        """Реализация crossfadeout эффекта"""
        def effect(get_frame, t):
            if t <= clip.duration - duration:
                return get_frame(t)

            frame = get_frame(t)
            opacity = 1 - (t - (clip.duration - duration)) / duration
            faded_frame = (frame * opacity).astype(frame.dtype)
            return faded_frame

        return clip.fl(effect)

    def slide_in(clip, duration, direction='right'):
        """Реализация slide-in эффекта"""
        def effect(get_frame, t):
            if t >= duration:
                return get_frame(t)

            frame = get_frame(t)
            h, w = frame.shape[:2]
            offset = int((1 - t/duration) * w)

            new_frame = np.zeros_like(frame)
            if direction == 'right':
                new_frame[:, -offset:] = frame[:, :offset]
            elif direction == 'left':
                new_frame[:, :w-offset] = frame[:, offset:]
            elif direction == 'top':
                new_frame[:h-offset, :] = frame[offset:, :]
            else:  # bottom
                new_frame[-offset:, :] = frame[:offset, :]

            return new_frame

        return clip.fl(effect)

    def slide_out(clip, duration, direction='left'):
        """Реализация slide-out эффекта"""
        def effect(get_frame, t):
            if t <= clip.duration - duration:
                return get_frame(t)

            frame = get_frame(t)
            h, w = frame.shape[:2]
            progress = (t - (clip.duration - duration)) / duration
            offset = int(progress * w)

            new_frame = np.zeros_like(frame)
            if direction == 'right':
                new_frame[:, offset:] = frame[:, :w-offset]
            elif direction == 'left':
                new_frame[:, :w-offset] = frame[:, offset:]
            elif direction == 'top':
                new_frame[:h-offset, :] = frame[offset:, :]
            else:  # bottom
                new_frame[offset:, :] = frame[:h-offset, :]

            return new_frame

        return clip.fl(effect)

    def create_transition(clip1, clip2, transition_type='fade', transition_duration=1.0):
        """Создание перехода между клипами"""
        if transition_type == 'fade':
            return concatenate_videoclips([
                clip1.fx(fadeout, transition_duration),
                clip2.fx(fadein, transition_duration)
            ])

        elif transition_type == 'slide':
            return CompositeVideoClip([
                slide_out(clip1, transition_duration, 'left'),
                slide_in(clip2, transition_duration, 'right')
            ])

        elif transition_type == 'zoom':
            def zoom_effect(t):
                if t < transition_duration:
                    zoom = 1.0 - (t/transition_duration)*0.3
                    return clip1.resize(zoom).set_position('center')
                else:
                    zoom = 0.7 + ((t-transition_duration)/transition_duration)*0.3
                    return clip2.resize(zoom).set_position('center')
            return VideoClip(zoom_effect, duration=transition_duration*2)

        elif transition_type in ['crossfade', 'dissolve']:
            return CompositeVideoClip([
                clip1.set_end(transition_duration),
                clip2.set_start(0).fx(crossfadein, transition_duration)
            ])

        else:  # простой переход без эффектов
            return concatenate_videoclips([clip1, clip2])

    def combine_videos(video_paths, output_path, transition_type='fade', transition_duration=1.0):
        """Основная функция для склейки видео с переходами"""
        if not video_paths:
            raise ValueError("Список видео пуст")

        clips = [VideoFileClip(path) for path in video_paths]

        if len(clips) == 1:
            clips[0].write_videofile(output_path)
            return output_path

        final_clips = []

        # Первый клип без перехода в начале
        first_clip_end = max(0, clips[0].duration - transition_duration)
        final_clips.append(clips[0].subclip(0, first_clip_end))

        for i in range(1, len(clips)):
            # Части для перехода
            prev_clip_part = clips[i-1].subclip(
                max(0, clips[i-1].duration - transition_duration),
                clips[i-1].duration
            )
            next_clip_part = clips[i].subclip(
                0,
                min(transition_duration, clips[i].duration)
            )

            # Создаем переход
            transition = create_transition(
                prev_clip_part,
                next_clip_part,
                transition_type,
                transition_duration
            )
            final_clips.append(transition)

            # Основная часть следующего клипа
            if i < len(clips) - 1:
                start = transition_duration
                end = max(transition_duration, clips[i].duration - transition_duration)
                final_clips.append(clips[i].subclip(start, end))
            else:
                final_clips.append(clips[i].subclip(transition_duration))

        # Финалная склейка
        final_clip = concatenate_videoclips(final_clips)

        # Сохранение
        final_clip.write_videofile(
            output_path,
            codec="libx264",
            audio_codec="aac",
            fps=clips[0].fps,
            threads=4,
            preset='medium',
            bitrate='8000k'
        )

        # Очистка
        for clip in clips:
            clip.close()

        return output_path

    def start(prompts, display_texts, speech_texts, fps = 30, frameTime = 5.0, enableAudio = False, enableFading = False, fadingType = None, enableSubTitles = False, subTitlesPosition = 'center'):
        # Получение параметров
        result_path = f"{uuid.uuid4()}.mp4"

        images = []
        # Генерация
        # for p in prompts:
        #    images.append(generateImage(p))
        for p in prompts:
            images.append(p)

        # Аудио
        audio_paths = []
        audio_durations = []
        if (enableAudio):
            for speech_text in speech_texts:
                filename = f"{uuid.uuid4()}.mp3"
                audio_path, audio_duration = text_to_speech(speech_text, filename=filename)
                audio_paths.append(audio_path)
                audio_durations.append(audio_duration)
        else:
            for i in range(len(images)):
                filename = f"{uuid.uuid4()}.mp3"
                create_silent_audio(filename, duration=frameTime)
                audio_paths.append(filename)
                audio_durations.append(frameTime)

        # Создаём видио
        video_files = []
        for i in range(len(images)):
            flag = i % 2 == 0
            # 2. Загружаем изображение
            img = cv2.imread(images[i])
            img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
            height, width = img.shape[:2]

            zoom_start = 1.5 if flag else 1.0
            zoom_end = 1.0 if flag else 1.5

            # 3. Генерируем кадры с учетом длительности аудио
            frames = generate_zoom_frames(img, audio_durations[i], (width, height), zoom_start, zoom_end, fps)

            # 4. Создаем видеоклип
            def make_frame(t):
                frame_idx = min(int(t * fps), len(frames) - 1)
                return frames[frame_idx]

            video_clip = VideoClip(make_frame, duration=audio_durations[i])
            video_clip.fps = fps

            # 5. Создаем текстовый слой
            if (enableSubTitles):
                text_clip = create_text_overlay_bottom(
                    display_texts[i],
                    duration=audio_durations[i],
                    size=(width, height),
                    fontsize=12,
                    bg_opacity=0.5
                )

                # 6. Комбинируем все элементы
                final_clip = CompositeVideoClip([video_clip, text_clip.set_position('center')])
            else:
                final_clip = video_clip

            # 7. Добавляем аудио
            if (enableAudio):
                audio_clip = AudioFileClip(audio_paths[i])
                final_clip = final_clip.set_audio(audio_clip)

            # 8. Сохраняем результат
            out_path = f"{uuid.uuid4()}.mp4"
            final_clip.write_videofile(
                out_path,
                fps=fps,
                codec='libx264',
                preset='medium',
                bitrate='8000k',
                threads=4,
                audio_codec='aac',
                audio_bitrate='192k'
            )

            # 9. Удаляем временные файлы
            os.remove(audio_paths[i])

            video_files.append(out_path)

        # Склейка
        for i, f in enumerate(video_files, 1):
            print(f"{i}. {f}")

        output = combine_videos(video_files, result_path, fadingType, 1.0)

        print("\nВидео успешно склеены! Скачиваем результат...")
        files.download(output)

        # Удаляем временные файлы
        for f in video_files:
            os.remove(f)
