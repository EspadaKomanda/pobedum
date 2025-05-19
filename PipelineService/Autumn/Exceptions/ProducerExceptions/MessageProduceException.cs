﻿namespace PipelineService.Autumn.Exceptions.ProducerExceptions;

public class MessageProduceException : ProducerException
{
    public MessageProduceException()
    {
    }

    public MessageProduceException(string message)
        : base(message)
    {
    }

    public MessageProduceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}