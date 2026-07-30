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
                // Explicitly tells the Identity context that Address maps to "Addresses".
                // ExcludeFromMigrations() is the key part: Address is owned and evolved
                // exclusively by ApparelProDbContext's own migrations (see AddMultipleAddressesToBank/
                // AddMultipleAddressesToBuyer for BankCode/BuyerCode). Without this, UserIdentityDbContext
                // independently tracks Address's full shape too, and any drift between the two contexts'
                // migration histories gets bundled into whatever migration you generate here next -
                // which is exactly what produced the stray BankCode/BuyerCode AddColumn statements today.
                entity.ToTable("Addresses", t => t.ExcludeFromMigrations());
            }
        }
    }

}
