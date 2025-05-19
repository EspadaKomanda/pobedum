namespace PipelineService.Utils;

public enum GenerationStatuses
{
    VALIDATION,
    ANALYZE_LETTER,
    CREATING_IMAGES,
    CREATING_AUDIO,
    MAKING_VIDEOS,
    ADD_SOUND,
    MERGE_VIDEOS,
    FINAL_PROCESS,
    SUCCESS,
    WAITING,
    CANCELLED,
    ERROR
}