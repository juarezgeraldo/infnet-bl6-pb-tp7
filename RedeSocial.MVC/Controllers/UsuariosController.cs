using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using NuGet.Common;
using RedeSocial.BLL.Models;
using static System.Net.WebRequestMethods;
using RedeSocial.MVC.Models;
using RedeSocial.MVC.Helpers;

namespace RedeSocial.MVC.Controllers
{
    public class UsuariosController : Controller
    {
        // GET: UsuariosController
        public IActionResult Login(string ReturnUrl = "/")
        {
            LoginModel objLoginModel = new LoginModel();
            objLoginModel.ReturnUrl = ReturnUrl;
            return View(objLoginModel);
        }

        // POST: UsuariosController/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("NomeUsuario,SenhaUsuario,ReturnUrl")] LoginModel login)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await "https://localhost:7200/api/Autorizacao/Login"
                        .PostJsonAsync(login)
                        .ReceiveJson<TokenModel>();

                    HttpContext.Response.Cookies.Append("token", response.Token,
                        new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTime.Now.AddMinutes(10) });

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, login.NomeUsuario)
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                        IsPersistent = true,
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return LocalRedirect(login.ReturnUrl);
                }
                catch (FlurlHttpException ex)
                {
                    var error = await ex.GetResponseJsonAsync<IdentityErrorModel>();

                    if (error == null)
                    {
                        return View();
                    }

                    ViewBag.ErrorMessage = error.Mensagem;

                    return View(login);
                }
            }

            return View(login);
        }

        [Authorize]
        // GET: UsuariosController/Detalhes/5
        public async Task<IActionResult> Detalhes()
        {
            var usuario = await BuscarDetalhesUsuario();

            return View(usuario);
        }

        private async Task<Usuario> BuscarDetalhesUsuario()
        {
            var nomeUsuario = User.Identity.GetName();

            var usuario = await "https://localhost:7200/api/Autorizacao/GetUsuario"
                .SetQueryParam("nomeUsuario", nomeUsuario)
                .GetJsonAsync<Usuario>();
            return usuario;
        }

        public async Task<IActionResult> Adicionar(AdicionarUsuarioViewModel adicionarUsuarioViewModel)
        {
            adicionarUsuarioViewModel.Perfis = await BuscarPerfis();

            return View(adicionarUsuarioViewModel);
        }

        // POST: UsuariosController/Adicionar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar([Bind("NomeUsuario,SenhaUsuario,EmailUsuario,TelefoneUsuario,NomeCompletoUsuario,PerfilId")] Usuario usuario)
        {
            var adicionarUsuarioViewModel = new AdicionarUsuarioViewModel();
            adicionarUsuarioViewModel.Usuario = usuario;

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await "https://localhost:7200/api/Autorizacao/Registrar"
                        .PostJsonAsync(usuario);

                    return RedirectToAction(nameof(Login));
                }
                catch(FlurlHttpException ex)
                {
                    var error = await ex.GetResponseJsonAsync<IdentityErrorModel>();

                    if (error == null)
                    {
                        return View();
                    }

                    foreach (var erro in error.Erros)
                    {
                        if (erro.Code.ToLowerInvariant().Contains("Username"))
                        {
                            ModelState.AddModelError(nameof(usuario.NomeUsuario), erro.Description);
                        }
                        else if (erro.Code.ToLowerInvariant().Contains("password"))
                        {
                            ModelState.AddModelError(nameof(usuario.SenhaUsuario), erro.Description);
                        }
                        else if (erro.Code.ToLowerInvariant().Contains("Email"))
                        {
                            ModelState.AddModelError(nameof(usuario.EmailUsuario), erro.Description);
                        }
                        else if (erro.Code.Contains(nameof(usuario.NomeCompletoUsuario)))
                        {
                            ModelState.AddModelError(nameof(usuario.NomeCompletoUsuario), erro.Description);
                        }
                        else if (erro.Code.Contains(nameof(usuario.TelefoneUsuario)))
                        {
                            ModelState.AddModelError(nameof(usuario.TelefoneUsuario), erro.Description);
                        }
                    }

                    ViewBag.ErrorMessage = error.Mensagem;
                    adicionarUsuarioViewModel.Perfis = await BuscarPerfis();
                    return View(adicionarUsuarioViewModel);
                }
            }

            return View(adicionarUsuarioViewModel);
        }

        public async Task<IActionResult> RecuperarSenha()
        {
            return View();
        }

        // POST: UsuariosController/RecuperarSenha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarSenha([Bind("NomeUsuario,SenhaUsuario")] CredencialLogin credenciais)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await "https://localhost:7200/api/Autorizacao/AlterarSenha"
                        .PostJsonAsync(credenciais);

                    return RedirectToAction(nameof(Login));
                }
                catch (FlurlHttpException ex)
                {
                    var error = await ex.GetResponseJsonAsync<AlterarSenhaErroViewModel>();

                    if (error == null)
                    {
                        return View();
                    }

                    
                    ViewBag.ErrorMessage = error.Mensagem;
                    return View();
                }
            }

            return View();
        }

        private static async Task<IEnumerable<Perfil>> BuscarPerfis()
        {
            return await "https://localhost:7200/api/Perfis"
                .GetJsonAsync<IEnumerable<Perfil>>();
        }

        // GET: UsuariosController
        public IActionResult Logout()
        {
            return View();
        }

        // POST: UsuariosController/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(bool logout)
        {
            try
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(nameof(Login));
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync<IdentityErrorModel>();

                if (error == null)
                {
                    return View();
                }

                ViewBag.ErrorMessage = error.Mensagem;

                return View();
            }
        }

        public async Task<IActionResult> AlterarPerfil(AlterarPerfilViewModel alterarPerfilViewModel)
        {
            var usuario = await BuscarDetalhesUsuario();

            alterarPerfilViewModel.Perfis = await BuscarPerfis();
            alterarPerfilViewModel.AlterarPerfil = new AlterarPerfilDto()
            {
                PerfilId = usuario.Perfil.PerfilId
            };

            return View(alterarPerfilViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarPerfil([Bind("PerfilId")] AlterarPerfilDto alterarPerfil)
        {
            var alterarPerfilViewModel = new AlterarPerfilViewModel();

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await "https://localhost:7200/api/Autorizacao/AlteraPerfil"
                        .WithOAuthBearerToken(Request.Cookies["token"])
                        .PostJsonAsync(alterarPerfil);

                    return RedirectToAction(nameof(Detalhes));
                }
                catch (FlurlHttpException ex)
                {
                    alterarPerfilViewModel.Perfis = await BuscarPerfis();
                    var error = await ex.GetResponseJsonAsync<AlterarSenhaErroViewModel>();

                    if (error == null)
                    {
                        return View(alterarPerfilViewModel);
                    }

                    ViewBag.ErrorMessage = error.Mensagem;
                    
                    return View(alterarPerfilViewModel);
                }
            }

            return View(alterarPerfilViewModel);
        }

    }
}
