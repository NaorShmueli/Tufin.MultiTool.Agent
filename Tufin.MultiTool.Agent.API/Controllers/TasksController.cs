using Microsoft.AspNetCore.Mvc;
using Tufin.MultiAgentTool.Application.Tasks;
using Tufin.MultiTool.Agent.API.Contracts;
using Tufin.MultiTool.Agent.API.Mapping;

namespace Tufin.MultiTool.Agent.API.Controllers;

[ApiController]
public sealed class TasksController : ControllerBase
{
    private readonly IGetAgentTaskService
        _getAgentTaskService;

    private readonly ISubmitAgentTaskService
        _submitAgentTaskService;

    public TasksController(
        ISubmitAgentTaskService submitAgentTaskService,
        IGetAgentTaskService getAgentTaskService)
    {
        _submitAgentTaskService =
            submitAgentTaskService;

        _getAgentTaskService =
            getAgentTaskService;
    }

    /// <summary>
    ///     Submits and executes a natural-language task.
    /// </summary>
    [HttpPost("/task")]
    [ProducesResponseType(
        typeof(AgentTaskResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentTaskResponse>> Submit(
        [FromBody] SubmitTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await _submitAgentTaskService
            .SubmitAsync(
                request.Task,
                cancellationToken);

        var response =
            AgentTaskResponseMapper.Map(task);

        return Ok(response);
    }

    [HttpGet("/tasks")]
    [ProducesResponseType(
        typeof(IReadOnlyList<AgentTaskListItemResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<
        ActionResult<IReadOnlyList<AgentTaskListItemResponse>>> GetAll(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var effectiveTo = to ?? DateTimeOffset.UtcNow;
        var effectiveFrom = from ?? effectiveTo.AddDays(-7);

        if (effectiveFrom >= effectiveTo)
        {
            return BadRequest(new
            {
                error = "'from' must be earlier than 'to'."
            });
        }

        if (take is < 1 or > 200)
        {
            return BadRequest(new
            {
                error = "Take must be between 1 and 200."
            });
        }

        var tasks = await _getAgentTaskService.GetAllAsync(
            effectiveFrom,
            effectiveTo,
            take,
            cancellationToken);

        var response = tasks
            .Select(task => new AgentTaskListItemResponse(
                task.TaskId,
                task.Input,
                task.Status,
                task.CreatedAt))
            .ToArray();

        return Ok(response);
    }

    /// <summary>
    ///     Retrieves a previously executed task and its trace.
    /// </summary>
    [HttpGet("/tasks/{taskId:guid}")]
    [ProducesResponseType(
        typeof(AgentTaskResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentTaskResponse>> GetById(
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var task = await _getAgentTaskService
            .GetByIdAsync(
                taskId,
                cancellationToken);

        if (task is null)
        {
            return NotFound(
                new
                {
                    error =
                        $"Task '{taskId}' was not found."
                });
        }

        return Ok(
            AgentTaskResponseMapper.Map(task));
    }
}