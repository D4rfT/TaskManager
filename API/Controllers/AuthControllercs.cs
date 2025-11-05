using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManager.Domain.Entities;
using TaskManager.Infra.Data.Context;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                if (string.IsNullOrWhiteSpace(registerDto.UserName))
                    return BadRequest(new { message = "Nome de usuário é obrigatório" });

                if (string.IsNullOrWhiteSpace(registerDto.Email))
                    return BadRequest(new { message = "Email é obrigatório" });

                if (string.IsNullOrWhiteSpace(registerDto.Password))
                    return BadRequest(new { message = "Senha é obrigatória" });

                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                    return BadRequest(new { message = "Usuário já existe com este email" });

                var user = new User(registerDto.UserName, registerDto.Email);
                user.CreatePasswordHash(registerDto.Password);

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
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                    return BadRequest(new { message = "Email e senha são obrigatórios" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                if (user == null)
                    return Unauthorized(new { message = "Login ou senha inválidos" });

                if (!user.VerifyPasswordHash(loginDto.Password))
                    return Unauthorized(new { message = "Login ou senha inválidos" });

                var accessToken = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

 
                user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
                await _context.SaveChangesAsync();

                return Ok(new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
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

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                    return BadRequest(new { message = "Refresh token é obrigatório" });

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

                if (user == null || !user.IsRefreshTokenValid(request.RefreshToken))
                    return Unauthorized(new { message = "Refresh token inválido ou expirado" });

                var newAccessToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
                await _context.SaveChangesAsync();

                return Ok(new AuthResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
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

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    user.SetRefreshToken(string.Empty, DateTime.UtcNow);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Logout realizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }


        /// Gera um JWT (JSON Web Token) de acesso para o usuário autenticado.
        /// O token contém as claims do usuário e expira em 2 horas.
        /// Retorna string contendo o JWT token codificado pronto para uso
        /// 
        /// Detalhes da implementação:
        /// - Usa HMAC-SHA256 para assinatura do token
        /// - Inclui claims: ID, Nome de usuário e Email
        /// - Tempo de expiração: 2 horas a partir do momento atual (UTC)
        /// - Configura Issuer e Audience baseados nas configurações da aplicação
        /// - A chave de assinatura é obtida das configurações (appsettings.json)
        /// 
        /// Estrutura do token:
        /// - ClaimTypes.NameIdentifier: ID do usuário
        /// - ClaimTypes.Name: Nome de usuário
        /// - ClaimTypes.Email: Email do usuário

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // Obtém a chave secreta das configurações e converte para array de bytes
            // A chave é usada para assinar o token e verificar sua autenticidade
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Define a identidade do token com as claims (informações) do usuário
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                // Define as credenciais de assinatura usando a chave secreta
                SigningCredentials = new SigningCredentials(
                    // Cria uma chave simétrica a partir dos bytes e define o algoritmo de assinatura como HMAC com SHA256
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            // Cria o token JWT baseado na descrição fornecida
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Converte o token para uma string formatada (xxxxx.yyyyy.zzzzz)
            return tokenHandler.WriteToken(token);
        }


        /// Gera um refresh token criptograficamente seguro para renovação de autenticação.
        /// Refresh tokens têm validade mais longa (7 dias) e são usados para obter
        /// novos tokens de acesso sem exigir nova autenticação do usuário.
        /// Retorna String Base64 contendo um token aleatório seguro de 64 bytes
        /// 
        /// Características de segurança:
        /// - Gera 64 bytes de dados aleatórios criptograficamente fortes
        /// - Usa RandomNumberGenerator para garantir aleatoriedade imprevisível
        /// - Converte para Base64 para facilitar armazenamento e transmissão
        /// - Tamanho final: ~88 caracteres Base64
        /// 
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            // Converte os bytes aleatórios para uma string em Base64
            // Base64 é usado porque é seguro para URLs e fácil de armazenar
            return Convert.ToBase64String(randomNumber);
        }
    }
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserResponse User { get; set; } = new UserResponse();
    }

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

    public class UserResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}