using System.Reactive;
using ReactiveUI;

namespace RDPro.ViewModels
{
    public class EditGroupViewModel : ReactiveObject
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public ReactiveCommand<Unit, string?> SaveCommand { get; }
        public ReactiveCommand<Unit, string?> CancelCommand { get; }

        public EditGroupViewModel()
        {
            var canSave = this.WhenAnyValue(x => x.Name, n => !string.IsNullOrWhiteSpace(n));
            // Return the new name on save, or null on cancel
            SaveCommand = ReactiveCommand.Create(() => (string?)Name.Trim(), canSave);
            CancelCommand = ReactiveCommand.Create(() => (string?)null);
        }
    }
}