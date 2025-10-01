using Avalonia.ReactiveUI;
using ReactiveUI;
using Avalonia.Markup.Xaml;
using RDPro.ViewModels;
using System;
using System.Reactive.Disposables;

namespace RDPro.Views
{
    public partial class EditGroupWindow : ReactiveWindow<EditGroupViewModel>
    {
        public EditGroupWindow()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                if (ViewModel == null) return;
                // Close the dialog, passing back the result from the command
                ViewModel.SaveCommand.Subscribe(result => Close(result)).DisposeWith(disposables);
                ViewModel.CancelCommand.Subscribe(result => Close(result)).DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}