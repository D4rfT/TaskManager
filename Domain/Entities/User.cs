using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;

namespace Domain.Entities
{
    public class User
    {

        public int Id { get; private set; }
        public string UserName { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string PasswordSalt { get; private set; }
        public DateTime CreatedAt {get; private set;}
        public DateTime UpdatedAt { get; private set; }
        public ICollection<TaskItem> Tasks { get; private set; }

        private User() { }

        public User(string userName, string email)
        {
            UserName = userName;
            Email = email;
            PasswordHash = string.Empty;
            PasswordSalt = string.Empty;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Tasks = new List<TaskItem>(); 
        }

    }
}
