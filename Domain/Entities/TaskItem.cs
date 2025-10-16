using Domain.Entities;
using System;
using Domain.Entities;

namespace TaskManager.Domain.Entities
{
    public class TaskItem
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime DueDate { get; private set; }
        public bool IsCompleted { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public int UserId { get; private set; }
        public User User { get; private set; }

        private TaskItem() { }
        public TaskItem(string title, string description, DateTime dueDate, int userId)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            else
                Title = title;

            if (description == null)
                description = string.Empty;
            else
                Description = description;

            DueDate = dueDate;
            IsCompleted = false;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
            UserId = userId;
        }

        // Métodos de domínio
        public void Update(string title, string description, DateTime dueDate)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            else
                Title = title;

            if (description == null)
                description = string.Empty;
            else
                Description = description;

            DueDate = dueDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsCompleted()
        {
            if (!IsCompleted)
            {
                IsCompleted = true; 
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void MarkAsIncomplete()
        {
            if (IsCompleted)
            {
                IsCompleted = false;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        // Validação de domínio
        public bool IsOverdue()
        {
            return !IsCompleted && DueDate < DateTime.UtcNow;
        }
    }
}