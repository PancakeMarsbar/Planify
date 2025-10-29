using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Planify.Models;
using Planify.Services;
using Planify.ViewModels;

namespace Planify.Pages
{
    public class FloorPage : ContentPage
    {
        private readonly FloorViewModel _vm;

        public FloorPage()
        {
            Title = "Floors";
            _vm = new FloorViewModel(new AppRepository());

            var canvas = new Grid
            {
                BackgroundColor = Color.FromArgb("#F4F6F8"),
                HeightRequest = 600,
                WidthRequest = 900
            };

            var seatsLayer = new AbsoluteLayout();
            canvas.Children.Add(seatsLayer);

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 12,
                    Spacing = 12,
                    Children =
                    {
                        new Label{ Text="Floorplan (MVP)", Style=(Style)Application.Current!.Resources["H1"] },
                        canvas
                    }
                }
            };

            Appearing += async (_, __) =>
            {
                try
                {
                    await _vm.InitAsync();
                    seatsLayer.Children.Clear();

                    foreach (var s in _vm.Seats)
                    {
                        var view = SeatView(s);
                        seatsLayer.Children.Add(view);
                        AbsoluteLayout.SetLayoutBounds(view, new Rect(s.X, s.Y, 200, 130));
                        AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.None);
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Floor error", ex.Message, "OK");
                }
            };
        }

        private View SeatView(Seat s)
        {
            var border = new Border
            {
                Padding = 6,
                Stroke = Colors.DarkSlateGray,
                StrokeThickness = 1,
                Background = Colors.White
            };

            var header = new Label { Text = $"LOCATER {s.LocaterId}", FontAttributes = FontAttributes.Bold };
            var role = new Label
            {
                Text = string.IsNullOrWhiteSpace(s.Role) ? "—" : s.Role,
                Style = (Style)Application.Current!.Resources["Small"]
            };

            border.Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    header, role,
                    new BoxView{ HeightRequest=1, Color=Colors.LightGray },
                    MachineListForSeat(s)
                }
            };

            // Tap → tildel person
            var tap = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            tap.Tapped += async (_, __) =>
            {
                string person = await DisplayPromptAsync("Tildel person", "Navn:");
                if (!string.IsNullOrWhiteSpace(person))
                    await _vm.AssignPerson(s, person);
            };
            border.GestureRecognizers.Add(tap);

            return border;
        }

        private View MachineListForSeat(Seat s)
        {
            var list = new VerticalStackLayout { Spacing = 2 };
            foreach (var c in _vm.CardsForSeat(s))
            {
                var line = new HorizontalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label{ Text=c.AssetTag, FontAttributes=FontAttributes.Bold },
                        new Label{ Text=c.Model, Style=(Style)Application.Current!.Resources["Small"] },
                        new Label{ Text=$"({c.Status})", Style=(Style)Application.Current!.Resources["Small"] }
                    }
                };

                if (c.DeadlineRed || c.DeadlineYellow)
                {
                    line.Children.Insert(0, new BoxView
                    {
                        WidthRequest = 8,
                        HeightRequest = 8,
                        CornerRadius = 4,
                        Color = c.DeadlineRed ? Colors.Red : Colors.Gold,
                        VerticalOptions = LayoutOptions.Center
                    });
                }

                list.Children.Add(line);
            }
            return list;
        }
    }
}
