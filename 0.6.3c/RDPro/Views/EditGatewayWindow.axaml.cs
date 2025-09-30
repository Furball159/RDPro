using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using RDPro.ViewModels;
using System.Reactive.Disposables;

namespace RDPro.Views
{
    public partial class EditGatewayWindow : ReactiveWindow<EditGatewayViewModel>
    {
        public EditGatewayWindow()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                if (ViewModel != null)
                {
                    ViewModel.SaveCommand.Subscribe(g => Close(g)).DisposeWith(disposables);
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
