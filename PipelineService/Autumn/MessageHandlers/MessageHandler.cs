namespace PipelineService.Autumn.MessageHandlers
{
    public abstract class MessageHandler
    {
        public abstract Task Consume();
    }
}