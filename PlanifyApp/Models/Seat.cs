namespace Planify.Models
{
    public sealed class Seat
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        // Pixel-position i floor-canvas
        public double X { get; set; }
        public double Y { get; set; }

        // LOCATER-ID: etage.rum.plads (fx "0.3.5")
        public string LocaterId { get; set; } = "";

        public string Role { get; set; } = "";

        // Status (Free/Occupied)
        public SeatStatus Status { get; set; } = SeatStatus.Free; // <- var Ledig før
    }
}
