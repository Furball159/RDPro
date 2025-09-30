using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Avalonia.Markup.Xaml;
using RDPro.ViewModels;
using RDPro.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace RDPro.Views
{
    public partial class AllConnectionsPage : UserControl 
    {
        public AllConnectionsPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ConnectionsListBox_DoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // retained for compatibility - not used by item-level DoubleTapped
        }

        private void Item_DoubleTapped(object? sender, TappedEventArgs e)
        {
            // legacy - not used
        }

            private void Item_PointerPressed(object? sender, PointerPressedEventArgs e)
            {
                // Debug logging to see click counts and which item was clicked
                try
                {
                    System.Console.WriteLine($"Item_PointerPressed: ClickCount={e.ClickCount}, Sender={sender?.GetType().Name}");
                }
                catch { }

                // Detect left-button double-click and invoke the ViewModel command to open the .rdp file
                if (e.ClickCount >= 2 && e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                {
                    // The DataTemplate root is a Grid; cast to Control so it works for Grid, StackPanel, etc.
                    var control = sender as Avalonia.Controls.Control;
                    if (control?.DataContext is ConnectionDetails details)
                    {
                        var vm = this.DataContext as AllConnectionsViewModel;
                        try
                        {
                            if (vm is not null && vm.ShowRdpFileCommand is not null && vm.ShowRdpFileCommand.CanExecute(details))
                            {
                                vm.ShowRdpFileCommand.Execute(details);
                            }
                            else
                            {
                                System.Console.WriteLine("ShowRdpFileCommand not available or cannot execute");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine($"Error executing ShowRdpFileCommand: {ex.Message}");
                        }
                    }
                }
            }

    private void Item_ContextRequested(object? sender, ContextRequestedEventArgs e)
        {
            try
            {
                System.Console.WriteLine($"Item_ContextRequested: Sender={sender?.GetType().Name}");
            }
            catch { }

            var control = sender as Avalonia.Controls.Control;
            if (control?.DataContext is ConnectionDetails details)
            {
                var vm = this.DataContext as AllConnectionsViewModel;
                if (vm != null)
                {
                        try
                        {
                            // Always show the Popup (unify hamburger and right-click behavior).
                            ShowActionsPopup(control, details, vm);
                            if (e != null) e.Handled = true;
                        }
                    catch (Exception ex)
                    {
                        try { Console.WriteLine($"Item_ContextRequested error: {ex.Message}"); } catch { }
                    }
                }
            }
        }

        private bool ShowProgrammaticMenu(Avalonia.Controls.Control placementTarget, ConnectionDetails details, AllConnectionsViewModel vm)
        {
            try
            {
                var programmatic = new ContextMenu();
                var list = programmatic.Items as System.Collections.IList;
                if (list == null) return false;

                list.Add(new MenuItem { Header = "Connect", Command = vm.ConnectCommand, CommandParameter = details });
                list.Add(new MenuItem { Header = "Connect Without RD Gateway", Command = vm.ConnectWithoutGatewayCommand, CommandParameter = details });
                list.Add(new Separator());
                list.Add(new MenuItem { Header = "Edit Connection", Command = vm.EditConnectionCommand, CommandParameter = details });
                list.Add(new Separator());
                list.Add(new MenuItem { Header = "View .RDPX File", Command = vm.ShowRdpXFileCommand, CommandParameter = details });
                list.Add(new MenuItem { Header = "View .RDP File", Command = vm.ShowRdpFileCommand, CommandParameter = details });
                list.Add(new Separator());
                list.Add(new MenuItem { Header = details.IsFavourite ? "Remove from Favourites" : "Add to Favourites", Command = vm.ToggleFavouriteCommand, CommandParameter = details });
                list.Add(new Separator());
                // Wrap delete in a confirm step by assigning a click handler that shows a confirmation dialog.
                var deleteItem = new MenuItem { Header = "Delete" };
                deleteItem.Click += async (_, __) =>
                {
                    try
                    {
                        bool ok = await RDPro.Services.UiService.ShowConfirmAsync("Delete Connection", $"Delete connection '{details.ConnectionName}'? This cannot be undone.");
                        if (ok && vm.DeleteConnectionCommand.CanExecute(details)) vm.DeleteConnectionCommand.Execute(details);
                    }
                    catch (Exception ex)
                    {
                        try { Console.WriteLine($"Delete menu click error: {ex.Message}"); } catch { }
                    }
                };

                list.Add(deleteItem);

                programmatic.Open(placementTarget);
                return true;
            }
            catch (Exception ex)
            {
                try { Console.WriteLine($"Programmatic ContextMenu failed: {ex.Message}"); } catch { }
                return false;
            }
        }

        // Build and show a Popup anchored to placementTarget with action Buttons bound to the ViewModel commands.
        private void ShowActionsPopup(Avalonia.Controls.Control? placementTarget, ConnectionDetails details, AllConnectionsViewModel vm)
        {
            try
            {
                Popup popup = null!; // will assign below

                var panel = new StackPanel { Orientation = Orientation.Vertical };

                void AddSeparator()
                {
                    panel.Children.Add(new Border { Height = 1, Background = Brushes.LightGray, Margin = new Thickness(4, 6) });
                }

                Button MakeButton(string text, ICommand command)
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
                            // Close the popup immediately to avoid UI overlap with the modal.
                            if (popup != null)
                                popup.IsOpen = false;

                            if (command != null && command.CanExecute(details))
                                command.Execute(details);
                        }
                        catch (Exception ex)
                        {
                            try { Console.WriteLine($"Popup button command error: {ex.Message}"); } catch { }
                        }
                    };

                    return b;
                }

                panel.Children.Add(MakeButton("Connect", vm.ConnectCommand));
                panel.Children.Add(MakeButton("Connect Without RD Gateway", vm.ConnectWithoutGatewayCommand));
                AddSeparator();
                panel.Children.Add(MakeButton("Edit Connection", vm.EditConnectionCommand));
                AddSeparator();
                panel.Children.Add(MakeButton("View .RDPX File", vm.ShowRdpXFileCommand));
                panel.Children.Add(MakeButton("View .RDP File", vm.ShowRdpFileCommand));
                AddSeparator();
                // Favourite toggle
                panel.Children.Add(MakeButton(details.IsFavourite ? "Remove from Favourites" : "Add to Favourites", vm.ToggleFavouriteCommand));
                AddSeparator();
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
                try { Console.WriteLine($"ShowActionsPopup error: {ex.Message}"); } catch { }
            }
        }

    private void Item_ActionsButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                if (btn?.DataContext is ConnectionDetails details)
                {
                    var vm = this.DataContext as AllConnectionsViewModel;
                    if (vm != null)
                    {
                        try { Console.WriteLine($"Item_ActionsButtonClick invoked for {details.ConnectionName}"); } catch { }
                        // Show the popup/dropdown anchored to the hamburger button
                        var btnControl = btn as Avalonia.Controls.Control;
                        ShowActionsPopup(btnControl, details, vm);
                    }
                }
            }
            catch (Exception ex)
            {
                try { Console.WriteLine($"Item_ActionsButtonClick error: {ex.Message}"); } catch { }
            }
        }

    }
}
