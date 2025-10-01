using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq; // Required for .Select() extension method
using System.Windows.Input;
using ReactiveUI;
using RDPro.Models;
using RDPro.Services;

namespace RDPro.ViewModels
{
    public class GroupsViewModel : ReactiveObject
    {
        public ObservableCollection<ConnectionGroup> Groups { get; } = new();

        private string _newGroupName = string.Empty;
        public string NewGroupName
        {
            get => _newGroupName;
            set => this.RaiseAndSetIfChanged(ref _newGroupName, value);
        }

        private ConnectionGroup? _selectedGroup;
        public ConnectionGroup? SelectedGroup
        {
            get => _selectedGroup;
            set => this.RaiseAndSetIfChanged(ref _selectedGroup, value);
        }

        public ICommand AddGroupCommand { get; }
        public ICommand RemoveGroupCommand { get; }
        public ICommand EditGroupCommand { get; }

        public GroupsViewModel()
        {
            var loadedGroups = GroupService.LoadGroups();
            foreach (var group in loadedGroups)
            {
                Groups.Add(group);
            }

            var canAdd = this.WhenAnyValue(x => x.NewGroupName, name => !string.IsNullOrWhiteSpace(name));
            AddGroupCommand = ReactiveCommand.Create(() =>
            {
                var trimmedName = NewGroupName.Trim();
                if (Groups.Any(g => string.Equals(g.Name, trimmedName, StringComparison.OrdinalIgnoreCase))) return;

                var newGroup = new ConnectionGroup { Name = trimmedName };
                Groups.Add(newGroup);
                Save();
                NewGroupName = string.Empty;
            }, canAdd);

            var canModify = this.WhenAnyValue(x => x.SelectedGroup).Select(g => g != null);
            RemoveGroupCommand = ReactiveCommand.Create(async () =>
            {
                if (SelectedGroup == null) return;
                bool confirmed = await UiService.ShowConfirmAsync("Delete Group", $"Are you sure you want to delete the group '{SelectedGroup.Name}'? This cannot be undone.");
                if (confirmed)
                {
                    Groups.Remove(SelectedGroup);
                    SelectedGroup = null; // Clear selection after removal
                    Save();
                }
            }, canModify);

            EditGroupCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedGroup == null) return;

                var vm = new EditGroupViewModel { Name = SelectedGroup.Name };
                var dialog = new Views.EditGroupWindow { DataContext = vm };

                var newName = await dialog.ShowDialog<string?>(App.GetTopLevel() as Avalonia.Controls.Window);

                if (newName != null && !string.Equals(SelectedGroup.Name, newName, StringComparison.Ordinal))
                {
                    // Check if a group with the new name already exists (case-insensitive)
                    if (Groups.Any(g => g != SelectedGroup && string.Equals(g.Name, newName, StringComparison.OrdinalIgnoreCase)))
                    {
                        await UiService.ShowInfoAsync("Edit Failed", $"A group named '{newName}' already exists.");
                        return;
                    }
                    SelectedGroup.Name = newName;
                    Save();
                }
            }, canModify);
        }

        private void Save()
        {
            GroupService.SaveGroups(Groups.ToList());
        }
    }
}