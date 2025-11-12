using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Planify.Models;
using Planify.Services;


namespace Planify.ViewModels
{
    public sealed class FloorViewModel : BaseViewModel
    {
        private readonly AppRepository _repo;

        public ObservableCollection<FloorPlan> Floors { get; } = new();
        public ObservableCollection<Table> Tables { get; } = new();
        public ObservableCollection<Seat> Seats { get; } = new();

        public FloorPlan? CurrentFloor { get; private set; }
        public string CurrentFloorName => CurrentFloor?.Name ?? "";
        public string? CurrentImagePath => CurrentFloor?.ImagePath;
        public int CurrentLevel => CurrentFloor?.Level ?? 0;

        public FloorViewModel(AppRepository repo) => _repo = repo;

        public async Task InitAsync()
        {
            await _repo.LoadAsync();

            Floors.Clear();
            foreach (var f in _repo.Floors)
                Floors.Add(f);

            CurrentFloor = Floors.FirstOrDefault();
            RebuildFromCurrent();
        }

        private void RebuildFromCurrent()
        {
            Tables.Clear();
            Seats.Clear();

            if (CurrentFloor != null)
            {
                foreach (var t in CurrentFloor.Tables)
                {
                    Tables.Add(t);
                    foreach (var s in t.Seats)
                        Seats.Add(s);
                }
            }

            Raise(nameof(Tables));
            Raise(nameof(Seats));
            Raise(nameof(CurrentFloor));
            Raise(nameof(CurrentFloorName));
            Raise(nameof(CurrentImagePath));
            Raise(nameof(CurrentLevel));
        }

        public void SelectFloor(FloorPlan floor)
        {
            if (floor == null || floor == CurrentFloor)
                return;

            CurrentFloor = floor;
            RebuildFromCurrent();
        }

        public async Task<FloorPlan> AddFloor(string name)
        {
            var f = new FloorPlan
            {
                Name = name,
                Company = "Company",
                Building = "",
                Level = Floors.Count
            };

            _repo.Floors.Add(f);
            Floors.Add(f);
            CurrentFloor = f;
            RebuildFromCurrent();
            await _repo.SaveAsync();
            return f;
        }

        public async Task SetImageForCurrent(string path)
        {
            if (CurrentFloor == null) return;

            CurrentFloor.ImagePath = path;
            await _repo.SaveAsync();
            Raise(nameof(CurrentImagePath));
        }

        public async Task<Table> AddTable()
        {
            if (CurrentFloor == null)
                throw new InvalidOperationException("Ingen etage valgt");

            var t = new Table
            {
                Id = $"T-{CurrentFloor.Tables.Count + 1:00}",

                // RELATIVE positioner (0-1)
                X = 0.1,
                Y = 0.1,

                // faste størrelser (kan du justere senere)
                Width = 260,
                Height = 140
            };

            CurrentFloor.Tables.Add(t);
            Tables.Add(t);
            await _repo.SaveAsync();
            return t;
        }

        // x / y er nu relative koordinater (0-1)
        public async Task UpdateTablePosition(Table t, double relativeX, double relativeY)
        {
            t.X = relativeX;
            t.Y = relativeY;
            await _repo.SaveAsync();
        }

        public System.Collections.Generic.IEnumerable<Card> CardsForSeat(Seat s)
            => _repo.CardsAtLocater(s.LocaterId);

        public async Task AssignPerson(Seat s, string personName)
        {
            _repo.AssignPersonToLocater(s.LocaterId, personName);
            await _repo.SaveAsync();
            Raise(nameof(Seats));
        }

        public string StatusText(Card c) => c.Status switch
        {
            MachineStatus.ToWipe => "To-Wipe",
            MachineStatus.Ready => "Ready",
            MachineStatus.InUse => "I brug",
            _ => "Lager"
        };
    }
}
