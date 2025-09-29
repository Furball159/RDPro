using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using RDPro.Models;
using RDPro.Services;

namespace RDPro.ViewModels
{
    public class GatewayManagementViewModel : ReactiveObject
    {
        public ObservableCollection<Gateway> Gateways { get; } = new ObservableCollection<Gateway>();
        private string _newHostname = string.Empty;
        public string NewHostname
        {
            get => _newHostname;
            set => this.RaiseAndSetIfChanged(ref _newHostname, value);
        }

        private Gateway? _selectedGateway;
        public Gateway? SelectedGateway
        {
            get => _selectedGateway;
            set => this.RaiseAndSetIfChanged(ref _selectedGateway, value);
        }

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }

        public GatewayManagementViewModel()
        {
            var loaded = GatewayService.LoadGateways();
            // Do not show the special 'No Gateway' entry in management UI; it's a default option.
            foreach (var g in loaded?.Where(x => !string.Equals(x.Hostname, "No Gateway", StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<Gateway>())
                Gateways.Add(g);

            var canAdd = this.WhenAnyValue(x => x.NewHostname, hostname => !string.IsNullOrWhiteSpace(hostname));
            AddCommand = ReactiveCommand.Create(() =>
            {
                var trimmed = (NewHostname ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) return;
                if (string.Equals(trimmed, "No Gateway", StringComparison.OrdinalIgnoreCase)) return;
                if (Gateways.Any(x => string.Equals(x.Hostname, trimmed, StringComparison.OrdinalIgnoreCase))) return;
                var gw = new Gateway { Hostname = trimmed };
                Gateways.Add(gw);
                Save();
                NewHostname = string.Empty;
            }, canAdd);

            var canRemove = this.WhenAnyValue(x => x.SelectedGateway, (Gateway? g) => g != null);
            RemoveCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedGateway == null) return;
                // Ask for confirmation using the existing UiService
                bool ok = await RDPro.Services.UiService.ShowConfirmAsync("Delete Gateway", $"Delete gateway '{SelectedGateway.Hostname}'? This cannot be undone.");
                if (!ok) return;
                Gateways.Remove(SelectedGateway);
                SelectedGateway = null;
                Save();
            }, canRemove);

            EditCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedGateway == null) return;
                // Populate the dialog with the selected gateway
                var vm = new EditGatewayViewModel
                {
                    Hostname = SelectedGateway.Hostname,
                    FriendlyName = SelectedGateway.FriendlyName,
                    UsePcCredentials = SelectedGateway.UsePcCredentials,
                    GatewayUsername = SelectedGateway.GatewayUsername,
                    GatewayPassword = SelectedGateway.GatewayPassword
                };

                var dialog = new RDPro.Views.EditGatewayWindow { DataContext = vm };
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
                {
                    var result = await dialog.ShowDialog<Gateway?>(desktop.MainWindow);
                    if (result != null)
                    {
                        // Replace the selected gateway object in the collection so UI updates immediately
                        int idx = Gateways.IndexOf(SelectedGateway);
                        if (idx >= 0)
                        {
                            Gateways[idx] = result;
                            // Set the selection to the new object
                            SelectedGateway = Gateways[idx];
                            Save();
                        }
                    }
                }
            }, canRemove);
        }

        private void Save()
        {
            GatewayService.SaveGateways(Gateways.ToList());
        }
    }
}
