using Confluent.Kafka;
using Newtonsoft.Json;
using PipelineService.Autumn.Attributes.MethodAttributes;
using PipelineService.Autumn.Attributes.ServiceAttributes;
using PipelineService.Autumn.Utils;
using PipelineService.Autumn.Utils.Models;
using PipelineService.Communicators;
using PipelineService.Database.Repositories;
using PipelineService.Models.Database;
using PipelineService.Models.Internal.Kafka.Requests;
using PipelineService.Models.Internal.Requests;
using PipelineService.Models.Requests;
using PipelineService.Models.Responses;
using PipelineService.Utils;

namespace PipelineService.Services.Pipeline;

[KafkaSimpleService("statusUpdates",3,2,MessageHandlerType.JSON,"PipelineService")]
public class PipelineService : IPipelineService
{
    #region Fields

    private readonly ILogger<PipelineService> _logger;
    private readonly KafkaProducer _producer;
    private readonly VideosCommunicator _communicator;
    private readonly UnitOfWork _unitOfWork;
    private List<Guid> _pipeline;
    private IConfiguration _configuration;
    
    #endregion

    #region Constructor

    public PipelineService(ILogger<PipelineService> logger, KafkaProducer producer, VideosCommunicator communicator, UnitOfWork unitOfWork, IConfiguration configuration)
    {
        _logger = logger;
        _producer = producer;
        _communicator = communicator;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _pipeline = new List<Guid>();
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
                    BeginTime = DateTime.Now,
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
    [KafkaMethod("updateStatus",1,false)]
    public async Task UpdateStatus(UpdateStatusRequest request)
    {
        try
        {
            if (_pipeline.Contains(request.TaskId))
            {
                _pipeline.Remove(request.TaskId);
            }

            var pipelineItem = await _unitOfWork.PipelineRepository.GetByIDAsync(request.TaskId);

          
            if (request.Status == GenerationStatuses.SUCCESS)
            {
                
                await _communicator.SendAddVideoRequest(new AddVideoRequest()
                {
                    BucketId = pipelineItem.Id.ToString(),
                    AuthorId = pipelineItem.UserId,
                    BuckedObjectId = pipelineItem.VideoId
                });
                pipelineItem.EndTime = DateTime.Now;
                
            }
            pipelineItem.Status = request.Status;
        
            _unitOfWork.PipelineRepository.Update(pipelineItem);
            
            await _unitOfWork.SaveAsync();
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
            var pipelineItem = await _unitOfWork.PipelineRepository.GetByIDAsync(taskId);
            int progres = 0;
            int eta = 0;
            if (pipelineItem.EndTime != null)
            {
                eta = Convert.ToInt32(pipelineItem.EndTime - pipelineItem.BeginTime);
                
            }
            else
            {
                eta = Convert.ToInt32(DateTime.Now - pipelineItem.BeginTime);
            }
            switch (pipelineItem.Status)
            {
                case GenerationStatuses.SUCCESS:
                    progres = 100;
                    break;
                case GenerationStatuses.VALIDATION:
                    progres = 0;
                    break;
                case GenerationStatuses.ANALYZE_LETTER:
                    progres = 1;
                    break;
                case GenerationStatuses.CREATING_IMAGES:
                    progres = 5;
                    break;
                case GenerationStatuses.CREATING_AUDIO:
                    progres = 55;
                    break;
                case GenerationStatuses.MAKING_VIDEOS:
                    progres = 58;
                    break;
                case GenerationStatuses.ADD_SOUND:
                    progres = 78;
                    break;
                case GenerationStatuses.MERGE_VIDEOS:
                    progres = 89;
                    break;
                case GenerationStatuses.FINAL_PROCESS:
                    progres = 99;
                    break;
                case GenerationStatuses.WAITING:
                    progres = 0;
                    break;
                case GenerationStatuses.ERROR:
                    progres = 0;
                    break;
            }

            return new GetStatusResponse()
            {
                Status = pipelineItem.Status,
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
        if (!_pipeline.Contains(taskId))
        {
            return -1;
        }

        return _pipeline.IndexOf(taskId);
    }

    #endregion
}