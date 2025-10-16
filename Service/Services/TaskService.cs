using AppServices.Interfaces;
using System;
using System.Collections.Generic;
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

        public async Task<TaskItem> CreateTaskAsync(string title, string description, DateTime dueDate)
        {
            int temporaryUserId = 1;

            // Validar dados de entrada
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Título não pode ser nulo ou vazio", nameof(title));

            if (dueDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Data de vencimento não pode ser no passado", nameof(dueDate));

            // Criar a entidade de domínio
            var task = new TaskItem(title, description, dueDate, temporaryUserId);

            return await _taskRepository.AddAsync(task);
        }

        public async Task UpdateTaskAsync(int id, string title, string description, DateTime dueDate)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero", nameof(id));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Título não pode ser nulo ou vazio", nameof(title));

            if (dueDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Data de vencimento não pode ser no passado", nameof(dueDate));

            // Buscar task existente
            var existingTask = await GetTaskByIdAsync(id);

            // Atualizar propriedades
            existingTask.Update(title, description, dueDate);

            await _taskRepository.UpdateAsync(existingTask);
        }

        public async Task DeleteTaskAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero", nameof(id));

            var existingTask = await GetTaskByIdAsync(id);

            await _taskRepository.DeleteAsync(id);

        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero", nameof(id));

            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                throw new KeyNotFoundException($"Task com o ID {id} não encontrada");

            return task;
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();

            if (!tasks.Any())
                throw new InvalidOperationException("No tasks found");

            return tasks;
        }

        public async Task<IEnumerable<TaskItem>> GetCompletedTasksAsync()
        {
            var completedTasks = await _taskRepository.GetCompletedAsync();

            if (!completedTasks.Any())
                throw new InvalidOperationException("Nenhuma task completa encontrada");

            return completedTasks;
        }

        public async Task<IEnumerable<TaskItem>> GetPendingTasksAsync()
        {
            var pendingTasks = await _taskRepository.GetPendingAsync();

            if (!pendingTasks.Any())
                throw new InvalidOperationException("Nenhuma task pendente encontrada");

            return pendingTasks.Where(t => !t.IsOverdue()).ToList();
        }

        public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
        {
            var overdueTasks = await _taskRepository.GetOverdueAsync();

            if (!overdueTasks.Any())
                throw new InvalidOperationException("Nenhuma task atrasada encontrada");

            return overdueTasks;
        }
    }
}
