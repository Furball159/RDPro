using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using RDPro.ViewModels;
using RDPro.Models;
using RDPro.Services;
using System;
using System.Windows.Input;

namespace RDPro.Views
{
    public partial class FavouritesPage : UserControl
    {
        public FavouritesPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Item_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // handle double-click to open rdp file
            if (e.ClickCount >= 2 && e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                var control = sender as Avalonia.Controls.Control;
                if (control?.DataContext is ConnectionDetails details)
                {
                    var vm = this.DataContext as FavouritesViewModel;
                    try
                    {
                        if (vm != null && vm.ShowRdpFileCommand != null && vm.ShowRdpFileCommand.CanExecute(details))
                        {
                            vm.ShowRdpFileCommand.Execute(details);
                        }
                    }
                    catch (Exception ex)
                    {
                        try { Console.WriteLine($"Error executing ShowRdpFileCommand on favourites: {ex.Message}"); } catch { }
                    }
                }
            }
        }

        private void Item_ContextRequested(object? sender, ContextRequestedEventArgs e)
        {
            var control = sender as Avalonia.Controls.Control;
            if (control?.DataContext is ConnectionDetails details)
            {
                var vm = this.DataContext as FavouritesViewModel;
                if (vm != null)
                {
                    try
                    {
                        ShowActionsPopup(control, details, vm);
                        if (e != null) e.Handled = true;
                    }
                    catch (Exception ex)
                    {
                        try { Console.WriteLine($"Favourites context request error: {ex.Message}"); } catch { }
                    }
                }
            }
        }

        private void Item_ActionsButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                if (btn?.DataContext is ConnectionDetails details)
                {
                    var vm = this.DataContext as FavouritesViewModel;
                    if (vm != null)
                    {
                        var btnControl = btn as Avalonia.Controls.Control;
                        ShowActionsPopup(btnControl, details, vm);
                    }
                }
            }
            catch (Exception ex)
            {
                try { Console.WriteLine($"Favourites actions click error: {ex.Message}"); } catch { }
            }
        }

        private void ShowActionsPopup(Avalonia.Controls.Control? placementTarget, ConnectionDetails details, FavouritesViewModel vm)
        {
            try
            {
                Popup popup = null!;

                var panel = new StackPanel { Orientation = Orientation.Vertical };

                Button MakeButton(string text, ICommand? command)
                {
                    var b = new Button
                    {
                        Content = text,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(8, 6)
                    };

                    b.Click += (_, __) =>
                    {
                        try
                        {
                            if (popup != null) popup.IsOpen = false;
                            if (command != null && command.CanExecute(details))
                                command.Execute(details);
                        }
                        catch (Exception ex)
                        {
                            try { Console.WriteLine($"Favourites popup button error: {ex.Message}"); } catch { }
                        }
                    };

                    return b;
                }

                panel.Children.Add(MakeButton("Connect", vm.ConnectCommand));
                panel.Children.Add(MakeButton("Connect Without RD Gateway", vm.ConnectWithoutGatewayCommand));
                panel.Children.Add(new Border { Height = 1, Background = Brushes.LightGray, Margin = new Thickness(4, 6) });
                panel.Children.Add(MakeButton("Edit Connection", vm.EditConnectionCommand));
                panel.Children.Add(new Border { Height = 1, Background = Brushes.LightGray, Margin = new Thickness(4, 6) });
                panel.Children.Add(MakeButton("View .RDPX File", vm.ShowRdpXFileCommand));
                panel.Children.Add(MakeButton("View .RDP File", vm.ShowRdpFileCommand));
                panel.Children.Add(new Border { Height = 1, Background = Brushes.LightGray, Margin = new Thickness(4, 6) });
                panel.Children.Add(MakeButton(details.IsFavourite ? "Remove from Favourites" : "Add to Favourites", vm.ToggleFavouriteCommand));
                panel.Children.Add(new Border { Height = 1, Background = Brushes.LightGray, Margin = new Thickness(4, 6) });
                panel.Children.Add(MakeButton("Remove Connection", vm.RemoveSpecificConnectionCommand));

                popup = new Popup
                {
                    Child = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Child = panel
                    },
                    PlacementTarget = placementTarget ?? this,
                    IsLightDismissEnabled = true
                };

                popup.IsOpen = true;
            }
            catch (Exception ex)
            {
                try { Console.WriteLine($"ShowActionsPopup (favourites) error: {ex.Message}"); } catch { }
            }
        }
    }
}
