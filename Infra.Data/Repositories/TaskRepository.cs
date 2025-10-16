using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infra.Data.Context;

namespace Infra.Data.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskContext _context;

        public TaskRepository(TaskContext context)
        {
            _context = context;
        }

        public async Task<TaskItem> AddAsync(TaskItem task)
        {
            try
            {
                await _context.Tasks.AddAsync(task);
                await _context.SaveChangesAsync();
                return task;
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("Não foi possivel adicionar a task", dbEx);
            }

        }

        public async Task<TaskItem> GetByIdAsync(int id)
        {
                return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
                return await _context.Tasks.ToListAsync();
        }

        public async Task UpdateAsync(TaskItem task)
        {
            try
            {
                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("Não foi possível atualizar a task", dbEx);
            }

        }

        public async Task DeleteAsync(int id)
        {      
                var task = await GetByIdAsync(id);
                if(task != null)
                {
                    _context.Tasks.Remove(task);
                    await _context.SaveChangesAsync();
                }
        }

        public async Task<IEnumerable<TaskItem>> GetCompletedAsync()
        {
            return await _context.Tasks
                .Where(t => t.IsCompleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetPendingAsync()
        {
            return await _context.Tasks
                .Where(t => !t.IsCompleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetOverdueAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Tasks
                .Where(t => !t.IsCompleted && t.DueDate < now)
                .ToListAsync();
        }

    }
}
