namespace PrijemPacijenata
{
    public class Korisnik
    {
        public string KorisnickoIme { get; set; } = string.Empty;
        public byte[] LozinkaHash { get; set; }
        public byte[] LozinkaSalt { get; set; }
    }
}
