using System.Net.Http.Headers;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedeSocial.API.DTOs;
using RedeSocial.BLL.Models;
using RedeSocial.MVC.Models;

namespace RedeSocial.MVC.Controllers
{
    public class MidiasController : Controller
    {
        // GET: MidiasController
        public async Task<ActionResult> Listar()
        {
            var midias = await $"https://localhost:7200/api/Midias"
                .GetJsonAsync<IEnumerable<Midia>>();
            return View(midias);
        }

        [Authorize]
        public ActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Criar(AdicionarMidiaViewModel adicionarMidiaViewModel)
        {
            var arquivo = adicionarMidiaViewModel.Arquivo.FormFile;
            var base64 = string.Empty;

            using (var memoryStream = new MemoryStream())
            {
                arquivo.CopyTo(memoryStream);
                var fileBytes = memoryStream.ToArray();
                base64 = Convert.ToBase64String(fileBytes);
            }

            var midia = new MidiaDto()
            {
                Titulo = adicionarMidiaViewModel.Titulo,
                Base64 = base64
            };

            var response = await "https://localhost:7200/api/Midias"
                .WithOAuthBearerToken(Request.Cookies["token"])
                .PostJsonAsync(midia);

            return RedirectToAction(nameof(Listar));
        }

        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null) 
                return RedirectToAction(nameof(MinhasMidias));
            
            var midia = await BuscarMidia(id);
            return View(midia);
        }

        private async Task<Midia> BuscarMidia(int? id)
        {
            var response = await $"https://localhost:7200/api/Midias/{id}"
                .WithOAuthBearerToken(Request.Cookies["token"])
                .GetJsonAsync<Midia>();

            return response;
        }

        // POST: Pessoas/Excluir/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Apagar(int id)
        {
            var response = await $"https://localhost:7200/api/Midias/{id}"
                .WithOAuthBearerToken(Request.Cookies["token"])
                .DeleteAsync();
            
            return RedirectToAction(nameof(MinhasMidias));
        }

        // GET: MidiasController
        [Authorize]
        public async Task<ActionResult> MinhasMidias()
        {
            var midias = await "https://localhost:7200/api/Midias/ListarMidiaUsuario"
                .SetQueryParam("nomeUsuario", User.Identity.Name)
                .GetJsonAsync<IEnumerable<Midia>>();
            
            return View(midias);
        }
    }
}
