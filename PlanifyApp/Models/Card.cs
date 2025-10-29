namespace Planify.Models
{
    public sealed class Card
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        // Placering på boardet
        public string LaneId { get; set; } = "";   // <-- dynamisk lane

        // Device/person info
        public string AssetTag { get; set; } = ""; // iMac/PC nr
        public string Serial { get; set; } = "";
        public string Model { get; set; } = "";
        public string PersonName { get; set; } = "";
        public string LocaterId { get; set; } = ""; // etage.rum.plads
        public string Role { get; set; } = "";

        public MachineStatus Status { get; set; } = MachineStatus.InStorage;
        public DateTime? SetupDeadline { get; set; }

        // UI badges
        public bool DeadlineRed => SetupDeadline.HasValue && SetupDeadline.Value.Date < DateTime.Today;
        public bool DeadlineYellow => SetupDeadline.HasValue && SetupDeadline.Value.Date == DateTime.Today.AddDays(1);
    }
}
