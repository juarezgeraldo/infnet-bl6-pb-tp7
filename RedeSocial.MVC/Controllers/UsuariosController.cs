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

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, login.NomeUsuario)
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        // Refreshing the authentication session should be allowed.

                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                        // The time at which the authentication ticket expires. A 
                        // value set here overrides the ExpireTimeSpan option of 
                        // CookieAuthenticationOptions set with AddCookie.

                        IsPersistent = true,
                        // Whether the authentication session is persisted across 
                        // multiple requests. When used with cookies, controls
                        // whether the cookie's lifetime is absolute (matching the
                        // lifetime of the authentication ticket) or session-based.

                        //IssuedUtc = <DateTimeOffset>,
                        // The time at which the authentication ticket was issued.

                        //RedirectUri = <string>
                        // The full path or absolute URI to be used as an http 
                        // redirect response value.
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
        public IActionResult Detalhes(int id)
        {
            return View();
        }


        // POST: UsuariosController/Adicionar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar([Bind("NomeUsuario,SenhaUsuario,EmailUsuario,TelefoneUsuario,NomeCompletoUsuario,PerfilId")] Usuario usuario)
        {
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
                        if (erro.Code.Contains(nameof(usuario.NomeUsuario)))
                        {
                            ModelState.AddModelError(nameof(usuario.NomeUsuario), erro.Description);
                        }
                        else if (erro.Code.ToLowerInvariant().Contains("password"))
                        {
                            ModelState.AddModelError(nameof(usuario.SenhaUsuario), erro.Description);
                        }
                        else if (erro.Code.Contains(nameof(usuario.EmailUsuario)))
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

                    return View(usuario);
                }
            }

            return View(usuario);
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
    }
}
