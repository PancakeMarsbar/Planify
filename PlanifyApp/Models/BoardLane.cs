namespace Planify.Models
{
    public sealed class BoardLane
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = "Ny kolonne";
        public int Order { get; set; }   // sorteringsrækkefølge
    }
}
