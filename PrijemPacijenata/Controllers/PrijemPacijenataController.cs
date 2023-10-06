using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PrijemPacijenata.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrijemPacijenataController : ControllerBase
    {
        private readonly DataContext context;

        public PrijemPacijenataController(DataContext context)
        {
            this.context = context;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("PrikaziPacijentaSaId-/{id}")]
        public async Task<ActionResult<Pacijent>> PrikaziPacijenta(int id)
        {
            var pacijent = await context.Pacijenti
                .Include(p => p.Doktor)
                .Include(p => p.Dijagnoze)
                .FirstOrDefaultAsync(p => p.IDPacijenta == id);

            if (pacijent == null)
            {
                return NotFound("Pacijent nije pronadjen");
            }

            var result = new
            {
                pacijent.IDPacijenta,
                pacijent.Ime,
                pacijent.Prezime,
                pacijent.BrojSobe,
                Doktor = new
                {
                    pacijent.Doktor.ImeDoktora,
                    pacijent.Doktor.PrezimeDoktora
                },
                Dijagnoze = pacijent.Dijagnoze.Select(dp => dp.ImeDijagnoze).ToList()
            };
            return Ok(result);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("PrikaziSvePacijente")]
        public async Task<ActionResult<List<Pacijent>>> PrikaziSvePacijente()
        {
            var pacijenti = await context.Pacijenti
                .Include(p => p.Doktor) // Dodaje detalje o doktoru za pacijenta
                .Include(p => p.Dijagnoze) // Dodaje dijagnoze za pacijenta
                .ToListAsync();

            var result = pacijenti.Select(p => new
            {
                p.IDPacijenta,
                p.Ime,
                p.Prezime,
                p.BrojSobe,
                Doktor = new
                {
                    p.Doktor.ImeDoktora,
                    p.Doktor.PrezimeDoktora
                },
                Dijagnoze = p.Dijagnoze.Select(dp => dp.ImeDijagnoze).ToList()
            }).ToList();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("DodajPacijenta")]
        public IActionResult CreatePatient([FromBody] Pacijent model)
        {
            if (model == null || string.IsNullOrEmpty(model.Ime) || string.IsNullOrEmpty(model.Prezime) || model.DoktorId <= 0 || model.BrojSobe <= 0)
            {
                return BadRequest("Nisu pruženi svi obavezni podaci: Ime, Prezime, DoktorId, BrojSobe.");
            }

            if (ModelState.IsValid)
            {
                var existingDoctor = context.Doktori.Include(d => d.Pacijenti).FirstOrDefault(d => d.IDDoktora == model.DoktorId);

                if (existingDoctor != null)
                {
                    if (existingDoctor.Pacijenti == null)
                    {
                        existingDoctor.Pacijenti = new List<Pacijent>();
                    }

                    if (existingDoctor.Pacijenti.Count >= 5) // Proveravamo da li lekar ima 5 pacijenata i ako ima vracamo upozorenje i prekidamo unos
                    {
                        return BadRequest("Doktor ima maksimalan broj pacijenata (5)");
                    }

                    //Proveravamo da li je neka od unetih dijagnoza COVID19 ako jeste proveravamo da li je uneta odgovarajuca soba (123 je COVID soba)
                    if (model.Dijagnoze != null && model.Dijagnoze.Any(d => d.ImeDijagnoze.Equals("COVID", StringComparison.OrdinalIgnoreCase)))
                    {
                        //Ako pacijent nije stavljen u sobu 123 a ima dijagnozu COVID saljemo upozorenje i prekidamo unos
                        if(model.BrojSobe != 123)
                        {
                            return BadRequest("Pacijenti sa Dijagnozom COVID moraju se smestiti u sobu 123");
                        }
                    }
                    else //Ako nema dijagnoze COVID za pacijenta a soba je 123 onda saljemo upozorenje i prekidamo unos
                    {
                        if (model.BrojSobe == 123)
                        {
                            return BadRequest("Pacijenti koji nemaju COVID dijagnozu ne mogu se smestiti u sobu 123");
                        }
                    }

                    var newPatient = new Pacijent
                    {
                        Ime = model.Ime,
                        Prezime = model.Prezime,
                        BrojSobe = model.BrojSobe,
                        Doktor = existingDoctor,
                        Dijagnoze = model.Dijagnoze
                    };

                    existingDoctor.Pacijenti.Add(newPatient);

                    context.Pacijenti.Add(newPatient);
                    context.SaveChanges();

                    return Ok(new { message = "Pacijent uspesno unet! Odabrani lekar ima " + existingDoctor.Pacijenti.Count + " pacijenata" });
                }
                else
                {
                    return BadRequest("Trazeni Doktor nije pronadjen");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("PretraziPacijente")]
        public async Task<ActionResult<List<Pacijent>>> SearchPatients([FromBody] PacijentSearchParameters searchParameters)
        {
            // Proverava da li ima parametara za pretragu
            if (searchParameters == null ||
                (string.IsNullOrEmpty(searchParameters.Ime) &&
                 string.IsNullOrEmpty(searchParameters.Prezime) &&
                 !searchParameters.BrojSobe.HasValue &&
                 !searchParameters.DoktorId.HasValue &&
                 string.IsNullOrEmpty(searchParameters.DiagnozaIme)))
            {
                return BadRequest("Niste uneli ni jedan parametar za pretragu");
            }

           
            var query = context.Pacijenti
                .Include(p => p.Doktor)
                .Include(p => p.Dijagnoze)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchParameters.Ime))
            {
                query = query.Where(p => p.Ime.Contains(searchParameters.Ime));
            }

            if (!string.IsNullOrEmpty(searchParameters.Prezime))
            {
                query = query.Where(p => p.Prezime.Contains(searchParameters.Prezime));
            }

            if (searchParameters.BrojSobe.HasValue)
            {
                query = query.Where(p => p.BrojSobe == searchParameters.BrojSobe);
            }

            if (searchParameters.DoktorId.HasValue)
            {
                query = query.Where(p => p.DoktorId == searchParameters.DoktorId);
            }

            if (!string.IsNullOrEmpty(searchParameters.DiagnozaIme))
            {
                query = query.Where(p => p.Dijagnoze.Any(d => d.ImeDijagnoze.Contains(searchParameters.DiagnozaIme)));
            }

            var pacijenti = await query.ToListAsync();

            var result = pacijenti.Select(p => new
            {
                p.IDPacijenta,
                p.Ime,
                p.Prezime,
                p.BrojSobe,
                Doktor = new
                {
                    p.Doktor.ImeDoktora,
                    p.Doktor.PrezimeDoktora
                },
                Dijagnoze = p.Dijagnoze.Select(dp => dp.ImeDijagnoze).ToList()
            }).ToList();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("ObrisiPacijenta/{id}")]
        public IActionResult DeletePatient(int id)
        {
            var patcijent = context.Pacijenti.Find(id);

            if (patcijent == null)
            {
                return NotFound("Pacijent nije pronadjen");
            }

            context.Pacijenti.Remove(patcijent);
            context.SaveChanges();

            return Ok("Pacijent uspesno obrisan");
        }

        /////////////AZURIRANJE PACIJENTA//////////////
        [Authorize(Roles = "Admin")]
        [HttpDelete("ObrisiDijagnozuPacijenta/{patientId}/{diagnosisName}")]
        public IActionResult DeleteDiagnosisFromPatient(int patientId, string diagnosisName)
        {
            var patient = context.Pacijenti.Include(p => p.Dijagnoze).FirstOrDefault(p => p.IDPacijenta == patientId);

            if (patient == null)
            {
                return NotFound("Pacijent nije pronadjen");
            }

            var diagnosisToDelete = patient.Dijagnoze.FirstOrDefault(d => d.ImeDijagnoze.Equals(diagnosisName, StringComparison.OrdinalIgnoreCase));

            if (diagnosisToDelete == null)
            {
                return NotFound($"Dijagnoza '{diagnosisName}' nije pronadjena za ovog pacijenta");
            }

            if (diagnosisName.Equals("COVID", StringComparison.OrdinalIgnoreCase))
            {
                // Ako obrisemo COVID dijagnozu pacijent se prebacuje u drugu sobu
                patient.BrojSobe = 100;
            }

            patient.Dijagnoze.Remove(diagnosisToDelete);

            // Proverava da li pacijent nema vise dijagnoza
            if (patient.Dijagnoze.Count == 0)
            {
                context.Pacijenti.Remove(patient);
                context.SaveChanges();

                return Ok("Pacijent nema vise dijagnoza pa je uklonjen iz sistema");
            }

            context.SaveChanges();

            return Ok($"Dijagnoza '{diagnosisName}' uspesno obrisana iz pacijentovih podataka");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("DodajDijagnozuPacijentu/{patientId}/{diagnosisName}")]
        public IActionResult AddDiagnosisToPatient(int patientId, string diagnosisName)
        {
            var patient = context.Pacijenti.Include(p => p.Dijagnoze).FirstOrDefault(p => p.IDPacijenta == patientId);

            if (patient == null)
            {
                return NotFound("Pacijent nije pronadjen");
            }

            // Proverava da li dijagnoza vec postoji kod pacijenta
            var existingDiagnosis = patient.Dijagnoze.FirstOrDefault(d => d.ImeDijagnoze.Equals(diagnosisName, StringComparison.OrdinalIgnoreCase));

            if (existingDiagnosis != null)
            {
                return BadRequest($"Dijagnoza '{diagnosisName}' vec postoji za ovog pacijenta");
            }

            var newDiagnosis = new Dijagnoza { ImeDijagnoze = diagnosisName };
            patient.Dijagnoze.Add(newDiagnosis);

            // Proverava da li je dodata dijagnoza COVID i ako jeste premesta pacijenta u COVID sobu
            if (diagnosisName.Equals("COVID", StringComparison.OrdinalIgnoreCase))
            {
                patient.BrojSobe = 123;
            }

            context.SaveChanges();

            return Ok($"Dijagnoza '{diagnosisName}' uspesno dodata pacijentu");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("PromeniDoktoraPacijenta/{patientId}/{newDoctorId}")]
        public IActionResult ChangePatientDoctor(int patientId, int newDoctorId)
        {
            var patient = context.Pacijenti.Include(p => p.Doktor).FirstOrDefault(p => p.IDPacijenta == patientId);

            if (patient == null)
            {
                return NotFound("Pacijent nije pronadjen");
            }

            var newDoctor = context.Doktori.Include(d => d.Pacijenti).FirstOrDefault(d => d.IDDoktora == newDoctorId);

            if (newDoctor == null)
            {
                return NotFound("Ovaj doktor nije pronadjen");
            }

            // Da li doktor kome zelimo da prebacimo pacijenta vec ima 5 ppacijenata
            if (newDoctor.Pacijenti != null && newDoctor.Pacijenti.Count >= 5)
            {
                return BadRequest("Ovaj doktor vec ima maksimalan broj pacijenata (5)");
            }

            // Brisemo pacijenta iz liste predhodnog doktora
            if (patient.Doktor != null)
            {
                patient.Doktor.Pacijenti.Remove(patient);
            }

            // Azurira se doktor pacijenta
            patient.Doktor = newDoctor;

            // Dodaje se pacijent na listu novog doktora
            if (newDoctor.Pacijenti == null)
            {
                newDoctor.Pacijenti = new List<Pacijent>();
            }
            newDoctor.Pacijenti.Add(patient);

            context.SaveChanges();

            return Ok($"Doktor pacijenta uspesno promenjen na {newDoctor.ImeDoktora} {newDoctor.PrezimeDoktora}");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("PromeniBrojSobePacijenta/{patientId}/{newRoomnumber}")]
        public IActionResult ChangePatientRoomNumber(int patientId, int newRoomNumber)
        {
            var patient = context.Pacijenti.Include(p => p.Dijagnoze).FirstOrDefault(p => p.IDPacijenta == patientId);

            if (patient == null)
            {
                return NotFound("Pacijent nije pronadjen");
            }

            // Check if the patient has a COVID diagnosis
            var hasCovidDiagnosis = patient.Dijagnoze.Any(d => d.ImeDijagnoze.Equals("COVID", StringComparison.OrdinalIgnoreCase));

            // Check if the new room number is 123 and the patient does not have COVID
            if (newRoomNumber == 123 && !hasCovidDiagnosis)
            {
                return BadRequest("Pacijent nema dijagnozu COVID. Promena broja sobe na 123 nije dozvoljena.");
            }

            patient.BrojSobe = newRoomNumber;
            context.SaveChanges();

            return Ok($"Broj sobe pacijenta uspesno promenjen na {newRoomNumber}");
        }


        ///////////////////////////////////////////////
    }

}
