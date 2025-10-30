using Domain.Entities;
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

        public async Task<TaskItem?> GetByIdAsync(int id, int userId)
        {
                return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync(int userId)
        {
                return await _context.Tasks.Where(t => t.UserId == userId).ToListAsync();
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

        public async Task DeleteAsync(int id, int userId)
        {      
                var task = await GetByIdAsync(id ,userId);
                if(task != null)
                {
                    _context.Tasks.Remove(task);
                    await _context.SaveChangesAsync();
                }
        }

        public async Task<IEnumerable<TaskItem>> GetCompletedAsync(int userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId && t.IsCompleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetPendingAsync(int userId)
        {
            return await _context.Tasks
                .Where(t =>t.UserId == userId && !t.IsCompleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetOverdueAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _context.Tasks
                .Where(t =>t.UserId == userId && !t.IsCompleted && t.DueDate < now)
                .ToListAsync();
        }

    }
}
