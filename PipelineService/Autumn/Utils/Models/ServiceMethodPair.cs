using System.Reflection;

namespace PipelineService.Autumn.Utils.Models
{
    public class ServiceMethodPair
    {
        public Type Service { get; set; } = null!;
        public MethodInfo Method { get; set; } = null!;
        public IEnumerable<ParameterInfo>? Parameters { get; set; }
    }
}