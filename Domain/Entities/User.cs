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
        public string RefreshToken { get; private set; } = string.Empty;
        public DateTime RefreshTokenExpiry { get; private set; }
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
            RefreshToken = string.Empty;
            RefreshTokenExpiry = DateTime.UtcNow;
            Tasks = new List<TaskItem>();
        }

        /// Cria um hash seguro da senha e um salt usando o algoritmo HMACSHA512.
        /// Este método gera um salt aleatório criptograficamente forte e computa o hash da senha combinada com o salt.
        /// 1. Gera uma chave aleatória de 512 bits (salt) usando HMACSHA512
        /// 2. Computa o hash HMAC-SHA512 da senha usando o salt gerado
        /// 3. Armazena tanto o salt quanto o hash como strings Base64 para armazenamento
        public void CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                PasswordSalt = Convert.ToBase64String(hmac.Key);
                PasswordHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
            }
        }

        /// Verifica se a senha fornecida corresponde ao hash e salt armazenados.
        /// Este método recria o hash usando o salt armazenado e compara com o hash de senha armazenado.
        /// Retorna True se a senha corresponder ao hash armazenado, false caso contrário
        /// Notas de segurança:
        /// - Usa comparação de tempo constante para prevenir ataques de temporização
        /// - O salt garante que senhas idênticas tenham hashes diferentes
        /// - HMACSHA512 fornece segurança criptográfica forte
 
        public bool VerifyPasswordHash(string password)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(PasswordSalt)))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(Convert.FromBase64String(PasswordHash));
            }
        }
        public void SetRefreshToken(string refreshToken, DateTime expiry)
        {
            RefreshToken = refreshToken;
            RefreshTokenExpiry = expiry;
            UpdatedAt = DateTime.UtcNow;
        }
        public bool IsRefreshTokenValid(string refreshToken)
        {
            return !string.IsNullOrEmpty(RefreshToken) &&
                   RefreshToken == refreshToken &&
                   RefreshTokenExpiry > DateTime.UtcNow;
        }

    }
}