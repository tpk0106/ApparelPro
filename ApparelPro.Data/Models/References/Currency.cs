using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.References
{
    public class Currency
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

      //  public Country Country { get; set; }
        public string? CountryCode { get; set; }
        public string? Minor { get; set; }

        [NotMapped]
        public string? CurrencyDetails
        {
            get { return Code + " : " + Name; }
        }      
    }
}
