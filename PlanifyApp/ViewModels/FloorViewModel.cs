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

        public ObservableCollection<Seat> Seats { get; } = new();
        public Floor? CurrentFloor { get; private set; }
        public int CurrentLevel => CurrentFloor?.Level ?? 0;

        public FloorViewModel(AppRepository repo) => _repo = repo;

        public async Task InitAsync()
        {
            await _repo.LoadAsync();

            // sikr mindst én floor
            if (_repo.Floors == null || _repo.Floors.Count == 0)
            {
                _repo.Floors = new System.Collections.Generic.List<Floor>
                {
                    new Floor
                    {
                        Company="Company1", Building="A", Level=0,
                        Tables =
                        {
                            new Table
                            {
                                Id="T-01", X=50, Y=60, Width=300, Height=160,
                                Seats =
                                {
                                    new Seat{ Id="S-01", X=60,  Y=80,  LocaterId="0.3.5", Role="Cutter" },
                                    new Seat{ Id="S-02", X=220, Y=80,  LocaterId="0.2.1", Role="Producer" }
                                }
                            }
                        }
                    }
                };
                await _repo.SaveAsync();
            }

            CurrentFloor = _repo.Floors.FirstOrDefault();
            Seats.Clear();
            if (CurrentFloor != null)
            {
                foreach (var s in CurrentFloor.Tables.SelectMany(t => t.Seats))
                    Seats.Add(s);
            }

            Raise(nameof(Seats));
            Raise(nameof(CurrentFloor));
            Raise(nameof(CurrentLevel));
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
