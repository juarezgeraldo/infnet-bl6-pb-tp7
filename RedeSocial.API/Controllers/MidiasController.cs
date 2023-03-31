using AzureBlobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedeSocial.API.DTOs;
using RedeSocial.BLL.Models;
using RedeSocial.DAL.Data;

public enum PerfilEnum
{
    Público = 1,
    Privado = 2
}

namespace RedeSocial.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MidiasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private BlobService _blobService;
        private readonly UserManager<ApplicationUser> _userManager;
        public MidiasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _blobService = new BlobService();
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Midia>>> GetMidias()
        {
            var midias = new List<Midia>();
            await using var context = _context;
            var innerJoin = from m in context.Midias
                join u in context.Users on m.NomeUsuario equals u.UserName
                where u.PerfilId == (int) PerfilEnum.Público
                select new
                Midia()
                {
                    Titulo = m.Titulo,
                    EnderecoBlob = m.EnderecoBlob,
                    Id = m.Id,
                    NomeUsuario = m.NomeUsuario
                };

            midias = innerJoin.ToList();

            return midias;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Midia>> GetMidia(int id)
        {
            var midia = await _context.Midias.FindAsync(id);
            if (midia == null)
            {
                return NotFound();
            }
            return midia;
        }

        [HttpGet]
        [Route("ListarMidiaUsuario")]
        public async Task<ActionResult<List<Midia>>> ListarMidiaUsuario(string nomeUsuario)
        {
            await using var context = _context;
            var retorno = context.Midias.Where(m => m.NomeUsuario == nomeUsuario).ToList();
            return retorno;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMidia(int id, Midia midia)
        {
            if (id != midia.Id)
            {
                return BadRequest();
            }
            _context.Entry(midia).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MidiaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Midia>> PostMidia(MidiaDto midia)
        {
            var enderecoBlob = await _blobService.AdicionarBlobAoContainer(midia.Base64);
            var midiaModel = new Midia()
            {
                EnderecoBlob = enderecoBlob,
                NomeUsuario = User.Identity.Name,
                Titulo = midia.Titulo
            };

            var midiaGerada = _context.Midias.Add(midiaModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMidia", new { id = midiaGerada.Entity.Id }, midiaGerada.Entity);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMidia(int id)
        {
            var midia = await _context.Midias.FindAsync(id);
            if (midia == null)
            {
                return NotFound();
            }

            _context.Midias.Remove(midia);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MidiaExists(int id)
        {
            return _context.Midias.Any(e => e.Id == id);
        }

    }
}
