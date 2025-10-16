using API.Models;
using AppServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Domain.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll()
        {
            try
            {
                var tasks = await _taskService.GetAllTasksAsync();
                var response = tasks.Select(ConvertToTaskResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponse>> GetById(int id)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                var response = ConvertToTaskResponse(task);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _taskService.DeleteTaskAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<TaskResponse>> Create([FromBody] CreateTaskRequest request)
        {
            try
            {
                var task = await _taskService.CreateTaskAsync(request.Title, request.Description, request.DueDate);
                var response = ConvertToTaskResponse(task);
                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
        {
            try
            {
                await _taskService.UpdateTaskAsync(
                    id,
                    request.Title,
                    request.Description,
                    request.DueDate);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("completed")]
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetCompleted()
        {
            try
            {
                var tasks = await _taskService.GetCompletedTasksAsync();
                var response = tasks.Select(ConvertToTaskResponse);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new List<TaskResponse>());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetPending()
        {
            try
            {
                var tasks = await _taskService.GetPendingTasksAsync();
                var response = tasks.Select(ConvertToTaskResponse);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new List<TaskResponse>());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetOverdue()
        {
            try
            {
                var tasks = await _taskService.GetOverdueTasksAsync();
                var response = tasks.Select(ConvertToTaskResponse);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new List<TaskResponse>());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private TaskResponse ConvertToTaskResponse(TaskItem task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Status = task.IsCompleted ? "completed" :
                         task.IsOverdue() ? "overdue" : "pending"
            };
        }
    }
}