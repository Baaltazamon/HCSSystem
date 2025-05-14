using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using HCSSystem.ViewModels.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels;

public class AddEditMeterDialogViewModel : INotifyPropertyChanged
{
    public event Action? CloseRequested;

    public bool IsEditMode { get; set; }

    public int Id { get; set; }
    public string MeterNumber { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public DateTime InstallationDate { get; set; } = DateTime.Today;

    public List<Service> Services { get; set; } = new();

    // Поиск и выбор адреса
    public string AddressSearchText
    {
        get => _addressSearchText;
        set
        {
            _addressSearchText = value;
            OnPropertyChanged();
            _ = SearchAddressesAsync();
        }
    }
    private string _addressSearchText = string.Empty;

    public ObservableCollection<ClientAddressDto> SearchResults { get; set; } = new();

    public ClientAddressDto? SelectedAddress
    {
        get => _selectedAddress;
        set
        {
            _selectedAddress = value;
            OnPropertyChanged();
        }
    }
    private ClientAddressDto? _selectedAddress;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddEditMeterDialogViewModel()
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

    private async Task SearchAddressesAsync()
    {
        if (string.IsNullOrWhiteSpace(AddressSearchText)) return;

        var searchTerms = AddressSearchText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (searchTerms.Length == 0) return;

        await using var db = new HcsDbContext();

        var query = db.Addresses.Where(a => !a.IsDeleted);

        foreach (var term in searchTerms)
        {
            var t = term.ToLower();
            query = query.Where(a =>
                EF.Functions.Like(a.City.ToLower(), $"%{t}%") ||
                EF.Functions.Like(a.Street.ToLower(), $"%{t}%") ||
                EF.Functions.Like(a.HouseNumber.ToLower(), $"%{t}%") ||
                EF.Functions.Like((a.Building ?? "").ToLower(), $"%{t}%") ||
                EF.Functions.Like((a.ApartmentNumber ?? "").ToLower(), $"%{t}%"));
        }

        var results = await query.OrderBy(a => a.City).Take(100).ToListAsync();

        SearchResults.Clear();
        foreach (var a in results)
        {
            SearchResults.Add(new ClientAddressDto
            {
                AddressId = a.Id,
                FullAddress = AddressHelpers.ToFullAddress(a)
            });
        }
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(MeterNumber))
        {
            MessageBox.Show("Введите номер счётчика.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedAddress == null)
        {
            MessageBox.Show("Выберите адрес.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = new HcsDbContext();

        if (IsEditMode)
        {
            var entity = db.Meters.FirstOrDefault(m => m.Id == Id);
            if (entity == null) return;

            entity.MeterNumber = MeterNumber;
            entity.InstallationDate = InstallationDate;
            entity.ServiceId = ServiceId;
            entity.AddressId = SelectedAddress.AddressId;
        }
        else
        {
            var meter = new Meter
            {
                MeterNumber = MeterNumber,
                InstallationDate = InstallationDate,
                ServiceId = ServiceId,
                AddressId = SelectedAddress.AddressId,
                IsDeleted = false
            };
            db.Meters.Add(meter);
        }

        db.SaveChanges();
        CloseRequested?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
