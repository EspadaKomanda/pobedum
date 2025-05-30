"""
Service containing media-related functions
"""
import os
import uuid
import tempfile
import logging
import json
from typing import Dict, Any
from redis import Redis
import cv2

from PIL import Image, ImageDraw, ImageFont

import numpy as np
from moviepy.editor import (
    VideoClip,
    CompositeAudioClip,
    CompositeVideoClip,
    VideoFileClip,
    concatenate_videoclips,
    AudioFileClip,
    ImageClip
)

from moviepy.audio.fx import all as afx

from moviepy.video.fx.all import fadein, fadeout
import wave
from app.services.kafka import ThreadedKafkaConsumer, KafkaProducerClient
from app.services.s3 import S3Service
from app.config import (
    KAFKA_BROKERS,
    REDIS_HOST,
    REDIS_PORT,
    REDIS_DB,
    REDIS_PASSWORD
)

logger = logging.getLogger(__name__)

class MergeService:
    """
    Service containing functions required to merge audio and photos
    to created animated videos.
    """

    def __init__(self):
        self.logger = logging.getLogger(self.__class__.__name__)
        self.redis = Redis(
            host=REDIS_HOST,
            port=REDIS_PORT,
            db=REDIS_DB,
            password=REDIS_PASSWORD,
            decode_responses=True
        )
        
        # Initialize Kafka consumer
        self.consumer = ThreadedKafkaConsumer(
            bootstrap_servers=KAFKA_BROKERS,
            group_id='merge-service',
            topics=['merge_requests'],
            message_callback=self.process_message
        )
        
        # Initialize Kafka producer
        self.producer = KafkaProducerClient(
            bootstrap_servers=KAFKA_BROKERS,
            value_serializer=lambda v: json.dumps(v).encode('utf-8')
        )


    def _generate_zoom_frames(self, image, duration, size, zoom_start=1.0, zoom_end=1.5, fps=30):
        frames = []

        for t in np.linspace(0, duration, int(duration * fps)):
            scale = zoom_start + (zoom_end - zoom_start) * (t / duration)

            M = cv2.getRotationMatrix2D((size[0]/2, size[1]/2), 0, scale)
            frame = cv2.warpAffine(image, M, size,
                                 flags=cv2.INTER_LANCZOS4,
                                 borderMode=cv2.BORDER_REFLECT101)
            frames.append(frame)

        return frames

    def _combine_videos2(self, video_paths, output_path="combined_video.mp4", enableFading = False, fadingType = None):
        clips = []

        # Загружаем все видео
        for path in video_paths:
            clip = VideoFileClip(path)
            clips.append(clip)

        # Склеиваем видео (можно изменить метод на "compose" для других эффектов)
        final_clip = self._concatenate_videoclips(clips, method="compose")

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

    def _create_transition3(self, clip1, clip2, transition_duration=1.0,):
        """Создаем плавный переход между клипами"""
        # Делаем fadeout для первого клипа и fadein для второго
        clip1_fading = clip1.subclip(clip1.duration - transition_duration).fx(fadeout, transition_duration)
        clip2_fading = clip2.subclip(0, transition_duration).fx(fadein, transition_duration)

        # Склеиваем переход
        transition = concatenate_videoclips([clip1_fading, clip2_fading])
        return transition

    def _combine_videos3(self, video_paths, output_path, transition_duration=1.0, enableFading = False, fadingType = None):
        """Склеиваем видео с переходами без дублирования"""
        clips = [VideoFileClip(path) for path in video_paths]

        # Создаем список для финального видео
        final_clips = []

        # Добавляем первый клип (без перехода в начале)
        final_clips.append(clips[0].subclip(0, -transition_duration))

        # Добавляем переходы и остальные клипы
        for i in range(1, len(clips)):
            # Создаем переход между текущим и следующим клипом
            transition = self._create_transition(clips[i-1], clips[i], transition_duration)
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

    def _crossfadein(clip, duration):
        """Реализация crossfadein эффекта"""
        def effect(get_frame, t):
            if t >= duration:
                return get_frame(t)

            frame = get_frame(t)
            opacity = t / duration
            faded_frame = (frame * opacity).astype(frame.dtype)
            return faded_frame

        return clip.fl(effect)

    def _crossfadeout(clip, duration):
        """Реализация crossfadeout эффекта"""
        def effect(get_frame, t):
            if t <= clip.duration - duration:
                return get_frame(t)

            frame = get_frame(t)
            opacity = 1 - (t - (clip.duration - duration)) / duration
            faded_frame = (frame * opacity).astype(frame.dtype)
            return faded_frame

        return clip.fl(effect)

    def _slide_in(clip, duration, direction='right'):
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

    def _slide_out(clip, duration, direction='left'):
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

    # def _create_transition(self, clip1, clip2, transition_type='fade', transition_duration=1.0):
    #     """Создание перехода между клипами"""
    #     if transition_type == 'fade':
    #         return concatenate_videoclips([
    #             clip1.fx(fadeout, transition_duration),
    #             clip2.fx(fadein, transition_duration)
    #         ])

    #     elif transition_type == 'slide':
    #         return CompositeVideoClip([
    #             self._slide_out(clip1, transition_duration, 'left'),
    #             self._slide_in(clip2, transition_duration, 'right')
    #         ])

    #     elif transition_type == 'zoom':
    #         def zoom_effect(t):
    #             if t < transition_duration:
    #                 zoom = 1.0 - (t/transition_duration)*0.3
    #                 return clip1.resize(zoom).set_position('center')
    #             else:
    #                 zoom = 0.7 + ((t-transition_duration)/transition_duration)*0.3
    #                 return clip2.resize(zoom).set_position('center')
    #         return VideoClip(zoom_effect, duration=transition_duration*2)

    #     elif transition_type in ['crossfade', 'dissolve']:
    #         return CompositeVideoClip([
    #             clip1.set_end(transition_duration),
    #             clip2.set_start(0).fx(self._crossfadein, transition_duration)
    #         ])

    #     else:  # простой переход без эффектов
    #         return concatenate_videoclips([clip1, clip2])

    # XXX : transition_type is actually hardcoded, please fix this
    def _combine_videos(self, video_paths, output_path, transition_type='fade', transition_duration=1.0):
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

        transitions = ["fade", "fade"]

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
            transition = self._create_transition(
                prev_clip_part,
                next_clip_part,
                transitions[i % 2],
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

    def _create_text_overlay_bottom(self, text, duration, size, fontsize=40, bg_opacity=0.5, bottom_margin=20):
        img = Image.new('RGBA', size, (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)

        try:
            font = ImageFont.truetype("/dir/glina_script.ttf", fontsize)
        except:
            font = ImageFont.load_default()

        lines = text.split('\n')
        line_height = fontsize + 5
        total_height = len(lines) * line_height

        # Calculate text widths for each line
        text_sizes = [draw.textlength(line, font=font) for line in lines]
        max_width = max(text_sizes)

        # Calculate background position (full width)
        bg_x1 = 0  # Start from left edge
        bg_y1 = size[1] - total_height - 2*bottom_margin  # Position from bottom
        bg_x2 = size[0]  # Extend to right edge
        bg_y2 = size[1]  # Extend to bottom

        # Draw full-width semi-transparent background
        draw.rectangle([bg_x1, bg_y1, bg_x2, bg_y2],
                     fill=(0, 0, 0, int(255 * bg_opacity)))

        # Draw text (centered horizontally)
        y_pos = bg_y1 + bottom_margin
        for line, width in zip(lines, text_sizes):
            x_pos = (size[0] - width) // 2
            draw.text((x_pos, y_pos), line, font=font, fill=(255, 255, 255, 255))
            y_pos += line_height

        return ImageClip(np.array(img), duration=duration)

    def _create_transition(self, clip1, clip2, transition_type='fade', transition_duration=1.0):
        """Создание перехода между клипами"""
        if transition_type == 'fade':
            return concatenate_videoclips([
                clip1.fx(fadeout, transition_duration),
                clip2.fx(fadein, transition_duration)
            ])

        elif transition_type == 'slide':
            def make_frame(t):
                progress = min(1.0, t / transition_duration)
                offset = int(progress * clip1.w)
                
                frame1 = clip1.get_frame(t)
                frame2 = clip2.get_frame(t)
                
                new_frame = np.zeros_like(frame1)
                new_frame[:, :clip1.w-offset] = frame1[:, offset:]
                new_frame[:, clip1.w-offset:] = frame2[:, :offset]
                
                return new_frame
            
            transition_clip = VideoClip(make_frame, duration=transition_duration)
            transition_clip = transition_clip.set_fps(clip1.fps)
            
            # Аудио для slide перехода
            if clip1.audio and clip2.audio:
                transition_clip.audio = CompositeAudioClip([
                    clip1.audio.subclip(-transition_duration, 0).fx(afx.audio_fadeout, transition_duration),
                    clip2.audio.subclip(0, transition_duration).fx(afx.audio_fadein, transition_duration)
                ])
            elif clip1.audio:
                transition_clip.audio = clip1.audio.subclip(-transition_duration, 0).fx(afx.audio_fadeout, transition_duration)
            elif clip2.audio:
                transition_clip.audio = clip2.audio.subclip(0, transition_duration).fx(afx.audio_fadein, transition_duration)
                
            return transition_clip
        # elif transition_type == 'slide':
        #     # Функция для создания эффекта сдвига
        #     def slide_effect(get_frame, t):
        #         if t < transition_duration:
        #             # Первый клип уезжает влево, второй приезжает справа
        #             progress = t / transition_duration

        #             # Получаем кадры из обоих клипов
        #             frame1 = get_frame(t) if hasattr(clip1, 'get_frame') else clip1.get_frame(t)
        #             frame2 = clip2.get_frame(t) if hasattr(clip2, 'get_frame') else clip2.get_frame(t)

        #             # Вычисляем смещение в пикселях
        #             offset = int(progress * frame1.shape[1])

        #             # Создаем новый кадр
        #             new_frame = np.zeros_like(frame1)

        #             # Первый клип - левая часть (уезжает влево)
        #             new_frame[:, :frame1.shape[1]-offset] = frame1[:, offset:]

        #             # Второй клип - правая часть (приезжает справа)
        #             new_frame[:, frame1.shape[1]-offset:] = frame2[:, :offset]

        #             return new_frame
        #         return get_frame(t)

        #     # Создаем переходный клип
        #     transition_clip = clip1.fl(slide_effect)
        #     transition_clip = transition_clip.set_duration(transition_duration)

        #     return transition_clip

        elif transition_type == 'zoom':
            # Размеры кадра
            w, h = clip1.size

            def make_frame(t):
                # Прогресс перехода (0..1)
                progress = min(1.0, t / transition_duration)

                # Получаем кадры из обоих клипов
                frame1 = clip1.get_frame(t)
                frame2 = clip2.get_frame(t)

                # Масштаб для первого клипа (уменьшение от 1.0 до 0.5)
                zoom1 = 1.0 - 0.5 * progress

                # Масштаб для второго клипа (увеличение от 0.5 до 1.0)
                zoom2 = 0.5 + 0.5 * progress

                # Подготовка кадров
                from PIL import Image
                import numpy as np

                # Обработка первого кадра (уменьшение)
                img1 = Image.fromarray(frame1)
                new_w1, new_h1 = int(w * zoom1), int(h * zoom1)
                img1 = img1.resize((new_w1, new_h1), Image.LANCZOS)

                # Центрирование уменьшенного кадра
                canvas1 = Image.new("RGB", (w, h))
                x_offset1 = (w - new_w1) // 2
                y_offset1 = (h - new_h1) // 2
                canvas1.paste(img1, (x_offset1, y_offset1))

                # Обработка второго кадра (увеличение)
                img2 = Image.fromarray(frame2)
                new_w2, new_h2 = int(w * zoom2), int(h * zoom2)
                img2 = img2.resize((new_w2, new_h2), Image.LANCZOS)

                # Центрирование увеличивающегося кадра
                canvas2 = Image.new("RGB", (w, h))
                x_offset2 = (w - new_w2) // 2
                y_offset2 = (h - new_h2) // 2
                canvas2.paste(img2, (x_offset2, y_offset2))

                # Смешивание кадров с учетом прогресса
                blended = Image.blend(canvas1, canvas2, progress)

                return np.array(blended)

            # Создаем переходный клип
            transition_clip = VideoClip(make_frame, duration=transition_duration)
            transition_clip = transition_clip.set_fps(clip1.fps)

            return transition_clip

        else:
            raise ValueError(f"Неизвестный тип перехода: {transition_type}")


    def _process_video_generation(self,
        images, display_texts, audio_paths, speech_texts, fps = 30, frameTime = 5.0,
        enableAudio = False, enableFading = False, fadingType = None, enableSubTitles = False, subTitlesPosition = 'center', task_id = None):
        """Processes the video generation"""

        # Аудио
        audio_durations = []
            
        self.producer.send_message(
            topic="status_update_requests",
            value={
                "Status": 5, # Add sound
                "TaskId": task_id,
            },
            method="updateStatus"
        )

        if (enableAudio):
            for file_path in audio_paths:
                with wave.open(file_path, 'r') as wav_file:
                    frames = wav_file.getnframes()
                    rate = wav_file.getframerate()
                    duration = frames / float(rate)
                    audio_durations.append(duration)
        else:
            for i in range(len(images)):
                filename = f"{uuid.uuid4()}.mp3"
                self._create_silent_audio(filename, duration=frameTime)
                audio_paths.append(filename)
                audio_durations.append(frameTime)

        self.producer.send_message(
            topic="status_update_requests",
            value={
                "Status": 4, # Making videos
                "TaskId": task_id,
            },
            method="updateStatus"
        )

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
            frames = self._generate_zoom_frames(img, audio_durations[i]+1.5, (width, height), zoom_start, zoom_end, fps)

            # 4. Создаем видеоклип
            def make_frame(t):
                frame_idx = min(int(t * fps), len(frames) - 1)
                return frames[frame_idx]

            video_clip = VideoClip(make_frame, duration=audio_durations[i])
            video_clip.fps = fps

            if (enableSubTitles):
                text_clip = self._create_text_overlay_bottom(
                    display_texts[i],
                    duration=audio_durations[i],
                    size=(width, height),
                    fontsize=40,
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
            out_path = f"/tmp/{uuid.uuid4()}.mp4"
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
            video_files.append(out_path)

        self.logger.info("Finished creating video parts for %s", task_id)
        # TODO: Удаляем временные файлы

        #for f in video_files:
            # os.remove(f)
            # 9. Удаляем временные файлы
            # os.remove(audio_paths[i])

            #video_files.append(out_path)

        self.producer.send_message(
            topic="status_update_requests",
            value={
                "Status": 6, # Merge videos
                "TaskId": task_id,
            },
            method="updateStatus"
        )

        # Склейка
        for i, f in enumerate(video_files, 1):
            print(f"{i}. {f}")

        # with tempfile.NamedTemporaryFile(suffix='.mp4', delete=True) as temp_file:

        #     # Get the name of the temporary file
        #     temp_file_name = temp_file.name
        #     
        #     # Call your function to combine videos and save the output to the temporary file
        #     output_file_path = self._combine_videos(video_files, temp_file_name, fadingType, 1.0)
        
        temp_file_name = f"/tmp/{uuid.uuid4()}.mp4"
        output_file_path = self._combine_videos(video_files, temp_file_name, fadingType, 1.0)
        
        self.producer.send_message(
            topic="status_update_requests",
            value={
                "Status": 7, # Final process
                "TaskId": task_id,
            },
            method="updateStatus"
        ) 

        # Удаляем временные файлы
        for f in video_files:
            os.remove(f)

        return output_file_path

    def _get_asset_status(self, pipeline_guid: str) -> Dict[str, bool]:
        """Get completion status for both asset types from Redis."""
        return {
            'audio': self.redis.exists(f"{pipeline_guid}:audio"),
            'photos': self.redis.exists(f"{pipeline_guid}:photos")
        }

    def _handle_completion(self, pipeline_guid: str, video_guid: str):
        """Trigger video generation when both assets are ready."""
        temp_images = []
        temp_audios = []
        temp_structure = None
        output_file_path = None

        try:
            # Download structure file
            with tempfile.NamedTemporaryFile(delete=False) as temp_file:
                temp_structure = temp_file.name
                S3Service.download(
                    bucket_name=pipeline_guid,
                    source="structure.json",
                    destination=temp_structure
                )
                with open(temp_structure, 'r', encoding='utf-8') as f:
                    structure = json.load(f)

            # Download generated assets
            images = []
            audio_paths = []
            for i, _ in enumerate(structure):
                # Download image
                img_key = f"photos/photo{i+1}.jpg"
                with tempfile.NamedTemporaryFile(suffix='.jpg', delete=False) as temp_img:
                    S3Service.download(
                        bucket_name=pipeline_guid,
                        source=img_key,
                        destination=temp_img.name
                    )
                    images.append(temp_img.name)
                    temp_images.append(temp_img.name)

                # Download audio
                audio_key = f"audio/audio{i+1}.wav"
                with tempfile.NamedTemporaryFile(suffix='.wav', delete=False) as temp_audio:
                    S3Service.download(
                        bucket_name=pipeline_guid,
                        source=audio_key,
                        destination=temp_audio.name
                    )
                    audio_paths.append(temp_audio.name)
                    temp_audios.append(temp_audio.name)

            # Generate final video
            output_file_path = self._process_video_generation(
                images=images,
                display_texts=[p['text'] for p in structure],
                audio_paths=audio_paths,
                speech_texts=[p['text'] for p in structure],
                enableAudio=True,
                enableSubTitles=True,
                task_id=pipeline_guid # For updating status for pipeline
                
            )

            # Upload video to S3
            S3Service.upload(
                bucket_name=pipeline_guid,
                destination=f"{video_guid}.mp4",
                source=output_file_path
            )

            # Notify pipeline
            self.producer.send_message(
                topic="status_update_requests",
                value={
                    "TaskId": pipeline_guid,
                    "Status": 8, # Success
                    "VideoId": video_guid
                },
                method="updateStatus"
            )

        except Exception as e:
            self.logger.exception("Video generation failed: %s", e)
            self.producer.send_message(
                topic="status_update_requests",
                value={
                    "TaskId": pipeline_guid,
                    "Status": 11 # Error 
                },
                method="updateStatus"
            )
        finally:
            # Cleanup temporary files
            for path in [temp_structure, output_file_path]:
                if path and os.path.exists(path):
                    try:
                        os.remove(path)
                    except Exception as e:
                        self.logger.warning("Failed to clean up %s: %s", path, e)
            
            for path in temp_images + temp_audios:
                if path and os.path.exists(path):
                    try:
                        os.remove(path)
                    except Exception as e:
                        self.logger.warning("Failed to clean up %s: %s", path, e)

            # Clean Redis state
            self.redis.delete(f"{pipeline_guid}:audio")
            self.redis.delete(f"{pipeline_guid}:photos")

    def process_message(self, message: Dict[str, Any]):
        """Process incoming merge requests."""
        try:
            pipeline_guid = message['TaskId']
            video_guid = message['VideoId']

            status = message['Status']

            # This logic isn't particularly good but it's alright for now
            if status == 'audio_completed':
                status_type = 'audio'
            elif status == 'photos_completed':
                status_type = 'photos'
            else:
                self.logger.error("Incorrect status type: %s", status)
                return

            self.logger.info("%s is ready for %s, will memorize in Redis", status_type, pipeline_guid)

            # Update completion status
            self.redis.set(f"{pipeline_guid}:{status_type}", "completed")
            
            # Check if both assets are ready
            if all(self._get_asset_status(pipeline_guid).values()):
                self.logger.info("Starting video merge for %s", pipeline_guid)
                self._handle_completion(pipeline_guid, video_guid)

        except KeyError as e:
            self.logger.error("Invalid message format: %s", e)
        except Exception as e:
            self.logger.error("Message processing failed: %s", e)

    def start(self):
        """Start the Kafka consumer thread."""
        self.consumer.start()
        self.logger.info("MergeService started")

    def shutdown(self):
        """Gracefully shutdown the service."""
        self.consumer.stop()
        self.producer.flush()
        self.logger.info("MergeService stopped")
