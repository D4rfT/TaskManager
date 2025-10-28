using System.Security.Cryptography;
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
        public DateTime CreatedAt { get; private set; }
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

        public void CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                PasswordSalt = Convert.ToBase64String(hmac.Key);
                PasswordHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
            }
        }

        public bool VerifyPasswordHash(string password)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(PasswordSalt)))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(Convert.FromBase64String(PasswordHash));
            }
        }
    }
}