using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.References
{
    public class Destination
    {
        // Real business code (legacy od_dest.dest_cd, e.g. "BAL"/"LON") - replaces the old
        // meaningless surrogate Id, which nothing referenced and had no legacy equivalent.
        public string Code { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;

        [NotMapped]
        public string? CountryName { get; set; }
    }
}
