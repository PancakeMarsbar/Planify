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
    public class FloorPageGuest : ContentPage
    {
        private const double DesignWidth = 1400;
        private const double DesignHeight = 900;

        private readonly FloorViewModel _vm;
        private readonly Picker _floorPicker;

        private readonly AbsoluteLayout _canvas;
        private readonly Image _background;

        private readonly Grid _zoomRoot;
        private readonly ContentView _scaleContainer;
        private readonly ScrollView _scroll;

        private readonly Slider _zoomSlider;
        private readonly Label _zoomValueLabel;
        private double _zoom = 1.0;

        public FloorPageGuest()
        {
            Title = "Floors";

            var repo = AppRepository.Instance;
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

            var BackBtn = new Button { Text = "Back" };
            BackBtn.Clicked += async (_, __) =>
            {
                Application.Current.MainPage = new LoginPage();
            };


            // Zoom UI
            _zoomSlider = new Slider { Minimum = 0.5, Maximum = 2.0, Value = 1.0, WidthRequest = 140 };
            _zoomValueLabel = new Label { Text = "100%", VerticalTextAlignment = TextAlignment.Center };

            _zoomSlider.ValueChanged += (_, e) =>
            {
                _zoom = e.NewValue;
                _zoomValueLabel.Text = $"{(int)(_zoom * 100)}%";
                ApplyZoom();
            };

            var headerRow = new HorizontalStackLayout
            {
                Padding = new Thickness(16, 8),
                Spacing = 12,
                Children =
                {
                    new Label { Text = "Floorplan", FontSize = 24, FontAttributes = FontAttributes.Bold },
                    _floorPicker,
                    BackBtn,
                    new Label { Text = "Zoom:", VerticalTextAlignment = TextAlignment.Center },
                    _zoomSlider,
                    _zoomValueLabel
                }
            };

            // Canvas (fast størrelse)
            _canvas = new AbsoluteLayout
            {
                WidthRequest = DesignWidth,
                HeightRequest = DesignHeight,
                BackgroundColor = Color.FromArgb("#f5f7fa")
            };

            // ✅ Aspect ratio fix
            _background = new Image { Aspect = Aspect.AspectFit };

            _canvas.Children.Add(_background);
            AbsoluteLayout.SetLayoutBounds(_background, new Rect(0, 0, DesignWidth, DesignHeight));
            AbsoluteLayout.SetLayoutFlags(_background, AbsoluteLayoutFlags.None);

            // Zoom container
            _scaleContainer = new ContentView
            {
                Content = _canvas,
                AnchorX = 0,
                AnchorY = 0
            };

            _zoomRoot = new Grid
            {
                WidthRequest = DesignWidth,
                HeightRequest = DesignHeight
            };
            _zoomRoot.Children.Add(_scaleContainer);

            _scroll = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
                VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                Content = _zoomRoot
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            grid.Children.Add(headerRow);
            Grid.SetRow(headerRow, 0);

            grid.Children.Add(_scroll);
            Grid.SetRow(_scroll, 1);

            Content = grid;

            Appearing += async (_, __) =>
            {
                await _vm.InitAsync();
                BuildFloorPickerSelection(_vm.CurrentFloor);
                RenderFloor();
                ApplyZoom();
            };
        }

        private void ApplyZoom()
        {
            _scaleContainer.Scale = _zoom;
            _zoomRoot.WidthRequest = DesignWidth * _zoom;
            _zoomRoot.HeightRequest = DesignHeight * _zoom;
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
            _canvas.Children.Clear();

            _canvas.Children.Add(_background);
            AbsoluteLayout.SetLayoutBounds(_background, new Rect(0, 0, DesignWidth, DesignHeight));
            AbsoluteLayout.SetLayoutFlags(_background, AbsoluteLayoutFlags.None);

            var img = _vm.CurrentImagePath;
            _background.Source = string.IsNullOrWhiteSpace(img) ? null : ImageSource.FromFile(img);

            if (_vm.CurrentFloor == null) return;

            var w = DesignWidth;
            var h = DesignHeight;

            foreach (var table in _vm.Tables)
            {
                var view = CreateTableView(table);
                var x = table.X * w;
                var y = table.Y * h;

                _canvas.Children.Add(view);
                AbsoluteLayout.SetLayoutBounds(view, new Rect(x, y, table.Width, table.Height));
                AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.None);
            }
        }

        private View CreateTableView(Table t)
        {
            var innerStack = new VerticalStackLayout { Spacing = 2 };

            if (t.Seats.Count == 0)
            {
                innerStack.Children.Add(new Label
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
                    innerStack.Children.Add(new Label
                    {
                        Text = $"LOCATER {seat.LocaterId}  {seat.Role}",
                        FontSize = 11,
                        FontAttributes = FontAttributes.Bold
                    });



                    innerStack.Children.Add(new BoxView { HeightRequest = 1, BackgroundColor = Colors.LightGray });
                }
                if (innerStack.Children.Count > 0)
                    innerStack.Children.RemoveAt(innerStack.Children.Count - 1);
            }

            var border = new Border
            {
                Background = Colors.White,
                Stroke = Colors.DarkSlateGray,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                Padding = 4,
                Content = innerStack,
            };

            var menuBtn = new Button
            {
                Text = "⋮",
                FontSize = 12,
                WidthRequest = 22,
                HeightRequest = 22,
                Padding = 0,
                BackgroundColor = Color.FromArgb("#EFEFEF")
            };
            menuBtn.Clicked += async (_, __) =>
            {
                var choice = await DisplayActionSheet("Bord", "Luk", null,
                    "Duplikér (samme størrelse)"
                );

                if (choice == "Duplikér (samme størrelse)")
                {
                    await _vm.DuplicateTable(t);
                    RenderFloor();
                }
            };

            var container = new Grid
            {
                RowDefinitions = { new RowDefinition { Height = GridLength.Star } },
                ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star } }
            };

            container.Children.Add(border);

            return container;
        }
    }
}
