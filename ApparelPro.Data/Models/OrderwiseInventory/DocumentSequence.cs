using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.OrderwiseInventory
{
    public class DocumentSequence
    {
        public string NoteType { get; set; } = null!; // "SRN", "GRN", "GIN", etc. (Primary Key)
        public int LastAllocatedNumber { get; set; } // Tracks the running sequence count (e.g. 1, 2, 3)
        public string Prefix { get; set; } = "";     // Optional string prefix tracking additions

    }
}
