using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using RDPro.Models;
using RDPro.Services;
using RDPro.Views;
using System.Diagnostics;

namespace RDPro.ViewModels
{
    public class AllConnectionsViewModel : ReactiveObject
    {
        private readonly ObservableCollection<ConnectionDetails> _allConnections = new();
        public ObservableCollection<ConnectionDetails> Connections { get; } = new();
        public ICommand OpenNewConnectionDialogCommand { get; }
        public ICommand EditConnectionCommand { get; }
        public ICommand ShowRdpFileCommand { get; }
        public ICommand ShowRdpXFileCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand ConnectWithoutGatewayCommand { get; }
        public ICommand DeleteConnectionCommand { get; }
        public ICommand RemoveSelectedConnectionCommand { get; }
        public ICommand RemoveSpecificConnectionCommand { get; }
        public ICommand ToggleFavouriteCommand { get; }

        private bool _hasConnections;
        public bool HasConnections
        {
            get => _hasConnections;
            private set
            {
                if (this.RaiseAndSetIfChanged(ref _hasConnections, value))
                {
                    this.RaisePropertyChanged(nameof(NoConnections));
                }
            }
        }

        public bool NoConnections => !HasConnections;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        private ConnectionDetails? _selectedConnection;
        public ConnectionDetails? SelectedConnection
        {
            get => _selectedConnection;
            set => this.RaiseAndSetIfChanged(ref _selectedConnection, value);
        }


        public AllConnectionsViewModel()
        {
            OpenNewConnectionDialogCommand = ReactiveCommand.CreateFromTask(ShowNewConnectionDialog);
            EditConnectionCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(EditConnectionAsync);
            ShowRdpFileCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(ShowRdpFileAsync);
            ShowRdpXFileCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(ShowRdpXFileAsync);
            ConnectCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(ShowRdpFileAsync);
            ConnectWithoutGatewayCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(ShowRdpFileWithoutGatewayAsync);
            DeleteConnectionCommand = ReactiveCommand.Create<ConnectionDetails>(DeleteConnection);
            ToggleFavouriteCommand = ReactiveCommand.Create<ConnectionDetails>(ToggleFavourite);

            // Command for the top-level "Remove" button, enabled only when a connection is selected.
            var canRemove = this.WhenAnyValue(x => x.SelectedConnection).Select(selected => selected != null);
            RemoveSelectedConnectionCommand = ReactiveCommand.CreateFromTask(RemoveSelectedConnectionAsync, canRemove);
            RemoveSpecificConnectionCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(RemoveSpecificConnectionAsync);


            LoadExistingConnections();

            // When SearchText changes, wait for a moment and then apply the filter.
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(200), RxApp.MainThreadScheduler)
                .Subscribe(_ => ApplyFilter());

            // When the underlying collection changes, re-apply the filter and update counts.
            _allConnections.CollectionChanged += (sender, e) =>
            {
                ApplyFilter();
                HasConnections = _allConnections.Count > 0;
            };

            // Initial state
            HasConnections = _allConnections.Count > 0;

            // When the filtered list changes, we need to notify that NoConnections might have changed.
            this.WhenAnyValue(x => x.Connections.Count)
                .Subscribe(_ => {
                this.RaisePropertyChanged(nameof(NoConnections));
            });
        }

        private void ApplyFilter()
        {
            Connections.Clear();
            var filter = SearchText.Trim();

            if (string.IsNullOrWhiteSpace(filter))
            {
                foreach (var conn in _allConnections.OrderBy(c => c.ConnectionName))
                {
                    Connections.Add(conn);
                }
            }
            else
            {
                var filtered = _allConnections.Where(c =>
                    (c.ConnectionName?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.ServerAddress?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false)
                ).OrderBy(c => c.ConnectionName);

                foreach (var conn in filtered)
                {
                    Connections.Add(conn);
                }
            };
        }

        private async Task RemoveSelectedConnectionAsync()
        {
            await ConfirmAndRemoveConnectionAsync(SelectedConnection);
        }

        private async Task RemoveSpecificConnectionAsync(ConnectionDetails details)
        {
            await ConfirmAndRemoveConnectionAsync(details);
        }

        private async Task ConfirmAndRemoveConnectionAsync(ConnectionDetails? details)
        {
            if (details == null) return;

            var title = "Remove Connection";
            var message = $"Are you sure you want to remove the connection '{details.ConnectionName}'? This cannot be undone.";

            bool confirmed = await UiService.ShowConfirmAsync(title, message);
            if (confirmed && DeleteConnectionCommand.CanExecute(details))
                DeleteConnectionCommand.Execute(details);
        }

