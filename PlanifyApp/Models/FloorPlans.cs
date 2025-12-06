using System;
using System.Collections.Generic;
using Planify.Models;

namespace Planify.Models
{
    // En etage med floorplan-billede og borde
    public sealed class FloorPlan
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        // Navn til UI – fx "St." eller "1. sal"
        public string Name { get; set; } = "St.";

        public string Company { get; set; } = "";
        public string Building { get; set; } = "";

        // Kan bruges til sortering hvis du vil
        public int Level { get; set; }

        // Lokal sti til floorplan-billedet
        public string? ImagePath { get; set; }

        // Borde på etagen
        public List<Table> Tables { get; set; } = new();
    }
}
