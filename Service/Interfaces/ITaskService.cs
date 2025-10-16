using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;

namespace AppServices.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItem> CreateTaskAsync(string title, string description, DateTime dueDate);
        Task UpdateTaskAsync(int id, string title, string description, DateTime dueDate);
        Task DeleteTaskAsync(int id);
        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<IEnumerable<TaskItem>> GetAllTasksAsync();
        Task<IEnumerable<TaskItem>> GetCompletedTasksAsync();
        Task<IEnumerable<TaskItem>> GetPendingTasksAsync();
        Task<IEnumerable<TaskItem>> GetOverdueTasksAsync();

    }
}
