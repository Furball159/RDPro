using System;
using System.Collections.ObjectModel;
using RDPro.Models;

namespace RDPro.Services;

public static class FavouritesService
{
    // Simple event to notify subscribers when favourites changed
    public static event Action? FavouritesChanged;

    public static void NotifyChanged() => FavouritesChanged?.Invoke();

    // Helper to filter favourites from a collection
    public static ObservableCollection<ConnectionDetails> GetFavouritesFrom(System.Collections.Generic.IEnumerable<ConnectionDetails> all)
    {
        var list = new ObservableCollection<ConnectionDetails>();
        foreach (var c in all)
        {
            if (c != null && c.IsFavourite) list.Add(c);
        }
        return list;
    }
}
