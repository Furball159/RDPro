// RDPro/ViewModels/MainWindowViewModel.cs (UPDATE)
using System.Reactive;
using ReactiveUI;
using RDPro.Models;

namespace RDPro.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    public AllConnectionsViewModel AllConnectionsVM { get; }

    public MainWindowViewModel()
    {
        AllConnectionsVM = new AllConnectionsViewModel();
    }
}