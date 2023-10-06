
Baza podataka: sastoji se od 4 tabele Pacijenti, Doktori, Dijagnoze, i tabela za linkovanje pacijenata i dijagnnoza DijagnozaPacijent


Autentifikacija:
Uradjena je bez koriscenja baze (Nemamo baze za korisnike)
Tako da se gube podaci o registraciji prilikom ponovnog pokretanja API-a

• Po pokretanju API-a Vrsi se prvo registracija:
- U postman-u -> https://localhost:7016/api/Autentifikacija/Registracija
salje se raw JSON:
{
    "KorisnickoIme":"UnesiKorisnickoIme",
    "Lozinka":"UnesiLozinku"
}

• Po zavrsetku registracije vrsi se login:
- U postman-u -> https://localhost:7016/api/Autentifikacija/Login
salje se raw JSON:
{
    "KorisnickoIme":"UnesiKorisnickoIme",
    "Lozinka":"UnesiLozinku"
}
Nakon uspesnog login-a dobijamo JWtoken koji koristimo za autentifikaciju svih CRUD operacija u API-u koriscenjem bearer token type autentifikacije


Doktor:
• Pregled svih doktora ide preko [HttpGet("PrikaziSveDoktore")]
- U postman-u ->https://localhost:7016/api/Doktor/PrikaziSveDoktore
(Prikazivanje doktora je dostupno bez autentifikacije)

• U bazu Doktori se dodaju preko [HttpPost("DodajDoktora")]
- U postman-u ->
https://localhost:7016/api/Doktor/DodajDoktora
salje se raw JSON:
{
    "Ime": "ImeDoktora",
    "Prezime": "PrezimeDoktora"
}


Pacijent:
• Pregled svih pacijenata [HttpGet("PrikaziSvePacijente")] 
- U postman-u ->
https://localhost:7016/api/PrijemPacijenata/PrikaziSvePacijente

• Dodavanje pacijenata [HttpPost("DodajPacijenta")]
- U postman-u ->
https://localhost:7016/api/PrijemPacijenata/DodajPacijenta

salje se raw JSON:
{
    "Ime": "ImePacijenta",
    "Prezime": "PrezimePacijenta",
    "BrojSobe": 100,
    "DoktorId": 1,
    "Dijagnoze": [
        {
            "ImeDijagnoze": "Dijagnoza1"
        },
        {
            "ImeDijagnoze": "Dijagnoza2"
        }
    ]
}

• Pretraga pacijenata [HttpPost("PretraziPacijente")]
- U postman-u ->
https://localhost:7016/api/PrijemPacijenata/PretraziPacijente
Za pretragu se moze koristiti bilo koja kombinacija atributa
salje se raw JSON:
{
    "Ime": "ImePacijenta", 
    "Prezime": "PrezimePacijenta",
    "BrojSobe": 101,
    "DoktorId": 2,
    "DiagnozaIme": "ImeDijagnoze"
}
Svaki od ovih argumenata se mogu koristiti pojedinacno u pretrazi a moze i samo jedan ili bilo koja kombinacija
{
    "Ime": "ImePacijenta", 
    "BrojSobe": 101,
    "DiagnozaIme": "ImeDijagnoze"
} ---//---itd.


• Brisanje pacijenata [HttpDelete("ObrisiPacijenta/{id}")]
- U postman-u -> https://localhost:7016/api/PrijemPacijenata
						/ObrisiPacijenta/{id pacijenta}


Azuriranje Pacijenta:
	-Azuriranja se vrse preko url-a

• Brisanje dijagnoze pacijenta [HttpDelete("ObrisiDijagnozuPacijenta/{patientId}/{diagnosisName}")]
- U postman-u -> https://localhost:7016/api/PrijemPacijenata
/ObrisiDijagnozuPacijenta/{id pacijenta}/{ime dijagnoze}
Brisanjem dijagnoze se uklanja dijagnoza iz liste dijagnoza pacijenta nakon brisanja svih dijagnoza pacijent se uklanja iz sistema a u slucaju da se ukloni dijagnoza "COVID" automatski se pacijent premesta u drugu sobu.

• Dodavanje dijagnozepacijenta [HttpPost("DodajDijagnozuPacijentu/{patientId}/{diagnosisName}")]
- U postman-u -> https://localhost:7016/api/PrijemPacijenata
/DodajDijagnozuPacijentu/{id pacijenta}/{ime dijagnoze}
Dodavanjem, dijagnoze se dodaju u listu kod pacijenta u slucaju da dijagnoza vec postoji kod pacijenta salje poruku da dijagnoza vec postoji u slucaju da se doda dijagnoza COVID automatski se pacijent premesta u sobu za COVID

• Promena lekara pacijenta [HttpPut("PromeniDoktoraPacijenta/{patientId}/{newDoctorId}")]
- U postman-u -> https://localhost:7016/api/PrijemPacijenata
/PromeniDoktoraPacijenta/{id pacijenta}/{id doktora koga prebacujemo pacijenta}
Kod promene doktora pacijent se brise iz liste pacijenata starog doktora i prebacuje u listu novog u slucaju da doktor kod koga zelimo da prebacimo pacijenta vec ima 5 pacijenata salje nam poruku da je nemoguce prebacivanje

• Promena sobe pacijenta [HttpPut("PromeniBrojSobePacijenta/{patientId}/{newRoomnumber}")]
- U postman-u -> https://localhost:7016/api/PrijemPacijenata
/PromeniBrojSobePacijenta/{id pacijenta}/{soba u koju prebacujemo}






