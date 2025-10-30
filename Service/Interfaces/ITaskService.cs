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
            Task<TaskItem> CreateTaskAsync(string title, string description, DateTime dueDate, int userId);
            Task UpdateTaskAsync(int id, string title, string description, DateTime dueDate, int userId);
            Task DeleteTaskAsync(int id, int userId);
            Task<TaskItem?> GetTaskByIdAsync(int id, int userId);
            Task<IEnumerable<TaskItem>> GetAllTasksAsync(int userId);
            Task<IEnumerable<TaskItem>> GetCompletedTasksAsync(int userId);
            Task<IEnumerable<TaskItem>> GetPendingTasksAsync(int userId);
            Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(int userId);
        }

}
