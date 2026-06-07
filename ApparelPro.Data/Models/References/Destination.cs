using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.References
{
    public class Destination
    {
        public int Id { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;      
      
        [NotMapped]
        public string? CountryName { get; set; }        
    }
}
