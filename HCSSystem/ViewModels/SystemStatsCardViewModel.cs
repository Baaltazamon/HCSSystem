using HCSSystem.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HCSSystem.ViewModels;

public class SystemStatsCardViewModel : INotifyPropertyChanged
{
    public int AddressCount { get; set; }
    public int ClientCount { get; set; }
    public int MeterCount { get; set; }
    public int ReadingCount { get; set; }
    public int PaymentCount { get; set; }

    public SystemStatsCardViewModel()
    {
        LoadStats();
    }

    private void LoadStats()
    {
        using var db = new HcsDbContext();
        var user = App.CurrentUser;

        if (user.Role.Name == "Клиент")
        {
            var clientId = user.Id;
            var addressIds = db.ClientAddresses.Where(ca => ca.ClientId == clientId).Select(ca => ca.AddressId).ToList();

            AddressCount = addressIds.Count;
            ClientCount = 1;

            var meterIds = db.Meters.Where(m => addressIds.Contains(m.AddressId)).Select(m => m.Id).ToList();
            MeterCount = meterIds.Count;

            ReadingCount = db.MeterReadings.Count(r => meterIds.Contains(r.MeterId));
            PaymentCount = db.Payments.Count(p => meterIds.Contains(p.MeterReading.MeterId));
        }
        else
        {
            AddressCount = db.Addresses.Count(a => !a.IsDeleted);
            ClientCount = db.Clients.Count();
            MeterCount = db.Meters.Count(m => !m.IsDeleted);
            ReadingCount = db.MeterReadings.Count();
            PaymentCount = db.Payments.Count();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}