using System.Reflection;
using PipelineService.Autumn.Attributes.ServiceAttributes;
using PipelineService.Autumn.Exceptions.ReflectionExceptions;
using PipelineService.Autumn.Utils.Models;

namespace PipelineService.Autumn.Utils
{
    public static class ServiceResolver
    {
        public static object InvokeMethodByHeader(IServiceProvider serviceProvider,MethodInfo methodInfo,Type service,IEnumerable<object>? parameters)
        {
            if(parameters!=null)
            {
                return InvokeMethodWithParameters( methodInfo, GetScopedService(serviceProvider,service), parameters);
            }
            return InvokeMethodWithoutParameters( methodInfo, GetScopedService(serviceProvider,service));
        }
        private static object InvokeMethodWithoutParameters(MethodInfo method,object serviceInstance)
        {
            if (method.GetParameters().Length != 0)
            {
                throw new InvokeMethodException("Wrong method implementation: method should not have parameters.");
            }

            if (method.ReturnType == typeof(void))
            {
                method.Invoke(serviceInstance, null);
                return true;
            }
            else
            {
                var result = method.Invoke(serviceInstance, null);
                if (result != null)
                {
                    return result;
                }
            }
            throw new InvokeMethodException("Method invocation failed");
        }

        private static object InvokeMethodWithParameters(MethodInfo method, object serviceInstance, IEnumerable<object> parameters)
        {

            if (method.GetParameters().Length == 0)
            {
                throw new InvokeMethodException("Wrong method implementation: method should have parameters.");
            }

            if (method.ReturnType == typeof(void))
            {
                method.Invoke(serviceInstance,  parameters.ToArray() );
                return true;
            }
            else
            {
                var result = method.Invoke(serviceInstance,parameters.ToArray());
                if (result != null)
                {
                    return result;
                }
            }
            throw new InvokeMethodException("Method invocation failed");
        }
       
        private static object GetScopedService(IServiceProvider serviceProvider, Type serviceType)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var serviceInstance = scope.ServiceProvider.GetRequiredService(serviceType.GetInterfaces().FirstOrDefault() ?? serviceType);
                if (serviceInstance != null)
                {
                    return serviceInstance;
                }
            }
            throw new GetScopedServiceException("Failed to get scoped service");
        }
        
        public static IEnumerable<IGrouping<TopicConfig, Type>> GetKafkaServices()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute(typeof(KafkaServiceAttribute), false)!=null)
                .GroupBy(t =>
                    ((KafkaServiceAttribute)t.GetCustomAttribute(typeof(KafkaServiceAttribute), false)!).RequestTopicConfig);

        }

        public static IEnumerable<IGrouping<TopicConfig, Type>> GetSimpleKafkaServices()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute(typeof(KafkaSimpleServiceAttribute), false)!=null)
                .GroupBy(t =>
                    ((KafkaSimpleServiceAttribute)t.GetCustomAttribute(typeof(KafkaSimpleServiceAttribute), false)!).RequestTopic);
        }
        private static IEnumerable<MethodInfo> GetMethodsByAttribute(Type attributeType, Type serviceType)
        {
            var methods = serviceType.GetMethods()
            .Where(m => m.GetCustomAttributes(attributeType, false).Any());
            return new HashSet<MethodInfo>(methods);
        }

       
    }
}