using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Linq;
using Planify.Models;
using Planify.Services;
// Brug den nye VM i V2-namespace (for at undgå kollision med gammel VM)
using V2 = Planify.ViewModels.V2;

namespace Planify.Pages
{
    public class BoardPage : ContentPage
    {
        private readonly V2.BoardViewModel _vm;

        public BoardPage()
        {
            Title = "Board";

            var repo = new AppRepository();
            _vm = new V2.BoardViewModel(repo);

            var lanesHost = new HorizontalStackLayout { Spacing = 12, Padding = 12 };
            Content = new ScrollView { Content = lanesHost };

            Appearing += async (_, __) =>
            {
                await _vm.InitAsync();
                BuildColumns(lanesHost);
            };
            Disappearing += (_, __) => _vm.Teardown();
        }

        private void BuildColumns(HorizontalStackLayout host)
        {
            host.Children.Clear();

            foreach (var lane in _vm.Lanes)
            {
                var header = new HorizontalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Label { Text = lane.Title, FontAttributes = FontAttributes.Bold },
                        LaneMenuButton(lane)
                    }
                };

                var list = new CollectionView
                {
                    ItemsSource = _vm.CardsByLane[lane.Id],
                    ItemTemplate = new DataTemplate(() => CardView(lane)),
                    SelectionMode = SelectionMode.None,
                    WidthRequest = 280
                };

