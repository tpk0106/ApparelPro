using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ApparelPro.Data.ValueGenerators
{
    public class ApparelProAddressIdValueGenerator : ValueGenerator<Guid>
    {
        public override bool GeneratesTemporaryValues { get; }

        public override Guid Next(EntityEntry entry)
        {            
            var guid = new Guid();
            Console.WriteLine(guid);
            return guid;
        }
    }
}
