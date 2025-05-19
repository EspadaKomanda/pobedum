using System.Reflection;
using Confluent.Kafka;
using PipelineService.Autumn.Attributes.MethodAttributes;
using PipelineService.Autumn.Attributes.ServiceAttributes;
using PipelineService.Autumn.Utils;
using PipelineService.Autumn.Utils.Models;

namespace PipelineService.Autumn.MessageHandlers;

//TODO: write factory for Message handlers
public class MessageHandlerFactory
{
    //TODO: Add 
    public IEnumerable<MessageHandler> CreateHandlers(IServiceProvider serviceProvider, Assembly cAssembly)
    {
        var handlers = new List<MessageHandler>();
        
        var handlerConfigs = CreateKafkaMessageHandlerConfig(cAssembly).ToList();

    

        foreach (var config in handlerConfigs)
        {
            

          
            if (config.MessageHandlerType == MessageHandlerType.JSON)
            {
                var producer = serviceProvider.GetRequiredService<KafkaProducer>();
                var consumer = serviceProvider.GetRequiredService<IConsumer<string, string>>();
                var loggerType = typeof(ILogger<>).MakeGenericType(typeof(JsonMessageHandler));
                var logger = serviceProvider.GetRequiredService(loggerType);
                var handler = (MessageHandler)ActivatorUtilities.CreateInstance(
                    serviceProvider,
                    typeof(JsonMessageHandler), 
                    producer, 
                    consumer, 
                    config, 
                    logger, 
                    serviceProvider);
                
                handlers.Add(handler);
            }
            
        }

        return handlers;
    }

    private IEnumerable<MessageHandlerConfig> CreateKafkaMessageHandlerConfig(Assembly cAssembly)
    {
        var configs = new List<MessageHandlerConfig>();
        

        var serviceTypes = cAssembly.GetTypes()
            .Where(t => t.GetCustomAttributes<KafkaServiceAttribute>().Any() || 
                       t.GetCustomAttributes<KafkaSimpleServiceAttribute>().Any()).ToList();

        foreach (var serviceType in serviceTypes)
        {
            var isSimpleService = serviceType.GetCustomAttributes<KafkaSimpleServiceAttribute>().Any();
            var serviceAttribute = isSimpleService 
                ? (Attribute)serviceType.GetCustomAttributes<KafkaSimpleServiceAttribute>().First() 
                : serviceType.GetCustomAttributes<KafkaServiceAttribute>().First();
         
            var methods = serviceType.GetMethods()
                .Where(m => isSimpleService 
                    ? m.GetCustomAttributes<KafkaSimpleMethodAttribute>().Any()
                    : m.GetCustomAttributes<KafkaMethodAttribute>().Any());

            if (!methods.Any())
            {
                throw new InvalidOperationException($"Service {serviceType.Name} has no valid Kafka methods");
            }

            var methodConfigs = methods.Select(m => 
            {
                
                var methodAttr = isSimpleService
                    ? (Attribute)m.GetCustomAttributes<KafkaSimpleMethodAttribute>().First()
                    : m.GetCustomAttributes<KafkaMethodAttribute>().First();
               var config = new KafkaMethodExecutionConfig();

                if (isSimpleService)
                {
                    var simpleAttr = (KafkaSimpleMethodAttribute)methodAttr;
                    var serviceAttr = (KafkaSimpleServiceAttribute)serviceAttribute;
                    config.KafkaMethodName = simpleAttr.MethodName;
                    config.responseTopicConfig = serviceAttr.ResponseTopic;
                    config.RequireResponse = simpleAttr.RequiresResponse;
                    config.KafkaServiceName = serviceAttr.KafkaServiceName;
                    config.responseTopicPartition = serviceAttr.ResponsePartition;
                    config.ServiceMethodPair = new ServiceMethodPair()
                    {
                        Service = serviceType,
                        Method = m,
                        Parameters = m.GetParameters()
                    };
                    
                }
                else 
                {
                    var kafkaMethodAttr = (KafkaMethodAttribute)methodAttr;
                    var serviceAttr = (KafkaServiceAttribute)serviceAttribute;
                    config.KafkaMethodName = kafkaMethodAttr.MethodName;
                    config.responseTopicConfig = serviceAttr.ResponseTopicConfig;
                    config.responseTopicPartition = kafkaMethodAttr.Partition;
                    config.RequireResponse = kafkaMethodAttr.RequiresResponse;
                    config.KafkaServiceName = serviceAttr.KafkaServiceName;
                    config.ServiceMethodPair = new ServiceMethodPair()
                    {
                        Service = serviceType,
                        Method = m,
                        Parameters = m.GetParameters()
                    };
                    
                }

                return config;
            }).ToList();
            var serviceAttr = (KafkaSimpleServiceAttribute)serviceAttribute;
            configs.Add( new MessageHandlerConfig()
            {
                kafkaMethodExecutionConfigs = new HashSet<KafkaMethodExecutionConfig>( methodConfigs),
                RequestTopicConfig = serviceAttr.RequestTopic,
                MessageHandlerType = serviceAttr.MessageHandlerType
            });
        }

        return configs;
    } 
}
