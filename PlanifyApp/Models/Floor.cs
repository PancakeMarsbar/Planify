using System.Collections.Generic;

namespace Planify.Models
{
    public sealed class Floor
    {
        public string Company { get; set; } = "Company1";
        public string Building { get; set; } = "A";
        public int Level { get; set; }              // 0 = stue
        public string Name => $"{Company}-{Building}-{Level}";

        public string ImagePath { get; set; } = ""; // valgfri i MVP
        public List<Table> Tables { get; set; } = new();
    }
}
