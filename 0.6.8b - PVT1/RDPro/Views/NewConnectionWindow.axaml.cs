// RDPro/Views/NewConnectionWindow.axaml.cs

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI; 
using ReactiveUI;         
using RDPro.ViewModels; 
using RDPro.Models; 
using System;
using System.Reactive.Disposables; 

namespace RDPro.Views
{
    // CRITICAL: Must inherit from ReactiveWindow<T> for the dialog return to work correctly
    public partial class NewConnectionWindow : ReactiveWindow<NewConnectionViewModel> 
    {
        public NewConnectionWindow()
        {
            InitializeComponent();
            
            this.WhenActivated(disposables =>
            {
                if (ViewModel != null)
                {
                    // FIX: Close(connection) passes the result to ShowDialog.
                    ViewModel.SaveCommand.Subscribe(connection => Close(connection)).DisposeWith(disposables); 
                    
                    // FIX: Close(null) indicates cancellation.
                    ViewModel.CancelCommand.Subscribe(_ => Close(null)).DisposeWith(disposables); 
                }
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}