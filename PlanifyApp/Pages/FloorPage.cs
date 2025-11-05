using System.IO;
using IOPath = System.IO.Path;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Storage;

using Planify.Models;
using Planify.Services;
using Planify.ViewModels;

namespace Planify.Pages
{
    public class FloorPage : ContentPage
    {
        private readonly FloorViewModel _vm;
        private readonly Picker _floorPicker;
        private readonly AbsoluteLayout _canvas;
        private readonly Image _background;

        public FloorPage()
        {
            Title = "Floors";

            var repo = new AppRepository();
            _vm = new FloorViewModel(repo);

            // --- topbar ---
            _floorPicker = new Picker { Title = "Vælg etage", WidthRequest = 200 };
            _floorPicker.ItemDisplayBinding = new Binding(nameof(FloorPlan.Name));
            _floorPicker.SelectedIndexChanged += (_, __) =>
            {
                if (_floorPicker.SelectedItem is FloorPlan f)
                {
                    _vm.SelectFloor(f);
                    RenderFloor();
                }
            };

            var addFloorBtn = new Button { Text = "+ Etage" };
            addFloorBtn.Clicked += async (_, __) =>
            {
                var name = await DisplayPromptAsync("Ny etage", "Navn (fx 1. sal):");
                if (string.IsNullOrWhiteSpace(name)) return;

                var floor = await _vm.AddFloor(name);
                BuildFloorPickerSelection(floor);
                RenderFloor();
            };

            var uploadBtn = new Button { Text = "Upload floorplan" };
            uploadBtn.Clicked += async (_, __) =>
            {
                if (_vm.CurrentFloor == null) return;

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Vælg floorplan-billede",
                    FileTypes = FilePickerFileType.Images
                });

                if (result == null) return;

                var dir = IOPath.Combine(FileSystem.AppDataDirectory, "floorplans");
                Directory.CreateDirectory(dir);
                var dest = IOPath.Combine(dir, result.FileName);

                using (var src = await result.OpenReadAsync())
                using (var dst = File.Open(dest, FileMode.Create, FileAccess.Write))
                {
                    await src.CopyToAsync(dst);
                }

                await _vm.SetImageForCurrent(dest);
                RenderFloor();
            };

            var addTableBtn = new Button { Text = "+ Bord" };
            addTableBtn.Clicked += async (_, __) =>
            {
                await _vm.AddTable();
                RenderFloor();
            };

            var headerRow = new HorizontalStackLayout
            {
                Padding = new Thickness(16, 8),
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "Floorplan (MVP)",
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold
                    },
                    _floorPicker,
                    addFloorBtn,
                    uploadBtn,
                    addTableBtn
                }
            };

            // Billedet fylder hele canvas – vi strækker det for at få stabile koordinater
            _background = new Image { Aspect = Aspect.Fill };

            _canvas = new AbsoluteLayout
            {
                BackgroundColor = Color.FromArgb("#f5f7fa"),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            _canvas.Children.Add(_background);
            AbsoluteLayout.SetLayoutBounds(_background, new Rect(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(_background, AbsoluteLayoutFlags.All);

            // Re-render når størrelsen ændrer sig
            _canvas.SizeChanged += (_, __) =>
            {
                if (_canvas.Width > 0 && _canvas.Height > 0)
                    RenderFloor();
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            grid.Children.Add(headerRow);
            Grid.SetRow(headerRow, 0);

            grid.Children.Add(_canvas);
            Grid.SetRow(_canvas, 1);

            Content = grid;

            Appearing += async (_, __) =>
            {
                await _vm.InitAsync();
                BuildFloorPickerSelection(_vm.CurrentFloor);
                RenderFloor();
            };
        }

        private void BuildFloorPickerSelection(FloorPlan? selected)
        {
            _floorPicker.ItemsSource = _vm.Floors;
            if (selected != null)
                _floorPicker.SelectedItem = selected;
            else if (_vm.Floors.Count > 0)
                _floorPicker.SelectedItem = _vm.Floors[0];
        }

        private void RenderFloor()
        {
            if (_canvas.Width <= 0 || _canvas.Height <= 0)
                return;

            _canvas.Children.Clear();
            _canvas.Children.Add(_background);
            AbsoluteLayout.SetLayoutBounds(_background, new Rect(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(_background, AbsoluteLayoutFlags.All);

            var img = _vm.CurrentImagePath;
            _background.Source = string.IsNullOrWhiteSpace(img)
                ? null
                : ImageSource.FromFile(img);

            if (_vm.CurrentFloor == null) return;

            var w = _canvas.Width;
            var h = _canvas.Height;

            foreach (var table in _vm.Tables)
            {
                var view = CreateTableView(table);

                // X/Y er RELATIVE (0-1) → skaler til canvas
                var x = table.X * w;
                var y = table.Y * h;

                _canvas.Children.Add(view);
                AbsoluteLayout.SetLayoutBounds(view, new Rect(x, y, table.Width, table.Height));
                AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.None);
            }
        }

        private View CreateTableView(Table t)
        {
            var border = new Border
            {
                Background = Colors.White,
                Stroke = Colors.DarkSlateGray,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                Padding = 4
            };

            var stack = new VerticalStackLayout { Spacing = 2 };

            if (t.Seats.Count == 0)
            {
                stack.Children.Add(new Label
                {
                    Text = $"{t.Id}   Nyt bord",
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold
                });
            }
            else
            {
                foreach (var seat in t.Seats)
                {
                    stack.Children.Add(new Label
                    {
                        Text = $"LOCATER {seat.LocaterId}  {seat.Role}",
                        FontSize = 11,
                        FontAttributes = FontAttributes.Bold
                    });

                    foreach (var card in _vm.CardsForSeat(seat))
                    {
                        stack.Children.Add(new Label
                        {
                            Text = $"• {card.AssetTag}  {card.Model}  ({_vm.StatusText(card)})",
                            FontSize = 11
                        });
                    }

                    stack.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        BackgroundColor = Colors.LightGray
                    });
                }

                if (stack.Children.Count > 0)
                    stack.Children.RemoveAt(stack.Children.Count - 1);
            }

            border.Content = stack;

            double startX = 0;
            double startY = 0;

            var pan = new PanGestureRecognizer();
            pan.PanUpdated += async (s, e) =>
            {
                switch (e.StatusType)
                {
                    case GestureStatus.Started:
                        var b = AbsoluteLayout.GetLayoutBounds(border);
                        startX = b.X;
                        startY = b.Y;
                        break;

                    case GestureStatus.Running:
                        var newX = startX + e.TotalX;
                        var newY = startY + e.TotalY;
                        AbsoluteLayout.SetLayoutBounds(border,
                            new Rect(newX, newY, t.Width, t.Height));
                        break;

                    case GestureStatus.Completed:
                    case GestureStatus.Canceled:
                        var b2 = AbsoluteLayout.GetLayoutBounds(border);
                        var w = _canvas.Width;
                        var h = _canvas.Height;
                        if (w > 0 && h > 0)
                        {
                            // gem som relative koordinater (0-1)
                            var relX = b2.X / w;
                            var relY = b2.Y / h;
                            relX = Math.Clamp(relX, 0, 1);
                            relY = Math.Clamp(relY, 0, 1);
                            await _vm.UpdateTablePosition(t, relX, relY);
                        }
                        break;
                }
            };
            border.GestureRecognizers.Add(pan);

            return border;
        }
    }
}
