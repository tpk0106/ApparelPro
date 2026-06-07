using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApparelPro.Data.Configurations.References
{    
    using global::ApparelPro.Data.Models.References;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    namespace ApparelPro.Data.Configurations.Registration
    {
        public class IdentityAddressConfig : IEntityTypeConfiguration<Address>
        {
            public void Configure(EntityTypeBuilder<Address> entity)
            {
                // Explicitly tells the Identity context that Address maps to "Addresses"
                entity.ToTable("Addresses");
            }
        }
    }

}
