using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using RDPro.Models;
using RDPro.Services;

namespace RDPro.ViewModels
{
    public class FavouritesViewModel : ReactiveObject
    {
        private readonly ObservableCollection<ConnectionDetails> _allFavourites = new();
        public ObservableCollection<ConnectionDetails> Favourites { get; } = new();

        public ICommand? EditConnectionCommand { get; }
        public ICommand? ShowRdpFileCommand { get; }
        public ICommand? ShowRdpXFileCommand { get; }
        public ICommand? ConnectCommand { get; }
        public ICommand? ConnectWithoutGatewayCommand { get; }
        public ICommand? DeleteConnectionCommand { get; }
        public ICommand? ToggleFavouriteCommand { get; }
        public ICommand? RemoveSpecificConnectionCommand { get; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        private AllConnectionsViewModel? _source;

        public FavouritesViewModel(AllConnectionsViewModel source)
        {
            if (source == null) return;

            _source = source;

            // Reuse commands from the AllConnectionsViewModel so behaviour is consistent
            EditConnectionCommand = source.EditConnectionCommand;
            ShowRdpFileCommand = source.ShowRdpFileCommand;
            ShowRdpXFileCommand = source.ShowRdpXFileCommand;
            ConnectCommand = source.ConnectCommand;
            ConnectWithoutGatewayCommand = source.ConnectWithoutGatewayCommand;
            DeleteConnectionCommand = source.DeleteConnectionCommand;
            ToggleFavouriteCommand = source.ToggleFavouriteCommand;
            RemoveSpecificConnectionCommand = source.RemoveSpecificConnectionCommand;

            // Initial population
            RefreshFromSource(source);

            // Subscribe to favourites change notifications
            FavouritesService.FavouritesChanged += () => RefreshFromSource(_source!);

            // Also listen for source collection changes (add/remove) to refresh favourites
            try
            {
                source.Connections.CollectionChanged += (_, __) => RefreshFromSource(source);
            }
            catch { }

            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(200), RxApp.MainThreadScheduler)
                .Subscribe(_ => ApplyFilter());
        }

        private void RefreshFromSource(AllConnectionsViewModel src)
        {
            try
            {
                _allFavourites.Clear();
                foreach (var c in src.Connections.Where(x => x.IsFavourite))
                    _allFavourites.Add(c);
                
                ApplyFilter();
            }
            catch (Exception ex)
            {
                try { Console.WriteLine($"Favourites refresh error: {ex.Message}"); } catch { }
            }
        }

        private void ApplyFilter()
        {
            Favourites.Clear();
            var filter = SearchText.Trim();

            if (string.IsNullOrWhiteSpace(filter))
            {
                foreach (var conn in _allFavourites.OrderBy(c => c.ConnectionName))
                {
                    Favourites.Add(conn);
                }
            }
            else
            {
                var filtered = _allFavourites.Where(c =>
                    (c.ConnectionName?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.ServerAddress?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false)
                ).OrderBy(c => c.ConnectionName);

                foreach (var conn in filtered)
                {
                    Favourites.Add(conn);
                }
            }
        }
    }
}