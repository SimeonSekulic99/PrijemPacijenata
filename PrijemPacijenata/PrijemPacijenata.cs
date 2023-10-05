using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PrijemPacijenata
{
    public class Pacijent
    {
        [Key]
        public int IDPacijenta { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public int? BrojSobe { get; set; }
        public int? DoktorId { get; set; }
        public Doktor? Doktor { get; set; }
        public List<Dijagnoza>? Dijagnoze { get; set; }

    }

    public class Doktor
    {
        [Key]
        public int IDDoktora { get; set; }
        public string ImeDoktora { get; set; }
        public string PrezimeDoktora { get; set; }
        [JsonIgnore]
        public List<Pacijent>? Pacijenti { get; set; }
    }

    public class Dijagnoza
    {
        [Key]
        public int IDDijagnoze { get; set; }
        public string ImeDijagnoze { get; set; }
        [JsonIgnore]
        public List<Pacijent>? Pacijents { get; set; }
    }

    public class PacijentSearchParameters
    {
        public string? Ime { get; set; }
        public string? Prezime { get; set; }
        public int? BrojSobe { get; set; }
        public int? DoktorId { get; set; }
        public string? DiagnozaIme { get; set; }
    }
}
