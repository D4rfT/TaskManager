using AppServices.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace AppServices.Services
{
    public class TaskService:ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<TaskItem> CreateTaskAsync(string title, string description, DateTime dueDate, int userId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Título não pode ser nulo ou vazio", nameof(title));

            if (dueDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Data de vencimento não pode ser no passado", nameof(dueDate));

            var task = new TaskItem(title, description, dueDate, userId);

            return await _taskRepository.AddAsync(task);
        }

        public async Task UpdateTaskAsync(int id, string title, string description, DateTime dueDate, int userId)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero", nameof(id));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Título não pode ser nulo ou vazio", nameof(title));

            if (dueDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Data de vencimento não pode ser no passado", nameof(dueDate));

            var existingTask = await GetTaskByIdAsync(id, userId);

            existingTask.Update(title, description, dueDate);

            await _taskRepository.UpdateAsync(existingTask);
        }

        public async Task DeleteTaskAsync(int id, int userId)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero", nameof(id));

            var existingTask = await GetTaskByIdAsync(id, userId);

            await _taskRepository.DeleteAsync(id, userId);

        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id, int userId)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero", nameof(id));

            var task = await _taskRepository.GetByIdAsync(id, userId);

            if (task == null)
                throw new KeyNotFoundException($"Task com o ID {id} não encontrada");

            return task;
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(int userId)
        {
            var tasks = await _taskRepository.GetAllAsync(userId);

            if (!tasks.Any())
                throw new InvalidOperationException("No tasks found");

            return tasks;
        }

        public async Task<IEnumerable<TaskItem>> GetCompletedTasksAsync(int userId)
        {
            var completedTasks = await _taskRepository.GetCompletedAsync(userId);

            if (!completedTasks.Any())
                throw new InvalidOperationException("Nenhuma task completa encontrada");

            return completedTasks;
        }

        public async Task<IEnumerable<TaskItem>> GetPendingTasksAsync(int userId)
        {
            var pendingTasks = await _taskRepository.GetPendingAsync(userId);

            if (!pendingTasks.Any())
                throw new InvalidOperationException("Nenhuma task pendente encontrada");

            return pendingTasks.Where(t => !t.IsOverdue()).ToList();
        }

        public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(int userId)
        {
            var overdueTasks = await _taskRepository.GetOverdueAsync(userId);

            if (!overdueTasks.Any())
                throw new InvalidOperationException("Nenhuma task atrasada encontrada");

            return overdueTasks;
        }
    }
}
