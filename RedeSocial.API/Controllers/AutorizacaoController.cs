using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RedeSocial.API.Configuration;
using RedeSocial.API.Model;
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
        private readonly UserManager<IdentityUser> _userManager;

        public AutorizacaoController(IOptions<JwtBearerTokenSettings> jwtTokenOptions, UserManager<IdentityUser> userManager)
        {
            this._jwtTokenSettings = jwtTokenOptions.Value;
            this._userManager = userManager;
        }

        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] Usuario usuario)
        {
            if(!ModelState.IsValid || usuario == null)
            {
                return new BadRequestObjectResult(new { Mensagem = "Registro do Usuário não efetuado." });
            }

            var identidadeUsuario = new IdentityUser() { UserName = usuario.NomeUsuario, Email = usuario.EmailUsuario };
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
            IdentityUser identidadeUsuario;

            if (!ModelState.IsValid || 
                credencialLogin == null || 
                (identidadeUsuario = await ValidarUsuario(credencialLogin)) == null)
            {
                return new BadRequestObjectResult(new { Mensagem = "Não foi possível realizar o login do Usuaário." });
            }

            var token = GerarToken(identidadeUsuario);

            return Ok(new { Token = token, Mensagem = "Login do Usuário realizado com sucesso." });
        }

        private object GerarToken(IdentityUser identityUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtTokenSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, identityUser.UserName.ToString()),
                    new Claim(ClaimTypes.Email, identityUser.Email)
                }),
                Expires = DateTime.UtcNow.AddSeconds(_jwtTokenSettings.ExpiryTimeInSeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _jwtTokenSettings.Audience,
                Issuer = _jwtTokenSettings.Issuer
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<IdentityUser> ValidarUsuario(CredencialLogin credencialLogin)
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
    }
}
