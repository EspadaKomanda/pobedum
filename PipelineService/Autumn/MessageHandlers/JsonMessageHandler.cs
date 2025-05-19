using System.Text;
using Confluent.Kafka;
using Newtonsoft.Json;
using PipelineService.Autumn.Exceptions.ConsumerExceptions;
using PipelineService.Autumn.Exceptions.ProducerExceptions;
using PipelineService.Autumn.Utils;
using PipelineService.Autumn.Utils.Models;

namespace PipelineService.Autumn.MessageHandlers
{
    public class JsonMessageHandler(
        KafkaProducer producer,
        IConsumer<string, string> consumer,
        MessageHandlerConfig messageHandlerConfig,
        ILogger<JsonMessageHandler> logger,
        IServiceProvider serviceProvider)
        : MessageHandler
    {
        public override async Task Consume()
        {
            try
            {
                consumer.Assign(GetPartitions());
                while (true)
                {
                    try
                    {
                        ConsumeResult<string, string> message = consumer.Consume();
                        if (await HandleMessage(message))
                        {
                            consumer.Commit(message);
                        }
                    }
                    catch (HandleMethodException ex)
                    {
                        logger.LogError(ex, "Handling error: {Message}", ex.Message);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred: {Message}", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while consuming: {Message}", ex.Message);
            }
            finally
            {
                consumer.Close();
            }
        }
        private List<TopicPartition> GetPartitions()
        {
            var partitions = new List<TopicPartition>();
            for (int partition = 0; partition < messageHandlerConfig.RequestTopicConfig.PartitionsCount; partition++)
            {
                partitions.Add(new TopicPartition(messageHandlerConfig.RequestTopicConfig.TopicName, partition));
            }
            return partitions;
        }
        private async Task<bool> HandleMessage(ConsumeResult<string,string> message)
        {
            var headerBytes = message.Message.Headers
                        .FirstOrDefault(x => x.Key.Equals("method"));
            if (headerBytes != null)
            {
                var methodString = Encoding.UTF8.GetString(headerBytes.GetValueBytes());
                if(messageHandlerConfig.kafkaMethodExecutionConfigs.Any(x=>x.KafkaMethodName.Equals(methodString)))
                {
                    KafkaMethodExecutionConfig config = messageHandlerConfig.kafkaMethodExecutionConfigs.FirstOrDefault(x=>x.KafkaMethodName.Equals(methodString)) ?? throw new MethodInvalidException("Invalid method name");
                    object result;
                    if(config.ServiceMethodPair.Parameters.Count()>0)
                    {
                        List<object> parameters = new List<object>();
                        foreach (var parameter in config.ServiceMethodPair.Parameters)
                        {
                            parameters.Add( JsonConvert.DeserializeObject(message.Message.Value,parameter.GetType()));
                        }
                        result = ServiceResolver.InvokeMethodByHeader(serviceProvider, config.ServiceMethodPair.Method, config.ServiceMethodPair.Service,parameters);
                    }
                    else
                    {
                        result = ServiceResolver.InvokeMethodByHeader(serviceProvider, config.ServiceMethodPair.Method, config.ServiceMethodPair.Service, null);
                    }
                    if(config.RequireResponse)
                    {
                        if(config.KafkaServiceName!=null)
                        {
                            return await SendResponse(config,new Message<string, string>(){
                            Key = message.Message.Key,
                                Value = JsonConvert.SerializeObject(result),
                                Headers = [
                                    new Header("method",Encoding.UTF8.GetBytes(methodString)),
                                    new Header("sender",Encoding.UTF8.GetBytes(config.KafkaServiceName))
                                ]
                            });
                        }
                        return await SendResponse(config,new Message<string, string>(){
                            Key = message.Message.Key,
                                Value = JsonConvert.SerializeObject(result),
                                Headers = [
                                    new Header("method",Encoding.UTF8.GetBytes(methodString)),
                                    new Header("sender",Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("GLOBAL_SERVICE_NAME") ?? throw new ConfigInvalidException("Invalid config")))
                                ]
                            });
                    }
                    else
                    {
                        return true;
                    }
                }
                throw new MethodInvalidException("Invalid method name");
            }
            throw new HeaderBytesNullException("Header bytes are null");
        }
        private async Task<bool> SendResponse(KafkaMethodExecutionConfig config, Message<string,string> message)
        {
            try
            {
                if(config is { responseTopicPartition: not null, responseTopicConfig: not null })
                {
                    return await producer.ProduceAsync(config.responseTopicConfig, (int)config.responseTopicPartition, message);
                }
                throw new ProducerException("Failed to send response");
            }
            catch
            {
                return false;
            }
        }
    }
}