using System.Collections.Generic;

namespace Planify.Models
{
    public sealed class Table
    {
        public string Id { get; set; } = "";
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 200;
        public double Height { get; set; } = 120;
        public List<Seat> Seats { get; set; } = new();
    }
}
