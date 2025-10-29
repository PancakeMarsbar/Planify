using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Planify.Models;
using Planify.Services;

namespace Planify.ViewModels.V2
{
    public sealed class BoardViewModel
    {
        private readonly AppRepository _repo;
        private System.Timers.Timer? _timer;
        private bool _refreshing;
        internal Func<string, string, Task> Alert;

        public ObservableCollection<BoardLane> Lanes { get; } = new();
        public Dictionary<string, ObservableCollection<Card>> CardsByLane { get; } = new();

        public BoardViewModel(AppRepository repo) { _repo = repo; }

        public async Task InitAsync()
        {
            await _repo.LoadAsync();
            Rebuild();

            _timer = new System.Timers.Timer(5000) { AutoReset = true, Enabled = true };
            _timer.Elapsed += async (_, __) => await SafeRefresh();
            _timer.Start();
        }

        public void Teardown()
        {
            try { _timer?.Stop(); _timer?.Dispose(); _timer = null; } catch { }
        }

        private async Task SafeRefresh()
        {
            if (_refreshing) return;
            _refreshing = true;
            try
            {
                await _repo.LoadAsync();
                MainThread.BeginInvokeOnMainThread(Rebuild);
            }
            catch { }
            finally { _refreshing = false; }
        }

        private void Rebuild()
        {
            Lanes.Clear();
            foreach (var l in _repo.Lanes.OrderBy(l => l.Order)) Lanes.Add(l);

            CardsByLane.Clear();
            foreach (var lane in Lanes)
                CardsByLane[lane.Id] = new ObservableCollection<Card>(_repo.Cards.Where(c => c.LaneId == lane.Id));
        }

        private ObservableCollection<Card> CollectionFor(string laneId)
            => CardsByLane.TryGetValue(laneId, out var oc) ? oc : (CardsByLane[laneId] = new ObservableCollection<Card>());

        public async Task Move(Card c, string toLaneId)
        {
            var wasRunning = _timer?.Enabled == true;
            if (wasRunning) _timer!.Stop();

            var fromLane = c.LaneId;
            if (fromLane == toLaneId) return;

            var fromList = CollectionFor(fromLane);
            var toList = CollectionFor(toLaneId);

            if (fromList.Contains(c)) fromList.Remove(c);
            if (!toList.Contains(c)) toList.Add(c);

            c.LaneId = toLaneId;
            await _repo.SaveAsync();
            _repo.Log("MoveCard", $"{c.AssetTag} : {fromLane} -> {toLaneId}");

            if (wasRunning) _timer!.Start();
        }

        // ------- Lane commands -------
        public async Task AddLane(string title)
        {
            var l = _repo.AddLane(title);
            await _repo.SaveAsync();
            Lanes.Add(l);
            CardsByLane[l.Id] = new ObservableCollection<Card>();
        }

        public async Task RenameLane(BoardLane lane, string newTitle)
        {
            _repo.RenameLane(lane.Id, newTitle);
            await _repo.SaveAsync();
            lane.Title = newTitle;
        }

        public async Task RemoveLane(BoardLane lane)
        {
            var fallback = _repo.Lanes.OrderBy(l => l.Order).First(l => l.Id != lane.Id).Id;
            _repo.RemoveLane(lane.Id, fallback);
            await _repo.SaveAsync();
            Rebuild();
        }

        // ------- Card edit -------
        public async Task EditCardField(Card c, string field, string? value)
        {
            switch (field)
            {
                case "AssetTag": c.AssetTag = value ?? ""; break;
                case "Model": c.Model = value ?? ""; break;
                case "Serial": c.Serial = value ?? ""; break;
                case "PersonName": c.PersonName = value ?? ""; break;
                case "LocaterId": c.LocaterId = value ?? ""; break;
                case "Deadline":
                    c.SetupDeadline = DateTime.TryParse(value, out var dt) ? dt : null;
                    break;
            }
            await _repo.SaveAsync();
        }

        // ------- Card create/delete -------
        public async Task<Card> CreateCard(string laneId, string? assetTag, string? model, string? serial)
        {
            var wasRunning = _timer?.Enabled == true;
            if (wasRunning) _timer!.Stop();

            var card = _repo.AddCard(laneId, assetTag, model, serial);
            await _repo.SaveAsync();

            CollectionFor(laneId).Add(card);

            if (wasRunning) _timer!.Start();
            return card;
        }

        public async Task DeleteCard(Card c)
        {
            var wasRunning = _timer?.Enabled == true;
            if (wasRunning) _timer!.Stop();

            var laneId = c.LaneId;
            _repo.RemoveCard(c.Id);
            await _repo.SaveAsync();

            CollectionFor(laneId).Remove(c);

            if (wasRunning) _timer!.Start();
        }
    }
}
