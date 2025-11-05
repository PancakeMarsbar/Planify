using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Planify.Models;
using Planify.Services;

namespace Planify.ViewModels.V2
{
    /// <summary>
    /// BoardViewModel v2 med dynamiske kolonner (lanes) og uden auto-refresh timer.
    /// </summary>
    public sealed class BoardViewModel
    {
        private readonly AppRepository _repo;

        public ObservableCollection<BoardLane> Lanes { get; } = new();
        public Dictionary<string, ObservableCollection<Card>> CardsByLane { get; } = new();

        public BoardViewModel(AppRepository repo) => _repo = repo;

        public async Task InitAsync()
        {
            await _repo.LoadAsync();
            Rebuild();
        }

        // I denne version er der ingen timer, så Teardown behøver ikke gøre noget
        public void Teardown()
        {
            // placeholder til senere hvis du vil have auto-refresh tilbage
        }

        /// <summary>
        /// Genopbygger lanes og kort-lister uden at skifte ObservableCollection-objekterne,
        /// så CollectionView stadig ser opdateringerne.
        /// </summary>
        private void Rebuild()
        {
            // --- lanes ---
            Lanes.Clear();
            foreach (var l in _repo.Lanes.OrderBy(l => l.Order))
                Lanes.Add(l);

            // --- cards pr. lane ---
            var existingLaneIds = CardsByLane.Keys.ToList();

            foreach (var lane in Lanes)
            {
                if (!CardsByLane.TryGetValue(lane.Id, out var oc))
                {
                    oc = new ObservableCollection<Card>();
                    CardsByLane[lane.Id] = oc;
                }

                oc.Clear();
                foreach (var card in _repo.Cards.Where(c => c.LaneId == lane.Id))
                    oc.Add(card);
            }

            // fjern lanes der ikke findes længere
            foreach (var id in existingLaneIds.Except(Lanes.Select(l => l.Id)).ToList())
                CardsByLane.Remove(id);
        }

        private ObservableCollection<Card> CollectionFor(string laneId)
        {
            if (!CardsByLane.TryGetValue(laneId, out var oc))
            {
                oc = new ObservableCollection<Card>();
                CardsByLane[laneId] = oc;
            }
            return oc;
        }

        // ----------- Flyt kort -----------
        public async Task Move(Card c, string toLaneId)
        {
            var fromLane = c.LaneId;
            if (fromLane == toLaneId)
                return;

            var fromList = CollectionFor(fromLane);
            var toList = CollectionFor(toLaneId);

            if (fromList.Contains(c))
                fromList.Remove(c);
            if (!toList.Contains(c))
                toList.Add(c);

            c.LaneId = toLaneId;

            await _repo.SaveAsync();
            _repo.Log("MoveCard", $"{c.AssetTag} : {fromLane} -> {toLaneId}");
        }

        // ----------- Lane commands -----------
        public async Task AddLane(string title)
        {
            var lane = _repo.AddLane(title);
            await _repo.SaveAsync();

            Lanes.Add(lane);
            CardsByLane[lane.Id] = new ObservableCollection<Card>();
        }

        public async Task RenameLane(BoardLane lane, string newTitle)
        {
            _repo.RenameLane(lane.Id, newTitle);
            await _repo.SaveAsync();

            lane.Title = newTitle;
            // ingen PropertyChanged her, men CollectionView bruger bare Title ved næste redraw
        }

        public async Task RemoveLane(BoardLane lane)
        {
            // flyt kort til første anden lane
            var fallback = _repo.Lanes.OrderBy(l => l.Order).First(l => l.Id != lane.Id).Id;
            _repo.RemoveLane(lane.Id, fallback);
            await _repo.SaveAsync();
            Rebuild();
        }

        // ----------- Card edit -----------
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

        // ----------- Card create/delete -----------
        public async Task<Card> CreateCard(string laneId, string? assetTag, string? model, string? serial)
        {
            var card = _repo.AddCard(laneId, assetTag, model, serial);
            await _repo.SaveAsync();

            CollectionFor(laneId).Add(card);
            return card;
        }

        public async Task DeleteCard(Card c)
        {
            var laneId = c.LaneId;

            _repo.RemoveCard(c.Id);
            await _repo.SaveAsync();

            CollectionFor(laneId).Remove(c);
        }
    }
}
