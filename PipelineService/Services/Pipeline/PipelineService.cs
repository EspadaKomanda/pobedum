using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PipelineService.Autumn.Attributes.MethodAttributes;
using PipelineService.Autumn.Attributes.ServiceAttributes;
using PipelineService.Autumn.Utils;
using PipelineService.Autumn.Utils.Models;
using PipelineService.Communicators;
using PipelineService.Database;
using PipelineService.Database.Repositories;
using PipelineService.Models.Database;
using PipelineService.Models.Internal.Kafka.Requests;
using PipelineService.Models.Internal.Requests;
using PipelineService.Models.Requests;
using PipelineService.Models.Responses;
using PipelineService.Utils;

namespace PipelineService.Services.Pipeline;

[KafkaSimpleService("status_update_requests",3,1,MessageHandlerType.JSON,"PipelineService")]
public class PipelineService : IPipelineService
{
    #region Fields

    private readonly ILogger<PipelineService> _logger;
    private readonly KafkaProducer _producer;
    private readonly VideosCommunicator _communicator;
    private readonly UnitOfWork _unitOfWork;
    private readonly Utils.Pipeline _pipeline;
    private IConfiguration _configuration;
    
    #endregion

    #region Constructor

    public PipelineService(ILogger<PipelineService> logger, KafkaProducer producer, VideosCommunicator communicator, UnitOfWork unitOfWork, IConfiguration configuration, Utils.Pipeline pipeline)
    {
        _logger = logger;
        _producer = producer;
        _communicator = communicator;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _pipeline = pipeline;
    }

    #endregion

    #region Methods

    public async Task<BeginVideoGenerationResponse> BeginVideoGeneration(BeginVideoGenerationRequest request, Guid userId)
    {
        try
        {
            var pipelineItem = await _unitOfWork.PipelineRepository.InsertAsync(
                new PipelineItem()
                {
                    Id = request.TaskId,
                    BeginTime = DateTime.UtcNow,
                    ColorScheme = request.ColorScheme,
                    FrameRate = request.FrameRate,
                    Model = request.Model,
                    Resolution = request.Resolution,
                    Status = GenerationStatuses.VALIDATION,
                    Text = request.Text,
                    UserId = userId,
                    VideoId = Guid.NewGuid()
                });
            if (await _unitOfWork.SaveAsync())
            {
                var item = pipelineItem.Entity;
                _pipeline.Add(item.Id);
                var promptServiceTopicConfig = _configuration.GetSection("promptServiceTopicConfig");
                if (await _producer.ProduceAsync(
                        new TopicConfig()
                        {
                            PartitionsCount = Convert.ToInt32(promptServiceTopicConfig["partitionsCount"]),
                            ReplicationFactor = Convert.ToInt16(promptServiceTopicConfig["replicationFactor"]),
                            TopicName = promptServiceTopicConfig["topicName"]
                        },
                        1,
                        new Message<string, string>()
                        {
                            Key = item.Id.ToString(),
                            Value = JsonConvert.SerializeObject(
                                new BeginGenerationRequest()
                                {
                                    PipelineId = item.Id,
                                    ColorScheme = item.ColorScheme,
                                    FrameRate = item.FrameRate,
                                    Model = item.Model,
                                    Resolution = item.Resolution,
                                    Text = item.Text,
                                    VideoId = item.VideoId
                                })
                        }))
                {
                    return new BeginVideoGenerationResponse()
                    {
                        PipelineId = item.VideoId
                    };
                }
                
                throw new Exception("Producing message went wrong!");
            }
            throw new Exception("Adding pipeline went wrong!");
        }
        catch (Exception e)
        {
            _logger.LogError(e,e.Message);
            throw;
        }
    }
    [KafkaSimpleMethod("updateStatus",false)]
    public async Task UpdateStatus(UpdateStatusRequest request)
    {
        try
        {
            if (_pipeline.Contains(request.TaskId) && request.Status == GenerationStatuses.SUCCESS)
            {
                _pipeline.Remove(request.TaskId);
            }

            using (ApplicationContext context = new ApplicationContext(_configuration))
            {
                var pipelineItem = context.Letters.FirstOrDefault(x => x.Id == request.TaskId);
                
                if (request.Status == GenerationStatuses.SUCCESS)
                {
                    VideosCringeCommunicator communicator = new VideosCringeCommunicator(new MicroservicesCringeHttpClient(new HttpClient()), _configuration);
                    await communicator.SendAddVideoRequest(new AddVideoRequest()
                    {
                        BucketId = pipelineItem.Id.ToString(),
                        AuthorId = pipelineItem.UserId,
                        BuckedObjectId = pipelineItem.VideoId
                    });
                    pipelineItem.EndTime = DateTime.UtcNow;
                
                }

                if (pipelineItem.Status < request.Status)
                {
                    pipelineItem.Status = request.Status;
                    context.Letters.Update(pipelineItem);
                }
            
                context.SaveChanges();
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e,e.Message);
            throw;
        }
    }

    public async Task<GetStatusResponse> GetStatus(Guid taskId)
    {
        try
        {
            var pipelineItem = await _unitOfWork.PipelineRepository.Get().FirstOrDefaultAsync(x => x.VideoId ==taskId);
            int progres = 0;
            string status = "";
            int eta = 0;
            if (pipelineItem.EndTime != null && pipelineItem.BeginTime != null)
            {
                TimeSpan duration = pipelineItem.EndTime.Value - pipelineItem.BeginTime;
                eta = Convert.ToInt32(duration.TotalSeconds); 
            }
            else
            {
                TimeSpan duration = DateTime.Now - pipelineItem.BeginTime;
                eta = Convert.ToInt32(duration.TotalSeconds); 
            }
            switch (pipelineItem.Status)
            {
                case GenerationStatuses.SUCCESS:
                    progres = 100;
                    status = "SUCCESS";
                    break;
                case GenerationStatuses.VALIDATION:
                    progres = 0;
                    status = "VALIDATION";  
                    break;
                case GenerationStatuses.ANALYZE_LETTER:
                    progres = 1;
                    status = "ANALYZE_LETTER";
                    break;
                case GenerationStatuses.CREATING_IMAGES:
                    progres = 5;
                    status = "CREATING_IMAGES";
                    break;
                case GenerationStatuses.CREATING_AUDIO:
                    progres = 55;
                    status = "CREATING_AUDIO";
                    break;
                case GenerationStatuses.MAKING_VIDEOS:
                    progres = 58;
                    status = "MAKING_VIDEOS";
                    break;
                case GenerationStatuses.ADD_SOUND:
                    progres = 78;
                    status = "ADD_SOUND";
                    break;
                case GenerationStatuses.MERGE_VIDEOS:
                    progres = 89;
                    status = "MERGE_VIDEOS";
                    break;
                case GenerationStatuses.FINAL_PROCESS:
                    progres = 99;
                    status = "FINAL_PROCESS";
                    break;
                case GenerationStatuses.WAITING:
                    progres = 0;
                    status = "WAITING";
                    break;
                case GenerationStatuses.ERROR:
                    progres = 0;
                    status = "ERROR";
                    break;
            }

            return new GetStatusResponse()
            {
                Status = status,
                Eta = eta,
                Progres = progres
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,e.Message);
            throw;
        }
    }

    public int GetQueuePosition(Guid taskId)
    {
        var pipelineItem =  _unitOfWork.PipelineRepository.Get().FirstOrDefault(x => x.VideoId ==taskId);

        if (!_pipeline.Contains(pipelineItem.Id))
        {
            return -1;
        }

        return _pipeline.IndexOf(pipelineItem.Id) + 1;
    }

    #endregion
}