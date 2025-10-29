using System.Collections.Generic;

namespace Planify.Models
{
    public sealed class Tags
    {
        public string Wipe { get; set; } = "P�kr�vet";     // P�kr�vet / Klar
        public string Remote { get; set; } = "Ikke sat";   // Skal s�ttes op / Sat op / Ikke sat
        public int ExtraScreens { get; set; } = 0;          // 0 / 1 / 2+
        public string ClipVersion { get; set; } = "";       // tekst
        public Dictionary<string, string> Custom { get; set; } = new(); // frie tags
    }
}
