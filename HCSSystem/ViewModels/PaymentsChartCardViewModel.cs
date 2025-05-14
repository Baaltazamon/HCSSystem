using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace HCSSystem.ViewModels;

public class PaymentsChartCardViewModel : INotifyPropertyChanged
{
    public ObservableCollection<int> AvailableYears { get; set; } = new();
    public ObservableCollection<string> AvailableQuarters { get; set; } = new() { "1 квартал", "2 квартал", "3 квартал", "4 квартал" };

    public int SelectedYear { get; set; }
    public string SelectedQuarter { get; set; } = "1 квартал";

    public SeriesCollection SeriesCollection { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public Func<double, string> Formatter { get; set; } = value => value.ToString("C0", CultureInfo.CurrentCulture);

    public ICommand LoadChartCommand { get; }

    public PaymentsChartCardViewModel()
    {
        LoadChartCommand = new RelayCommand(_ => LoadChart());
        InitYears();
        SelectedYear = AvailableYears.Max();
        LoadChart();
    }

    private void InitYears()
    {
        using var db = new HcsDbContext();
        var years = db.Payments
            .Select(p => p.PeriodStart.Year)
            .Distinct()
            .OrderBy(y => y)
            .ToList();

        foreach (var y in years)
            AvailableYears.Add(y);
    }

    private void LoadChart()
    {
        SeriesCollection.Clear();
        Labels.Clear();

        int quarter = AvailableQuarters.IndexOf(SelectedQuarter) + 1;
        int startMonth = (quarter - 1) * 3 + 1;

        var user = App.CurrentUser;

        using var db = new HcsDbContext();
        var query = db.Payments
            .Where(p => p.PeriodStart.Year == SelectedYear &&
                        p.PeriodStart.Month >= startMonth &&
                        p.PeriodStart.Month <= startMonth + 2);

        if (user.Role.Name == "Клиент")
        {
            var addressIds = db.ClientAddresses
                .Where(c => c.ClientId == user.Id)
                .Select(c => c.AddressId).ToList();

            var meterIds = db.Meters
                .Where(m => addressIds.Contains(m.AddressId))
                .Select(m => m.Id).ToList();

            query = query.Where(p => meterIds.Contains(p.MeterReading.MeterId)).Include(c=> c.PaymentStatus);
        }

        var grouped = query.Include(c => c.PaymentStatus)
            .AsEnumerable()
            .GroupBy(p => new { p.PeriodStart.Month, Status = p.PaymentStatus.Name })
            .GroupBy(g => g.Key.Status)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(x => x.Key.Month, x => x.Sum(p => p.AmountToPay))
            );

        var allMonths = Enumerable.Range(startMonth, 3).ToList();
        Labels = allMonths.Select(m => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m)).ToList();

        foreach (var status in grouped.Keys)
        {
            var values = new ChartValues<decimal>();
            foreach (var m in allMonths)
                values.Add(grouped[status].ContainsKey(m) ? grouped[status][m] : 0);

            SeriesCollection.Add(new ColumnSeries
            {
                Title = status,
                Values = values
            });
        }

        OnPropertyChanged(nameof(SeriesCollection));
        OnPropertyChanged(nameof(Labels));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