                host.Children.Add(new VerticalStackLayout
                {
                    WidthRequest = 300,
                    Children = { header, list }
                });
            }
        }

        // === Kolonne-menu (kun ét prompt ved "Tilføj kort") ===
        private View LaneMenuButton(BoardLane lane)
        {
            var btn = new Button { Text = "⋯", WidthRequest = 28, HeightRequest = 28, Padding = 0, FontSize = 14 };
            btn.Clicked += async (_, __) =>
            {
                var options = new System.Collections.Generic.List<string>
                {
                    "Tilføj kort",          // <- ét prompt
                    "Omdøb kolonne",
                    "Tilføj kolonne"
                };
                if (_vm.Lanes.Count > 1) options.Add("Fjern kolonne");

                var choice = await DisplayActionSheet("Kolonne", "Luk", null, options.ToArray());
                switch (choice)
                {
                    case "Tilføj kort":
                        {
                            var tag = await DisplayPromptAsync("Nyt kort", "Navn (AssetTag):");
                            if (!string.IsNullOrWhiteSpace(tag))
                            {
                                await _vm.CreateCard(lane.Id, tag, null, null);

                                // Scroll en smule hen mod kolonnen så brugeren ser kortet
                                if (this.Content is ScrollView sv)
                                {
                                    var index = _vm.Lanes.OrderBy(l => l.Order).ToList().FindIndex(l => l.Id == lane.Id);
                                    if (index >= 0) await sv.ScrollToAsync(index * 300, 0, true);
                                }
                            }
                            break;
                        }
                    case "Omdøb kolonne":
                        {
                            var txt = await DisplayPromptAsync("Omdøb", "Nyt navn:", initialValue: lane.Title);
                            if (!string.IsNullOrWhiteSpace(txt)) await _vm.RenameLane(lane, txt);
                            break;
                        }
                    case "Tilføj kolonne":
                        {
                            var txt = await DisplayPromptAsync("Ny kolonne", "Navn:");
                            if (!string.IsNullOrWhiteSpace(txt))
                            {
                                await _vm.AddLane(txt);
                                if (this.Content is ScrollView sv && sv.Content is HorizontalStackLayout host) BuildColumns(host);
                            }
                            break;
                        }
                    case "Fjern kolonne":
                        {
                            await _vm.RemoveLane(lane);
                            if (this.Content is ScrollView sv && sv.Content is HorizontalStackLayout host) BuildColumns(host);
                            break;
                        }
                }
            };
            return btn;
        }

        private View CardView(BoardLane lane)
        {
            var border = new Border
            {
                Padding = 8,
                Margin = 6,
                Stroke = Colors.LightGray,
                StrokeThickness = 1,
                Background = Colors.White
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition{ Width = GridLength.Auto },
                    new ColumnDefinition{ Width = GridLength.Star },
                    new ColumnDefinition{ Width = GridLength.Auto }
                }
            };

            // Venstre pil (←)
            var leftBtn = new Button { Text = "←", WidthRequest = 36, HeightRequest = 36, Padding = 0 };
            leftBtn.Clicked += async (_, __) =>
            {
                if (border.BindingContext is not Card c) return;
                var lanes = _vm.Lanes.OrderBy(x => x.Order).ToList();
                var idx = lanes.FindIndex(l => l.Id == lane.Id);
                if (idx > 0) await _vm.Move(c, lanes[idx - 1].Id);
            };

            // Indhold
            var asset = new Label { FontAttributes = FontAttributes.Bold };
            asset.SetBinding(Label.TextProperty, nameof(Card.AssetTag));
            var model = new Label { Style = (Style)Application.Current!.Resources["Small"] };
            model.SetBinding(Label.TextProperty, nameof(Card.Model));
            var loc = new Label { Style = (Style)Application.Current!.Resources["Small"] };
            loc.SetBinding(Label.TextProperty, nameof(Card.LocaterId));
            var status = new Label { Style = (Style)Application.Current!.Resources["Small"] };
            status.SetBinding(Label.TextProperty, nameof(Card.Status));
            var deadline = new Label { Style = (Style)Application.Current!.Resources["Small"] };
            deadline.SetBinding(Label.TextProperty, new Binding(nameof(Card.SetupDeadline), stringFormat: "Deadline: {0:yyyy-MM-dd}"));
            var content = new VerticalStackLayout { Spacing = 2, Children = { asset, model, loc, status, deadline } };

            // Højre side
            var badge = new BoxView { WidthRequest = 8, HeightRequest = 8, CornerRadius = 4, VerticalOptions = LayoutOptions.Center };
            badge.SetBinding(BoxView.ColorProperty, new MultiBinding
            {
                Bindings = { new Binding(nameof(Card.DeadlineRed)), new Binding(nameof(Card.DeadlineYellow)) },
                Converter = new DeadlineToColor()
            });

            var rightBtn = new Button { Text = "→", WidthRequest = 36, HeightRequest = 36, Padding = 0 };
            rightBtn.Clicked += async (_, __) =>
            {
                if (border.BindingContext is not Card c) return;
                var lanes = _vm.Lanes.OrderBy(x => x.Order).ToList();
                var idx = lanes.FindIndex(l => l.Id == lane.Id);
                if (idx < lanes.Count - 1) await _vm.Move(c, lanes[idx + 1].Id);
            };

            var menuBtn = new Button { Text = "⋯", WidthRequest = 36, HeightRequest = 36, Padding = 0, FontSize = 18 };
            menuBtn.Clicked += async (_, __) =>
            {
                if (border.BindingContext is not Card c) return;

                var action = await DisplayActionSheet("Kort", "Luk", null,
                    "Redigér Navn (AssetTag)", "Redigér Model", "Redigér Serienr.",
                    "Redigér Person", "Redigér LOCATER-ID", "Redigér Deadline", "Slet kort");

                if (action == "Redigér Navn (AssetTag)")
                    await _vm.EditCardField(c, "AssetTag", await DisplayPromptAsync("Navn", "AssetTag:", initialValue: c.AssetTag));
                else if (action == "Redigér Model")
                    await _vm.EditCardField(c, "Model", await DisplayPromptAsync("Model", "Model:", initialValue: c.Model));
                else if (action == "Redigér Serienr.")
                    await _vm.EditCardField(c, "Serial", await DisplayPromptAsync("Serienr.", "Serienr.:", initialValue: c.Serial));
                else if (action == "Redigér Person")
                    await _vm.EditCardField(c, "PersonName", await DisplayPromptAsync("Person", "Navn:", initialValue: c.PersonName));
                else if (action == "Redigér LOCATER-ID")
                    await _vm.EditCardField(c, "LocaterId", await DisplayPromptAsync("LOCATER-ID", "Fx 0.3.5:", initialValue: c.LocaterId));
                else if (action == "Redigér Deadline")
                    await _vm.EditCardField(c, "Deadline", await DisplayPromptAsync("Deadline", "YYYY-MM-DD:", initialValue: c.SetupDeadline?.ToString("yyyy-MM-dd")));
                else if (action == "Slet kort")
                {
                    var ok = await DisplayActionSheet("Slet dette kort?", "Annuller", null, "Ja, slet");
                    if (ok == "Ja, slet")
                        await _vm.DeleteCard(c);
                }

                // Flyt til bestemt kolonne
                var lanes = _vm.Lanes.OrderBy(x => x.Order).ToList();
                var titles = lanes.Select(l => l.Title + (l.Id == lane.Id ? " ✓" : "")).ToArray();
                var moveChoice = await DisplayActionSheet("Flyt til …", "Luk", null, titles);
                var clean = moveChoice?.Replace(" ✓", "");
                var dest = lanes.FirstOrDefault(l => l.Title == clean);
                if (dest != null) await _vm.Move(c, dest.Id);
            };

            var right = new HorizontalStackLayout { Spacing = 6, Children = { badge, rightBtn, menuBtn } };

            grid.Children.Add(leftBtn); Microsoft.Maui.Controls.Grid.SetColumn(leftBtn, 0);
            grid.Children.Add(content); Microsoft.Maui.Controls.Grid.SetColumn(content, 1);
            grid.Children.Add(right); Microsoft.Maui.Controls.Grid.SetColumn(right, 2);

            border.BindingContextChanged += (_, __) =>
            {
                if (border.BindingContext is Card cc)
                {
                    if (cc.DeadlineRed) border.Background = Color.FromArgb("#ffe6e6");
                    else if (cc.DeadlineYellow) border.Background = Color.FromArgb("#fff7cc");
                    else border.Background = Colors.White;
                }
            };

            border.Content = grid;
            return border;
        }
    }

    // Badge converter
    public sealed class DeadlineToColor : IMultiValueConverter
    {
        public object Convert(object[]? values, System.Type targetType, object? parameter, System.Globalization.CultureInfo? culture)
        {
            bool red = values is { Length: > 0 } && values![0] is bool b1 && b1;
            bool yellow = values is { Length: > 1 } && values![1] is bool b2 && b2;
            if (red) return Colors.Red;
            if (yellow) return Colors.Gold;
            return Colors.Transparent;
        }

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}
