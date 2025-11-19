using System.Collections.Generic;

namespace Planify.Models
{
    public class Table
    {
        public string Id { get; set; } = "";
        // RELATIVE position (0..1)
        public double X { get; set; }
        public double Y { get; set; }

        // Størrelse i pixels på design-canvas
        public double Width { get; set; } = 260;
        public double Height { get; set; } = 140;

        // NY: Rotation i grader (0, 90, 180, 270)
        public double Rotation { get; set; } = 0;

        public List<Seat> Seats { get; set; } = new();
    }
}
