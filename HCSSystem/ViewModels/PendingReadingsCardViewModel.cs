using HCSSystem.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HCSSystem.ViewModels;

public class PendingReadingsCardViewModel : INotifyPropertyChanged
{
    private int _count;
    public int Count
    {
        get => _count;
        set { _count = value; OnPropertyChanged(); }
    }

    public PendingReadingsCardViewModel()
    {
        LoadCount();
    }

    private void LoadCount()
    {
        using var db = new HcsDbContext();
        var user = App.CurrentUser;

        if (user.Role.Name == "Клиент")
        {
            var addressIds = db.ClientAddresses
                .Where(ca => ca.ClientId == user.Id)
                .Select(ca => ca.AddressId)
                .ToList();

            var meterIds = db.Meters
                .Where(m => addressIds.Contains(m.AddressId))
                .Select(m => m.Id)
                .ToList();

            Count = db.MeterReadings
                .Count(r => meterIds.Contains(r.MeterId) && !r.IsApproved);
        }
        else
        {
            Count = db.MeterReadings.Count(r => !r.IsApproved);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}