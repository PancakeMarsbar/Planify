using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Planify.Models;
using Planify.Pages;
using Planify.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace Planify.Services
{
    public sealed class AppRepository
    {

        private static readonly Lazy<AppRepository> _instance = new(() => new AppRepository());
        public static AppRepository Instance => _instance.Value;

        private AppRepository() { } // private constructor
        public List<Card> Cards { get; private set; } = new();

        // Floors er en liste af FloorPlan (vores nye model)
        public List<FloorPlan> Floors { get; set; } = new();

        public List<BoardLane> Lanes { get; private set; } = new();

        public List<UserAccount> Users { get; private set; } = new();


        private readonly JsonStore _store = new();
        private readonly AuditLog _audit = new();
        private readonly FileMutex _mutex = new("repo");

        public string CurrentUser { get; set; } = "Not loged in there is an ERROR";
        public bool IsAdmin { get; set; } = false;
        public bool IsLogedIn { get; set; } = false;

        // ---------- Load / Save ----------
        public async Task LoadAsync()
        {
            try
            {
                if (_mutex.TryLock())
                {
                    try
                    {
                        Cards = await _store.LoadAsync<List<Card>>("cards") ?? new List<Card>();
                        Floors = await _store.LoadAsync<List<FloorPlan>>("floors") ?? SeedFloors();
                        Lanes = await _store.LoadAsync<List<BoardLane>>("lanes") ?? SeedLanes();
                        Users = await _store.LoadAsync<List<UserAccount>>("users") ?? SeedBaseUsers();

                        // Sørg for at alle kort har en lane
                        var firstLaneId = Lanes.OrderBy(l => l.Order).First().Id;
                        foreach (var c in Cards.Where(c => string.IsNullOrWhiteSpace(c.LaneId)))
                            c.LaneId = firstLaneId;
                    }
                    finally
                    {
                        _mutex.Unlock();

                    }
                }

                if (Cards.Count == 0)
                    Cards = SeedCards(Lanes);
            }
            catch
            {
                if (Lanes.Count == 0) Lanes = SeedLanes();
                if (Floors.Count == 0) Floors = SeedFloors();
                if (Cards.Count == 0) Cards = SeedCards(Lanes);
                if (Users.Count == 0) Users = SeedBaseUsers();
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
                        await _store.SaveAsync("users", Users);
                    }
                    finally
                    {
                        _mutex.Unlock();
                    }
                }
            }
            catch
            {
                // slug IO-fejl i MVP
            }
        }

        public void Log(string action, string details)
            => _audit.Write(CurrentUser, action, details);


        // --------- Account Logic -------------
        public UserAccount? GetUser(string username) => Users.FirstOrDefault(u => u.Username == username);

        public void CreateUser(UserAccount user)
        {
            if (!Users.Any(u => u.Username == user.Username))
                Users.Add(user);
        }

        public void UpdateUser(UserAccount oldUser,UserAccount user)
        {
            // double check if it has not been deleted
            var existing = GetUser(oldUser.Username);
            if (existing != null)
            {
                existing.Username = user.Username;
                existing.IsAdmin = user.IsAdmin;
                existing.Password = user.Password;
                existing.Image = user.Image;
            }
        }

        public void RemoveUser(UserAccount user)
        {
            var existing = GetUser(user.Username);
            if (existing != null)
            {
                Users.Remove(existing);
            }
        }


        // --------- Login Credentials -----------
        public async Task<bool> LoginAsync(string username, string password)
        {
            var users = await _store.LoadAsync<List<UserAccount>>("users") ?? new List<UserAccount>();

            if (users.Count == 0)
            {
                users = SeedBaseUsers();
            }

            var user = users.FirstOrDefault(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (user != null)
            {
                CurrentUser = user.Username;
                IsAdmin = user.IsAdmin;
                IsLogedIn = true;
                return true;
            }

            return false;
        }

        public async void Logout()
        {
            CurrentUser = "Not loged in there is an ERROR";
            IsAdmin = false;
            IsLogedIn = false;
            Application.Current.MainPage = new LoginPage();
            return;
        }





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

            Cards.Where(c => c.LaneId == laneId)
                 .ToList()
                 .ForEach(c => c.LaneId = fallbackLaneId);

            Lanes.RemoveAll(l => l.Id == laneId);
        }

        // ---------- Cards ----------
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

        // ---------- Helper / regler ----------
        public IEnumerable<Card> CardsAtLocater(string loc)
            => Cards.Where(c => string.Equals(c.LocaterId, loc, StringComparison.OrdinalIgnoreCase));

        public bool LocaterExistsOnLevel(int level, string locaterId)
        {
            if (!Regex.IsMatch(locaterId, @"^\d+\.\d+\.\d+$"))
                return false;

            return Floors.Any(f => f.Level == level &&
                                   f.Tables.SelectMany(t => t.Seats)
                                           .Any(s => s.LocaterId == locaterId));
        }

        public bool CanMoveToInUse(Card c, int level, out string? reason)
        {
            if (string.IsNullOrWhiteSpace(c.LocaterId))
            {
                reason = "LOCATER-ID mangler";
                return false;
            }

            if (!LocaterExistsOnLevel(level, c.LocaterId))
            {
                reason = "LOCATER-ID findes ikke på etagen";
                return false;
            }

            if (string.IsNullOrWhiteSpace(c.AssetTag))
            {
                reason = "iMac/PC nr mangler";
                return false;
            }

            if (string.IsNullOrWhiteSpace(c.Serial))
            {
                reason = "Serienr. mangler";
                return false;
            }

            if (c.Status == MachineStatus.ToWipe)
            {
                reason = "Maskine skal wipes";
                return false;
            }

            if (!c.SetupDeadline.HasValue)
            {
                reason = "Setup-deadline mangler";
                return false;
            }

            reason = null;
            return true;
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
                new Card{
                    LaneId = lane("SetupQueue"),
                    LocaterId = "0.3.5",
                    AssetTag = "IMAC-001",
                    Serial = "S-001",
                    Model = "iMac 24",
                    Status = MachineStatus.Ready,
                    SetupDeadline = DateTime.Today.AddDays(1)
                },
                new Card{
                    LaneId = lane("David"),
                    LocaterId = "0.3.5",
                    AssetTag = "LAP-101",
                    Serial = "S-101",
                    Model = "MBP 14",
                    Status = MachineStatus.ToWipe,
                    SetupDeadline = DateTime.Today.AddDays(-1)
                },
                new Card{
                    LaneId = lane("I brug"),
                    LocaterId = "0.2.1",
                    AssetTag = "IMAC-002",
                    Serial = "S-002",
                    Model = "iMac 27",
                    PersonName = "Mads",
                    Status = MachineStatus.InUse,
                    SetupDeadline = DateTime.Today
                },
                new Card{
                    LaneId = lane("Lager"),
                    LocaterId = "",
                    AssetTag = "IMAC-050",
                    Serial = "S-050",
                    Model = "iMac 24",
                    Status = MachineStatus.InStorage
                }
            };
        }

        private static List<FloorPlan> SeedFloors()
        {
            var f = new FloorPlan
            {
                Level = 0,
                Company = "Company1",
                Building = "A",
                Name = "St."
            };

            // VIGTIGT:
            // X og Y er nu RELATIVE koordinater (0-1),
            // så bordet ligger cirka 10% fra venstre / 10% fra toppen.
            var t = new Table
            {
                Id = "T-01",
                X = 0.10,
                Y = 0.10,
                Width = 300,
                Height = 160
            };

            t.Seats.Add(new Seat
            {
                Id = "S-01",
                X = 60,
                Y = 80,
                LocaterId = "0.3.5",
                Role = "Cutter"
            });

            t.Seats.Add(new Seat
            {
                Id = "S-02",
                X = 220,
                Y = 80,
                LocaterId = "0.2.1",
                Role = "Producer"
            });

            f.Tables.Add(t);
            return new List<FloorPlan> { f };
        }



        private List<UserAccount> SeedBaseUsers()
        {
            var users = new List<UserAccount>
            {
                new UserAccount
                {
                    Username = "admin",
                    Password = "admin123",  // TODO: hash later
                    IsAdmin = true,
                }
            };
            return users;
        }
    }
}
