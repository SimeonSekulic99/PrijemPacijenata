using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PrijemPacijenata.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoktorController : ControllerBase
    {
        private readonly DataContext context;

        public DoktorController(DataContext context)
        {
            this.context = context;
        }


        [HttpGet("PrikaziSveDoktore")]
        public ActionResult<IEnumerable<Doktor>> PtrikaziSveDoktore()
        {
            var doktori = context.Doktori.ToList(); // Vraca sve doktore iz baze
            return Ok(doktori);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("DodajDoktora")]
        public IActionResult DodajDoktora([FromBody] Doktor model)
        {
            if (ModelState.IsValid)
            {
                // Pravi novog doktora i dodaje mu ime i prezime
                var newDoctor = new Doktor
                {
                    ImeDoktora ="Dr."+ model.ImeDoktora,
                    PrezimeDoktora = model.PrezimeDoktora,
                };

                // Dodaje novog doktora i cuva promene
                context.Doktori.Add(newDoctor);
                context.SaveChanges();

                return Ok("Doktor je dodat");
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
