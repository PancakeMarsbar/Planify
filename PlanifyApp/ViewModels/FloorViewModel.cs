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
        public string? CurrentImagePath => CurrentFloor?.ImagePath;

        public FloorViewModel(AppRepository repo) => _repo = repo;

        public async Task InitAsync()
        {
            await _repo.LoadAsync();

            Floors.Clear();
            foreach (var f in _repo.Floors) Floors.Add(f);

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
                    foreach (var s in t.Seats) Seats.Add(s);
                }
            }

            Raise(nameof(Tables));
            Raise(nameof(Seats));
            Raise(nameof(CurrentFloor));
            Raise(nameof(CurrentImagePath));
        }

        public void SelectFloor(FloorPlan floor)
        {
            if (floor == null || floor == CurrentFloor) return;
            CurrentFloor = floor;
            RebuildFromCurrent();
        }

        public async Task<FloorPlan> AddFloor(string name)
        {
            var f = new FloorPlan { Name = name, Company = "Company", Level = Floors.Count };
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
            if (CurrentFloor == null) throw new InvalidOperationException("Ingen etage valgt");

            var t = new Table
            {
                Id = $"T-{CurrentFloor.Tables.Count + 1:00}",
                Name = "Nyt bord",
                X = 0.1,
                Y = 0.1,
                Width = 260,
                Height = 140,
            };

            CurrentFloor.Tables.Add(t);
            Tables.Add(t);
            await _repo.SaveAsync();
            return t;
        }

        public async Task UpdateTablePosition(Table t, double relativeX, double relativeY)
        {
            t.X = relativeX;
            t.Y = relativeY;
            await _repo.SaveAsync();
        }

        public async Task UpdateTableSize(Table t, double width, double height)
        {
            t.Width = Math.Max(60, width);
            t.Height = Math.Max(40, height);
            await _repo.SaveAsync();
            Raise(nameof(Tables));
        }

        public async Task<Table> DuplicateTable(Table src)
        {
            if (CurrentFloor == null) throw new InvalidOperationException("Ingen etage valgt");

            var t = new Table
            {
                Id = $"T-{CurrentFloor.Tables.Count + 1:00}",
                Name = src.Name + " (kopi)",
                X = Math.Clamp(src.X + 0.03, 0, 0.97),
                Y = Math.Clamp(src.Y + 0.03, 0, 0.97),
                Width = src.Width,
                Height = src.Height
            };

            CurrentFloor.Tables.Add(t);
            Tables.Add(t);
            await _repo.SaveAsync();
            return t;
        }

        public async Task RenameTable(Table t, string newName)
        {
            if (t == null || CurrentFloor == null) return;

            t.Name = newName;

            await _repo.SaveAsync();
            RebuildFromCurrent();
        }

        public async Task RemoveTable(Table t)
        {
            if (CurrentFloor == null || t == null) return;

            CurrentFloor.Tables.Remove(t);
            Tables.Remove(t);

            await _repo.SaveAsync();
            RebuildFromCurrent();
        }

        public System.Collections.Generic.IEnumerable<Card> CardsForSeat(Seat s)
            => _repo.CardsAtLocater(s.LocaterId);

        public string StatusText(Card c) => c.Status switch
        {
            MachineStatus.ToWipe => "To-Wipe",
            MachineStatus.Ready => "Ready",
            MachineStatus.InUse => "I brug",
            _ => "Lager"
        };

        public async Task RemoveCard(Card card)
        {
            if (card == null) return;

            _repo.RemoveCard(card.Id);
            await _repo.SaveAsync();
            RebuildFromCurrent();
        }
    }
}
