using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem> AddAsync(TaskItem task);
        Task<TaskItem?> GetByIdAsync(int id, int userId);
        Task<IEnumerable<TaskItem>> GetAllAsync(int userId);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(int id, int userId);


        // Operações específicas de domínio
        Task<IEnumerable<TaskItem>> GetCompletedAsync(int userId);
        Task<IEnumerable<TaskItem>> GetPendingAsync(int userId);
        Task<IEnumerable<TaskItem>> GetOverdueAsync(int userId);
    }
}