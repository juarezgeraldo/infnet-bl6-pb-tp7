using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RedeSocial.API.Configuration;
using RedeSocial.BLL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RedeSocial.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutorizacaoController : ControllerBase
    {
        private readonly JwtBearerTokenSettings _jwtTokenSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public AutorizacaoController(IOptions<JwtBearerTokenSettings> jwtTokenOptions, UserManager<ApplicationUser> userManager)
        {
            this._jwtTokenSettings = jwtTokenOptions.Value;
            this._userManager = userManager;
        }

        [HttpGet]
        [Route("Listar")]
        public async Task<ActionResult<List<ApplicationUser>>> Listar()
        {
            return await _userManager.Users.ToListAsync();
        }
        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] Usuario usuario)
        {
            if(!ModelState.IsValid || usuario == null)
            {
                return new BadRequestObjectResult(new { Mensagem = "Registro do Usuário não efetuado." });
            }

            var identidadeUsuario = new ApplicationUser() { 
                UserName = usuario.NomeUsuario, 
                Email = usuario.EmailUsuario,
                NomeCompleto = usuario.NomeCompletoUsuario,
                PhoneNumber = usuario.TelefoneUsuario,
                PerfilId = usuario.PerfilId};

            var resultado = await _userManager.CreateAsync(identidadeUsuario, usuario.SenhaUsuario);

            if(!resultado.Succeeded)
            {
                var dicionario = new ModelStateDictionary();
                foreach (IdentityError erro in resultado.Errors)
                {
                    dicionario.AddModelError(erro.Code, erro.Description);
                }
                return new BadRequestObjectResult(new {Mensagem = "Não foi possível registrar Usuario.", Erros = dicionario });
            }
            return Ok(new { Mensagem = "Registro do Usuario concluído com sucesso." });
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] CredencialLogin credencialLogin)
        {
            ApplicationUser identidadeUsuario;

            if (!ModelState.IsValid || 
                credencialLogin == null || 
                (identidadeUsuario = await ValidarUsuario(credencialLogin)) == null)
            {
                return new BadRequestObjectResult(new { Mensagem = "Não foi possível realizar o login do Usuaário." });
            }

            var token = GerarToken(identidadeUsuario);

            return Ok(new { Token = token, Mensagem = "Login do Usuário realizado com sucesso." });
        }

        private object GerarToken(ApplicationUser applicationUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtTokenSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, applicationUser.UserName.ToString()),
                    new Claim(ClaimTypes.Email, applicationUser.Email)
                }),
                Expires = DateTime.UtcNow.AddSeconds(_jwtTokenSettings.ExpiryTimeInSeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _jwtTokenSettings.Audience,
                Issuer = _jwtTokenSettings.Issuer
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<ApplicationUser> ValidarUsuario(CredencialLogin credencialLogin)
        {
            var identidadeUsuario = await _userManager.FindByNameAsync(credencialLogin.NomeUsuario);
            if (identidadeUsuario != null)
            {
                var resultado = _userManager.PasswordHasher
                            .VerifyHashedPassword(identidadeUsuario, 
                                    identidadeUsuario.PasswordHash, 
                                    credencialLogin.SenhaUsuario);
                return resultado == PasswordVerificationResult.Failed ? null : identidadeUsuario;
            }
            return null;
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok(new { Token = "", Message = "Logged Out" });
        }
    }
}
