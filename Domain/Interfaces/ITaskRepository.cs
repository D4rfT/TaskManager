using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem> AddAsync(TaskItem task);
        Task<TaskItem?> GetByIdAsync(int id);
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(int id);


        // Operações específicas de domínio
        Task<IEnumerable<TaskItem>> GetCompletedAsync();
        Task<IEnumerable<TaskItem>> GetPendingAsync();
        Task<IEnumerable<TaskItem>> GetOverdueAsync();
    }
}