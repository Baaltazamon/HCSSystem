using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels;

public class AddEditRateDialogViewModel : INotifyPropertyChanged
{
    public event Action? CloseRequested;

    public bool IsEditMode { get; set; }

    private int _serviceId;
    private decimal _pricePerUnit;
    private DateTime _effectiveFrom = DateTime.Today;

    public int Id { get; set; }

    public int ServiceId
    {
        get => _serviceId;
        set { _serviceId = value; OnPropertyChanged(); }
    }

    public decimal PricePerUnit
    {
        get => _pricePerUnit;
        set { _pricePerUnit = value; OnPropertyChanged(); }
    }

    public DateTime EffectiveFrom
    {
        get => _effectiveFrom;
        set { _effectiveFrom = value; OnPropertyChanged(); }
    }

    public List<Service> Services { get; set; } = new();

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddEditRateDialogViewModel()
    {
        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());

        LoadServices();
    }

    private void LoadServices()
    {
        using var db = new HcsDbContext();
        Services = db.Services
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToList();

        OnPropertyChanged(nameof(Services));
    }

    private void Save()
    {
        if (PricePerUnit <= 0)
        {
            MessageBox.Show("Цена должна быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = new HcsDbContext();

        if (IsEditMode)
        {
            var entity = db.Rates.FirstOrDefault(r => r.Id == Id);
            if (entity == null) return;

            entity.PricePerUnit = PricePerUnit;
            entity.EffectiveFrom = EffectiveFrom;
        }
        else
        {
            var rate = new Rate
            {
                ServiceId = ServiceId,
                PricePerUnit = PricePerUnit,
                EffectiveFrom = EffectiveFrom,
                CreatedByUserId = App.CurrentUser!.Id,
                IsDeleted = false
            };
            db.Rates.Add(rate);
        }

        db.SaveChanges();
        CloseRequested?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
