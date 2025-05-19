using System.Reflection;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PipelineService.Autumn.MessageHandlers;
using PipelineService.Autumn.Utils;

namespace PipelineService.Autumn.Extensions;

public static class KafkaServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection services, Action<KafkaOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddSingleton<KafkaTopicManager>();
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var config = options.ProducerConfig;
            return new ProducerBuilder<string, string>(config).Build();
        });
        services.AddSingleton<KafkaProducer>();
        
        services.AddTransient<IConsumer<string, string>>(provider => 
        {
            var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var config = options.ConsumerConfig;
            return new ConsumerBuilder<string, string>(config).Build();
        });
        services.AddTransient<IAdminClient>(provider => 
        {
            var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var config = options.AdminClientConfig;
            return new AdminClientBuilder(config).Build();
        });
        services.AddSingleton<KafkaTopicManager>();
        services.AddSingleton<MessageHandlerFactory>();
        
        return services;
    }

    public static IApplicationBuilder UseKafka(this IApplicationBuilder app, Assembly cAssembly)
    {
        var serviceProvider = app.ApplicationServices;
        MessageHandlerFactory factory = serviceProvider.GetRequiredService<MessageHandlerFactory>();
        var handlers = factory.CreateHandlers(serviceProvider,cAssembly);
        foreach (var handler in handlers)
        {
            new Thread(() => handler.Consume()).Start();
        }    
        
        return app;
    }
}

public class KafkaOptions
{
    public ProducerConfig ProducerConfig { get; set; }
    public ConsumerConfig ConsumerConfig { get; set; }
    public AdminClientConfig AdminClientConfig { get; set; }
}