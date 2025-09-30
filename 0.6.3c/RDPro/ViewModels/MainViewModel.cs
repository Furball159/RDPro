// RDPro/ViewModels/MainViewModel.cs

using ReactiveUI;

namespace RDPro.ViewModels;

public class MainViewModel : ReactiveObject
{
    // Property to hold the ViewModel for the All Connections tab.
    public AllConnectionsViewModel AllConnectionsVM { get; } 
        public FavouritesViewModel FavouritesVM { get; }
    
    public MainViewModel()
    {
        AllConnectionsVM = new AllConnectionsViewModel();
        FavouritesVM = new FavouritesViewModel(AllConnectionsVM);
    }
}