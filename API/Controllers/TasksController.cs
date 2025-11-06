using API.Models;
using AppServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Domain.Entities;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private int GetUserId()
        {
            // Busca a claim do tipo NameIdentifier
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verifica se a claim existe E se consegue converter o valor para int
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID não encontrado no token");
            }

            // Verifica se a claim existe E se consegue converter o valor para int
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll()
        {
            try
            {
                var userId = GetUserId();
                var tasks = await _taskService.GetAllTasksAsync(userId);
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
                var userId = GetUserId();
                var task = await _taskService.GetTaskByIdAsync(id, userId);
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
                var userId = GetUserId();
                await _taskService.DeleteTaskAsync(id, userId);
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
                var userId = GetUserId();
                var task = await _taskService.CreateTaskAsync(request.Title, request.Description, request.DueDate, userId);
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
                var userId = GetUserId();
                await _taskService.UpdateTaskAsync(
                    id,
                    request.Title,
                    request.Description,
                    request.DueDate,
                    userId);

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
                var userId = GetUserId();
                var tasks = await _taskService.GetCompletedTasksAsync(userId);
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
                var userId = GetUserId();
                var tasks = await _taskService.GetPendingTasksAsync(userId);
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
                var userId = GetUserId();
                var tasks = await _taskService.GetOverdueTasksAsync(userId);
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