using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace PrijemPacijenata.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutentifikacijaController : ControllerBase
    {
        public static Korisnik korisnik = new Korisnik();
        private readonly IConfiguration configuration;

        public AutentifikacijaController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost("Registracija")]
        public async Task<ActionResult<Korisnik>> Registracija(Registracija request)
        {
            NapraviHashLozinke(request.Lozinka, out byte[] lozinkaHash, out byte[] lozinkaSalt);

            korisnik.KorisnickoIme = request.KorisnickoIme;
            korisnik.LozinkaHash = lozinkaHash;
            korisnik.LozinkaSalt = lozinkaSalt;

            return Ok(korisnik);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(Registracija request)
        {
            if (korisnik.KorisnickoIme != request.KorisnickoIme)
            {
                return BadRequest("Korisnik nije pronadjen");
            }

            if(!VrifikujHashLozinke(request.Lozinka, korisnik.LozinkaHash, korisnik.LozinkaSalt))
            {


                return BadRequest("Pogresna lozinka");
            }

            string token = NapraviToken(korisnik);
            return Ok(token);
        }
        
        private string NapraviToken(Korisnik korisnik)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, korisnik.KorisnickoIme),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var kljuc = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value));

            var akredit=new SigningCredentials(kljuc, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: akredit
                );
            
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void NapraviHashLozinke(string lozinka, out byte[] lozinkaHash, out byte[] lozinkaSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                lozinkaSalt = hmac.Key;
                lozinkaHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(lozinka));
            }
        }

        private bool VrifikujHashLozinke(string lozinka, byte[] lozinkaHash, byte[] lozinkaSalt)
        {
            using(var hmac = new HMACSHA512(lozinkaSalt))
            {
                var racunajHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(lozinka));
                return racunajHash.SequenceEqual(lozinkaHash);
            }
        }


    }
}