        private async Task ShowNewConnectionDialog()
        {
            var dialog = new NewConnectionWindow
            {
                DataContext = new NewConnectionViewModel()
            };

            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var result = await dialog.ShowDialog<ConnectionDetails?>(desktop.MainWindow);

                if (result != null)
                {
                    ConnectionFileService.SaveConnection(result);
                    _allConnections.Add(result);
                }
            }
        }

        private async Task EditConnectionAsync(ConnectionDetails existing)
        {
            if (existing == null) return;

            var vm = new NewConnectionViewModel
            {
                ConnectionName = existing.ConnectionName,
                ServerAddress = existing.ServerAddress,
                Username = existing.Username,
                // SelectedGateway will be set after the VM populates its AvailableGateways
            };

            var dialog = new NewConnectionWindow
            {
                DataContext = vm
            };
            dialog.Title = "Edit Connection";

            // Try to set SelectedGateway to the matching GatewayOption before showing the dialog
            if (!string.IsNullOrWhiteSpace(existing.Gateway))
            {
                var match = vm.AvailableGateways.FirstOrDefault(x => string.Equals(x.Hostname, existing.Gateway, StringComparison.OrdinalIgnoreCase));
                if (match != null) vm.SelectedGateway = match;
            }

            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var result = await dialog.ShowDialog<ConnectionDetails?>(desktop.MainWindow);

                if (result != null)
                {
                    // Keep the same ConnectionID to overwrite files
                    result.ConnectionID = existing.ConnectionID;

                    ConnectionFileService.SaveConnection(result);

                    // Update in-memory list (replace existing)
                    int idx = _allConnections.IndexOf(existing);
                    if (idx >= 0)
                    {
                        _allConnections[idx] = result;
                    }
                }
            }
        }

        private async Task ShowRdpFileAsync(ConnectionDetails details)
        {
            if (details == null) return;
            // Ensure the legacy .rdp is regenerated from the current connection (so gateway info is up-to-date)
            try
            {
                ConnectionFileService.SaveConnection(details);

                string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro", "Connections", "Legacy");
                string rdpFile = Path.Combine(baseDir, $"{details.ConnectionID}.rdp");

                if (File.Exists(rdpFile))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = rdpFile,
                        UseShellExecute = true
                    });
                }
                else
                {
                    await UiService.ShowMessageAsync("File not found", $"RDP file not found:\n{rdpFile}");
                }
            }
            catch (Exception ex)
            {
                await UiService.ShowMessageAsync("Error launching RDP", ex.Message);
            }
        }

        private async Task ShowRdpFileWithoutGatewayAsync(ConnectionDetails details)
        {
            if (details == null) return;

            try
            {
                // Use the ConnectionFileService to create a temporary .rdp without gateway entries and launch it.
                string tempRdp = ConnectionFileService.CreateTemporaryRdpWithoutGateway(details);

                if (File.Exists(tempRdp))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempRdp,
                        UseShellExecute = true
                    });
                }
                else
                {
                    await UiService.ShowMessageAsync("Temporary file missing", $"Temporary RDP file was not created:\n{tempRdp}");
                }
            }
            catch (Exception ex)
            {
                await UiService.ShowMessageAsync("Error launching temporary RDP", ex.Message);
            }
        }

        private async Task ShowRdpXFileAsync(ConnectionDetails details)
        {
            if (details == null) return;

            string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro", "Connections", "Enhanced");
            string rdpxFile = Path.Combine(baseDir, $"{details.ConnectionID}.rdpx");

            try
            {
                if (File.Exists(rdpxFile))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = rdpxFile,
                        UseShellExecute = true
                    });
                }
                else
                {
                    await UiService.ShowMessageAsync("File not found", $"RDPX file not found:\n{rdpxFile}");
                }
            }
            catch (Exception ex)
            {
                await UiService.ShowMessageAsync("Error opening file", ex.Message);
            }
        }

        private void DeleteConnection(ConnectionDetails details)
        {
            if (details == null) return;

            try
            {
                string baseLegacy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro", "Connections", "Legacy");
                string baseEnhanced = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro", "Connections", "Enhanced");

                string rdpFile = Path.Combine(baseLegacy, $"{details.ConnectionID}.rdp");
                string rdpxFile = Path.Combine(baseEnhanced, $"{details.ConnectionID}.rdpx");

                if (File.Exists(rdpFile)) File.Delete(rdpFile);
                if (File.Exists(rdpxFile)) File.Delete(rdpxFile);

                // Remove from collection
                if (_allConnections.Contains(details))
                {
                    _allConnections.Remove(details);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting connection files: {ex.Message}");
            }
        }

        private void LoadExistingConnections()
        {
            try
            {
                string connectionsPath = ConnectionFileService.GetEnhancedPath();
                
                Directory.CreateDirectory(connectionsPath);

                var files = Directory.EnumerateFiles(connectionsPath, "*.rdpx").ToList();
                
                if (files.Any())
                {
                    foreach (var filePath in files)
                    {
                        try
                        {
                            var connection = ConnectionFileService.Load(filePath); 
                            if (connection != null)
                            {
                                _allConnections.Add(connection);
                            }
                        }
                        catch (Exception fileEx)
                        {
                            Console.WriteLine($"Error loading file {filePath}: {fileEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during connection directory access: {ex.Message}");
            }
        }

        private void ToggleFavourite(ConnectionDetails details)
        {
            if (details == null) return;

            try
            {
                details.IsFavourite = !details.IsFavourite;
                // Persist change
                ConnectionFileService.SaveConnection(details);
                // Notify any subscribers that favourites changed
                FavouritesService.NotifyChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling favourite: {ex.Message}");
            }
        }
    }
}
