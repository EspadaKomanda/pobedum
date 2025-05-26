namespace PipelineService.Services.Utils;

public class Pipeline
{
    private List<Guid> _pipeline;

    public Pipeline()
    {
        _pipeline = new List<Guid>();
    }
    
    public List<Guid> GetPipeline() => _pipeline;
    
    public void Add(Guid id) => _pipeline.Add(id);
    
    public void Remove(Guid id) => _pipeline.Remove(id);
    
    public void Clear() => _pipeline.Clear();
    
    public bool Contains(Guid id) => _pipeline.Contains(id);
    public int IndexOf(Guid id) => _pipeline.IndexOf(id);
}