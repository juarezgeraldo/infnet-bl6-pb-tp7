using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using RedeSocial.API.Configuration;
using RedeSocial.BLL.Models;
using RedeSocial.DAL.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
        private readonly ApplicationDbContext _context;


        public AutorizacaoController(IOptions<JwtBearerTokenSettings> jwtTokenOptions, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            this._jwtTokenSettings = jwtTokenOptions.Value;
            this._userManager = userManager;
            _context = context;

        }

        [HttpPost]
        [Route("AlterarSenha")]
        public async Task<ActionResult> AlterarSenha([FromBody] CredencialLogin credencialLogin)
        {
            var identidadeUsuario = await _userManager.FindByNameAsync(credencialLogin.NomeUsuario);

            if (identidadeUsuario != null)
            {

                identidadeUsuario.PasswordHash = _userManager.PasswordHasher.HashPassword(identidadeUsuario, credencialLogin.SenhaUsuario);
                var resultado = await _userManager.UpdateAsync(identidadeUsuario);
                return NoContent();
            }
            return BadRequest(new BadRequestObjectResult(new { Mensagem = "Usuário inválido." }));
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
                PerfilId = usuario.Perfil.PerfilId
            };

            var resultado = await _userManager.CreateAsync(identidadeUsuario, usuario.SenhaUsuario);

            if (!resultado.Succeeded)
            {
                return new BadRequestObjectResult(new { Mensagem = "Não foi possível registrar Usuario.", Erros = resultado.Errors });
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
                return new BadRequestObjectResult(new { Mensagem = "Não foi possível realizar o login do Usuário." });
            }

            var token = GerarToken(identidadeUsuario);

            return Ok(new { Token = token, nomeUsuario = credencialLogin.NomeUsuario, Mensagem = "Login do Usuário realizado com sucesso." });
        }
        [HttpGet]
        [Route("GetUsuario")]
        public async Task<ActionResult<Usuario>> GetUsuario(string nomeUsuario)
        {
            var identidadeUsuario = await _userManager.FindByNameAsync(nomeUsuario);
            if (identidadeUsuario != null)
            {
                var usuario = new Usuario();
                usuario.NomeUsuario = identidadeUsuario.UserName;
                usuario.EmailUsuario = identidadeUsuario.Email;
                usuario.TelefoneUsuario = identidadeUsuario.PhoneNumber;
                usuario.NomeCompletoUsuario = identidadeUsuario.NomeCompleto;
                usuario.Perfil = await _context.Perfis.FindAsync(identidadeUsuario.PerfilId);

                return usuario;
            }
            return null;
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
        [Route("AlteraPerfil")]
        [Authorize]
        public async Task<IActionResult> AlteraPerfil([FromBody] AlterarPerfilDto perfilId)
        {
            var nomeUsuario = User.Identity.Name;

            var identidadeUsuario = await _userManager.FindByNameAsync(nomeUsuario);
            if (identidadeUsuario != null)
            {
                identidadeUsuario.PerfilId = perfilId.PerfilId;

                var resultado = await _userManager.UpdateAsync(identidadeUsuario);

                if (!resultado.Succeeded)
                {
                    return new BadRequestObjectResult(new { Mensagem = "Não foi possível altualizar Perfil.", Erros = resultado.Errors });
                }
            }
            return Ok(new { Mensagem = "Perfil do Usuario atualizado com sucesso." });
        }


        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok(new { Token = "", Message = "Logged Out" });
        }

        [HttpDelete]
        [Route("Excluir")]
        public async Task<IActionResult> Excluir(string emailUsuario)
        {
            if (!ModelState.IsValid || emailUsuario == null)
            {
                return new BadRequestObjectResult(new { Mensagem = "Exclusão do Usuário não efetuada." });
            }

            Task<ApplicationUser> applicationUser = BuscarUsuario(emailUsuario);

            if (applicationUser == null)
            {
                return new BadRequestObjectResult(new { Mensagem = "Exclusão do Usuário não efetuada." });
            }

            var resultado = await _userManager.DeleteAsync(applicationUser.Result);

            if (!resultado.Succeeded)
            {
                var dicionario = new ModelStateDictionary();
                foreach (IdentityError erro in resultado.Errors)
                {
                    dicionario.AddModelError(erro.Code, erro.Description);
                }
                return new BadRequestObjectResult(new { Mensagem = "Não foi possível Excluir Usuario.", Erros = dicionario });
            }
            return Ok(new { Mensagem = "Usuário excluído com sucesso." });

        }

        private async Task<ApplicationUser> BuscarUsuario(string emailUsuario)
        {
            Task<ApplicationUser> applicationUser = _userManager.FindByEmailAsync(emailUsuario);

            return await applicationUser;

        }
    }
}
