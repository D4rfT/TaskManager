using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Infra.Data.Context;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TaskContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(TaskContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                // Validar dados de entrada
                if (string.IsNullOrWhiteSpace(registerDto.UserName))
                    return BadRequest(new { message = "Nome de usuário é obrigatório" });

                if (string.IsNullOrWhiteSpace(registerDto.Email))
                    return BadRequest(new { message = "Email é obrigatório" });

                if (string.IsNullOrWhiteSpace(registerDto.Password))
                    return BadRequest(new { message = "Senha é obrigatória" });

                // Verificar se usuário já existe
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                    return BadRequest(new { message = "Usuário já existe com este email" });

                // Criar novo usuário
                var user = new User(registerDto.UserName, registerDto.Email);

                user.CreatePasswordHash(registerDto.Password);

                // Salvar usuário no banco
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Usuário criado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Validar dados de entrada
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                    return BadRequest(new { message = "Email e senha são obrigatórios" });

                // Buscar usuário pelo email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                if (user == null)
                    return Unauthorized(new { message = "Credenciais inválidas" });

                if (!user.VerifyPasswordHash(loginDto.Password))
                    return Unauthorized(new { message = "Credenciais inválidas" });

                // Gerar token JWT
                var token = GenerateJwtToken(user);

                return Ok(new AuthResponse
                {
                    Token = token,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    // DTOs
    public class RegisterDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserResponse User { get; set; } = new UserResponse();
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}