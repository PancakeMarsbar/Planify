using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Planify.Models;
using Planify.Services;

namespace Planify.ViewModels.V2
{
    public sealed class BoardViewModel
    {
        private readonly AppRepository _repo;
        private bool _refreshing;

        public ObservableCollection<BoardLane> Lanes { get; } = new();
        public Dictionary<string, ObservableCollection<Card>> CardsByLane { get; } = new();

        public BoardViewModel(AppRepository repo) => _repo = repo;

        public async Task InitAsync()
        {
            await _repo.LoadAsync();
            Rebuild();
        }
        private void Rebuild()
        {
            Lanes.Clear();
            foreach (var l in _repo.Lanes.OrderBy(l => l.Order))
                Lanes.Add(l);

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
        }

        public async Task RemoveLane(BoardLane lane)
        {
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
                case "AssetTag":
                    c.AssetTag = value ?? "";
                    break;

                case "Model":
                    c.Model = value ?? "";
                    break;

                case "Serial":
                    c.Serial = value ?? "";
                    break;

                case "PersonName":
                    {
                        var newName = value ?? "";
                        c.PersonName = newName;

                        // Hvis kortet er knyttet til et LOCATER (Table.Id),
                        // så skal ALLE kort + selve bordet have samme navn.
                        if (!string.IsNullOrWhiteSpace(c.LocaterId))
                        {
                            var locId = c.LocaterId;

                            // 1) Opdatér alle kort på samme LOCATER-ID
                            foreach (var other in _repo.Cards.Where(x =>
                                         string.Equals(x.LocaterId, locId, StringComparison.OrdinalIgnoreCase)))
                            {
                                other.PersonName = newName;
                            }

                            // 2) Opdatér alle borde (Tables) med samme Id
                            foreach (var floor in _repo.Floors)
                            {
                                foreach (var table in floor.Tables.Where(t =>
                                             string.Equals(t.Id, locId, StringComparison.OrdinalIgnoreCase)))
                                {
                                    table.Name = newName;
                                }
                            }
                        }
                        break;
                    }

                case "LocaterId":
                    c.LocaterId = value ?? "";
                    break;

                case "Deadline":
                    c.SetupDeadline = DateTime.TryParse(value, out var dt) ? dt : null;
                    break;
            }

            await _repo.SaveAsync();

            // Tving refresh af lane-listen for det kort
            var col = CollectionFor(c.LaneId);
            if (col.Contains(c))
            {
                col.Remove(c);
                col.Add(c);
            }
        }

        // ----------- LOCATER: bind til Table -----------
        public async Task PickTableForCard(Card c)
        {
            // Saml alle borde fra alle etager
            var tables = _repo.Floors
                .SelectMany(f => f.Tables)
                .ToList();

            if (tables.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ingen borde",
                    "Der er ingen borde oprettet på floorplan.",
                    "OK");
                return;
            }

            // Vis: "T-01   Klipperum"
            var options = tables
                .Select(t => $"{t.Id}   {t.Name}")
                .ToList();

            var choice = await Application.Current.MainPage.DisplayActionSheet(
                "Vælg bord (LOCATER-ID)",
                "Luk", null,
                options.ToArray()
            );

            if (string.IsNullOrWhiteSpace(choice)) return;

            var index = options.IndexOf(choice);
            if (index < 0 || index >= tables.Count) return;

            var selectedTable = tables[index];

            // LOCATER = Table.Id, person = Table.Name
            c.LocaterId = selectedTable.Id;
            c.PersonName = selectedTable.Name;

            await _repo.SaveAsync();

            var col = CollectionFor(c.LaneId);
            if (col.Contains(c))
            {
                col.Remove(c);
                col.Add(c);
            }
        }

        // ----------- LOCATER: ryd igen -----------
        public async Task ClearLocaterForCard(Card c)
        {
            c.LocaterId = "";
            c.PersonName = "";

            await _repo.SaveAsync();

            var col = CollectionFor(c.LaneId);
            if (col.Contains(c))
            {
                col.Remove(c);
                col.Add(c);
            }
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
