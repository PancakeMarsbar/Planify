using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Planify.Models;

namespace Planify.Services
{
    public sealed class AppRepository
    {
        public List<Card> Cards { get; private set; } = new();
        public List<Floor> Floors { get; set; } = new();
        public List<BoardLane> Lanes { get; private set; } = new();

        private readonly JsonStore _store = new();
        private readonly AuditLog _audit = new();
        private readonly FileMutex _mutex = new("repo");

        public string CurrentUser { get; set; } = "David";
        public bool IsAdmin { get; set; } = true;

        // ---------- Load/Save ----------
        public async Task LoadAsync()
        {
            try
            {
                if (_mutex.TryLock())
                {
                    try
                    {
                        Cards = await _store.LoadAsync<List<Card>>("cards") ?? new List<Card>();
                        Floors = await _store.LoadAsync<List<Floor>>("floors") ?? SeedFloors();
                        Lanes = await _store.LoadAsync<List<BoardLane>>("lanes") ?? SeedLanes();

                        var firstLaneId = Lanes.OrderBy(l => l.Order).First().Id;
                        foreach (var c in Cards.Where(c => string.IsNullOrWhiteSpace(c.LaneId)))
                            c.LaneId = firstLaneId;
                    }
                    finally { _mutex.Unlock(); }
                }

                if (Cards.Count == 0) Cards = SeedCards(Lanes);
            }
            catch
            {
                if (Lanes.Count == 0) Lanes = SeedLanes();
                if (Floors.Count == 0) Floors = SeedFloors();
                if (Cards.Count == 0) Cards = SeedCards(Lanes);
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                if (_mutex.TryLock())
                {
                    try
                    {
                        await _store.SaveAsync("cards", Cards);
                        await _store.SaveAsync("floors", Floors);
                        await _store.SaveAsync("lanes", Lanes);
                    }
                    finally { _mutex.Unlock(); }
                }
            }
            catch { /* ignore in MVP */ }
        }

        public void Log(string action, string details) => _audit.Write(CurrentUser, action, details);

        // ---------- Lanes ----------
        public BoardLane AddLane(string title)
        {
            var order = Lanes.Any() ? Lanes.Max(l => l.Order) + 1 : 0;
            var lane = new BoardLane { Title = title, Order = order };
            Lanes.Add(lane);
            return lane;
        }

        public void RenameLane(string laneId, string newTitle)
        {
            var lane = Lanes.FirstOrDefault(l => l.Id == laneId);
            if (lane != null) lane.Title = newTitle;
        }

        public void RemoveLane(string laneId, string fallbackLaneId)
        {
            if (laneId == fallbackLaneId) return;
            Cards.Where(c => c.LaneId == laneId).ToList().ForEach(c => c.LaneId = fallbackLaneId);
            Lanes.RemoveAll(l => l.Id == laneId);
        }

        // ---------- Cards (NYT) ----------
        public Card AddCard(string laneId, string? assetTag = null, string? model = null, string? serial = null)
        {
            var card = new Card
            {
                LaneId = laneId,
                AssetTag = assetTag ?? "",
                Model = model ?? "",
                Serial = serial ?? "",
                Status = MachineStatus.InStorage
            };
            Cards.Add(card);
            Log("AddCard", $"{card.AssetTag} -> lane:{laneId}");
            return card;
        }

        public void RemoveCard(string cardId)
        {
            var idx = Cards.FindIndex(c => c.Id == cardId);
            if (idx >= 0)
            {
                Log("RemoveCard", $"{Cards[idx].AssetTag} ({cardId})");
                Cards.RemoveAt(idx);
            }
        }

        // ---------- Helper/Regler ----------
        public IEnumerable<Card> CardsAtLocater(string loc) =>
            Cards.Where(c => string.Equals(c.LocaterId, loc, StringComparison.OrdinalIgnoreCase));

        public bool LocaterExistsOnLevel(int level, string locaterId)
        {
            if (!Regex.IsMatch(locaterId, @"^\d+\.\d+\.\d+$")) return false;
            return Floors.Any(f => f.Level == level &&
                                   f.Tables.SelectMany(t => t.Seats).Any(s => s.LocaterId == locaterId));
        }

        public bool CanMoveToInUse(Card c, int level, out string? reason)
        {
            if (string.IsNullOrWhiteSpace(c.LocaterId)) { reason = "LOCATER-ID mangler"; return false; }
            if (!LocaterExistsOnLevel(level, c.LocaterId)) { reason = "LOCATER-ID findes ikke p� etagen"; return false; }
            if (string.IsNullOrWhiteSpace(c.AssetTag)) { reason = "iMac/PC nr mangler"; return false; }
            if (string.IsNullOrWhiteSpace(c.Serial)) { reason = "Serienr. mangler"; return false; }
            if (c.Status == MachineStatus.ToWipe) { reason = "Maskine skal wipes"; return false; }
            if (!c.SetupDeadline.HasValue) { reason = "Setup-deadline mangler"; return false; }
            reason = null; return true;
        }

        public void AssignPersonToLocater(string loc, string person)
        {
            foreach (var card in CardsAtLocater(loc))
            {
                if (card.Status != MachineStatus.Ready)
                    card.Status = MachineStatus.ToWipe;
            }
            Log("AssignPerson", $"{person} -> {loc}");
        }

        // ---------- Seed ----------
        private static List<BoardLane> SeedLanes() => new()
        {
            new BoardLane{ Title="SetupQueue", Order=0 },
            new BoardLane{ Title="David",      Order=1 },
            new BoardLane{ Title="Done",       Order=2 },
            new BoardLane{ Title="I brug",     Order=3 },
            new BoardLane{ Title="Lager",      Order=4 },
        };

        private static List<Card> SeedCards(List<BoardLane> lanes)
        {
            string lane(string title) => lanes.First(l => l.Title == title).Id;
            return new List<Card>
            {
                new Card{ LaneId=lane("SetupQueue"), LocaterId="0.3.5", AssetTag="IMAC-001", Serial="S-001", Model="iMac 24",
                          Status=MachineStatus.Ready, SetupDeadline=DateTime.Today.AddDays(1) },
                new Card{ LaneId=lane("David"), LocaterId="0.3.5", AssetTag="LAP-101", Serial="S-101", Model="MBP 14",
                          Status=MachineStatus.ToWipe, SetupDeadline=DateTime.Today.AddDays(-1) },
                new Card{ LaneId=lane("I brug"), LocaterId="0.2.1", AssetTag="IMAC-002", Serial="S-002", Model="iMac 27",
                          PersonName="Mads", Status=MachineStatus.InUse, SetupDeadline=DateTime.Today },
                new Card{ LaneId=lane("Lager"), LocaterId="", AssetTag="IMAC-050", Serial="S-050", Model="iMac 24",
                          Status=MachineStatus.InStorage }
            };
        }

        private static List<Floor> SeedFloors()
        {
            var f = new Floor { Level = 0, Company = "Company1", Building = "A" };
            var t = new Table { Id = "T-01", X = 50, Y = 60, Width = 300, Height = 160 };
            t.Seats.Add(new Seat { Id = "S-01", X = 60, Y = 80, LocaterId = "0.3.5", Role = "Cutter" });
            t.Seats.Add(new Seat { Id = "S-02", X = 220, Y = 80, LocaterId = "0.2.1", Role = "Producer" });
            f.Tables.Add(t);
            return new List<Floor> { f };
        }
    }
}
