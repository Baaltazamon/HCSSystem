using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using HCSSystem.ViewModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels;

public class RatesViewModel : INotifyPropertyChanged
{
    private int _selectedServiceId;

    public ObservableCollection<RateViewModel> FilteredRates { get; set; } = new();
    private List<RateViewModel> _allRates = new();
    public List<Service> Services { get; set; } = new();

    public int SelectedServiceId
    {
        get => _selectedServiceId;
        set
        {
            _selectedServiceId = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public RatesViewModel()
    {
        AddCommand = new RelayCommand(_ => AddRate());
        EditCommand = new RelayCommand(rate => EditRate(rate as RateViewModel));
        DeleteCommand = new RelayCommand(rate => DeleteRate(rate as RateViewModel));

        LoadData();
    }

    private void LoadData()
    {
        using var db = new HcsDbContext();

        Services = db.Services
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToList();

        _allRates = db.Rates
            .Include(r => r.Service)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.EffectiveFrom)
            .Select(r => new RateViewModel
            {
                Id = r.Id,
                ServiceId = r.ServiceId,
                ServiceName = r.Service.Name,
                PricePerUnit = r.PricePerUnit,
                EffectiveFrom = r.EffectiveFrom
            })
            .ToList();

        Services.Insert(0, new Service { Id = 0, Name = "Все услуги" });
        SelectedServiceId = 0;

        if (Services.Any())
            SelectedServiceId = Services.First().Id;
        else
            ApplyFilter();
        
    }

    private void ApplyFilter()
    {
        var filtered = _allRates.AsEnumerable();

        if (SelectedServiceId != 0)
            filtered = filtered.Where(r => r.ServiceId == SelectedServiceId);

        FilteredRates.Clear();
        foreach (var r in filtered)
            FilteredRates.Add(r);
    }

    private void AddRate()
    {
        var vm = new AddEditRateDialogViewModel { IsEditMode = false };
        var dialog = new Views.AddEditRateDialog { DataContext = vm };
        vm.CloseRequested += () =>
        {
            dialog.Close();
            LoadData();
        };
        dialog.ShowDialog();
    }

    private void EditRate(RateViewModel? rate)
    {
        if (rate == null) return;

        using var db = new HcsDbContext();
        var entity = db.Rates.FirstOrDefault(r => r.Id == rate.Id);
        if (entity == null) return;

        var vm = new AddEditRateDialogViewModel
        {
            IsEditMode = true,
            Id = entity.Id,
            ServiceId = entity.ServiceId,
            PricePerUnit = entity.PricePerUnit,
            EffectiveFrom = entity.EffectiveFrom
        };
        var dialog = new Views.AddEditRateDialog { DataContext = vm };

        vm.CloseRequested += () =>
        {
            dialog.Close();
            LoadData();
        };

        dialog.ShowDialog();

        
    }

    private void DeleteRate(RateViewModel? rate)
    {
        if (rate == null) return;

        var result = MessageBox.Show($"Удалить тариф для '{rate.ServiceName}' от {rate.EffectiveFrom:dd.MM.yyyy}?",
                                     "Подтверждение", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        using var db = new HcsDbContext();
        var entity = db.Rates.FirstOrDefault(r => r.Id == rate.Id);
        if (entity == null) return;

        entity.IsDeleted = true;
        db.SaveChanges();
        LoadData();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
