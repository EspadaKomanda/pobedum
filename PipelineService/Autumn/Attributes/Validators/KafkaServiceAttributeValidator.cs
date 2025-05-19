using System.Reflection;

namespace PipelineService.Autumn.Attributes.Validators
{
    public static class KafkaServiceAttributeValidator
    {
        public static void ValidateAttributes<T>(Type serviceAttributeType, Type methodAttributeType)
        {
            var type = typeof(T);
            var hasClassAttribute = type.GetCustomAttributes(serviceAttributeType, false).Any();

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var hasMethodAttribute = method.GetCustomAttributes(methodAttributeType, false).Any();
                if (hasMethodAttribute && !hasClassAttribute)
                {
                    throw new InvalidOperationException($"Method '{method.Name}' cannot have '{methodAttributeType.Name}' without the class having '{serviceAttributeType.Name}'.");
                }
            }
        }
    }
}