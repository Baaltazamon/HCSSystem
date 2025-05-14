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

public class MetersViewModel : INotifyPropertyChanged
{
    private string _searchQuery = string.Empty;
    private MeterViewModel? _selected;

    public ObservableCollection<MeterViewModel> FilteredMeters { get; set; } = new();
    private List<MeterViewModel> _allMeters = new();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    private bool _onlyWithUnapprovedReadings;
    public bool OnlyWithUnapprovedReadings
    {
        get => _onlyWithUnapprovedReadings;
        set
        {
            _onlyWithUnapprovedReadings = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public MeterViewModel? Selected
    {
        get => _selected;
        set { _selected = value; OnPropertyChanged(); }
    }

    public bool CanEdit => App.CurrentUser?.Role?.Name is "Администратор" or "Сотрудник";
    public bool ShowUserColumn => CanEdit;

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand OpenReadingsCommand { get; }
    public ICommand GeneratePaymentCommand { get; }
    public ICommand GenerateAllPaymentsCommand { get; }
    public ICommand ExportMeterReadingsCommand { get; }
    public MetersViewModel()
    {
        AddCommand = new RelayCommand(_ => AddMeter(), _ => CanEdit);
        EditCommand = new RelayCommand(m => EditMeter(m as MeterViewModel), _ => CanEdit);
        DeleteCommand = new RelayCommand(m => DeleteMeter(m as MeterViewModel), _ => CanEdit);
        OpenReadingsCommand = new RelayCommand(m => OpenReadings(m as MeterViewModel));
        GeneratePaymentCommand = new RelayCommand(m => GeneratePayment(m as MeterViewModel), _ => CanEdit);
        GenerateAllPaymentsCommand = new RelayCommand(_ => GenerateAllPayments(), _ => CanEdit);
        ExportMeterReadingsCommand = new RelayCommand(_ =>
        {
            if (App.CurrentUser?.Role?.Name is "Администратор" or "Сотрудник")
                ReportService.GenerateMeterReadingsReport();
            else
                ReportService.GenerateMeterReadingsReportForClient(App.CurrentUser!.Id);
        });
        LoadMeters();
    }

    private void LoadMeters()
    {
        using var db = new HcsDbContext();

        var isClient = App.CurrentUser?.Role?.Name == "Клиент";

        var query = db.Meters
            .Include(m => m.Service)
            .Include(m => m.Address)
            .Where(m => !m.IsDeleted);

        if (isClient && App.CurrentUser != null)
        {
            var today = DateTime.Today;

            var addressIds = db.ClientAddresses
                .Where(ca =>
                    ca.ClientId == App.CurrentUser.Id &&
                    ca.OwnershipStartDate <= today &&
                    (ca.OwnershipEndDate == null || ca.OwnershipEndDate >= today))
                .Select(ca => ca.AddressId)
                .ToList();

            query = query.Where(m => addressIds.Contains(m.AddressId));
        }

        var unapprovedCounts = db.MeterReadings
            .Where(r => !r.IsApproved)
            .GroupBy(r => r.MeterId)
            .ToDictionary(g => g.Key, g => g.Count());

        _allMeters = query
            .Select(m => new MeterViewModel
            {
                Id = m.Id,
                MeterNumber = m.MeterNumber,
                InstallationDate = m.InstallationDate,
                ServiceName = m.Service.Name,
                AddressString = AddressHelpers.ToFullAddress(m.Address),
                UnapprovedReadingsCount = unapprovedCounts.ContainsKey(m.Id) ? unapprovedCounts[m.Id] : 0
            })
            .ToList();

        ApplyFilter();
    }

    private void GenerateAllPayments()
    {
        using var db = new HcsDbContext();

        var approvedReadings = db.MeterReadings
            .Include(r => r.Meter)
            .ThenInclude(m => m.Service)
            .ThenInclude(s => s.UnitOfMeasurement)
            .Where(r => r.IsApproved)
            .OrderBy(r => r.ReadingDate)
            .ToList();

        int success = 0, skipped = 0;

        foreach (var reading in approvedReadings)
        {
            var result = PaymentGenerator.TryGeneratePaymentForReading(reading, db);
            if (result != null)
                success++;
            else
                skipped++;
        }

        MessageBox.Show($"Создано: {success}, пропущено: {skipped}", "Результат генерации", MessageBoxButton.OK, MessageBoxImage.Information);
        LoadMeters();
    }


    private void GeneratePayment(MeterViewModel? meter)
    {
        if (meter == null) return;

        using var db = new HcsDbContext();

        var reading = db.MeterReadings
            .Where(r => r.MeterId == meter.Id && r.IsApproved)
            .OrderByDescending(r => r.ReadingDate)
            .FirstOrDefault();

        if (reading == null)
        {
            MessageBox.Show("Нет подтверждённых показаний.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var payment = PaymentGenerator.TryGeneratePaymentForReading(reading, db);

        if (payment == null)
        {
            MessageBox.Show("Платёж уже существует или не удалось создать.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show($"Платёж создан на сумму {payment.AmountToPay:0.00} ₽ (статус: {payment.PaymentStatus.Name}).", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadMeters(); // Обновим счётчик (на случай изменения статуса/значка)
        }
    }


    private void OpenReadings(MeterViewModel? meter)
    {
        if (meter == null) return;

        var vm = new MeterReadingsViewModel(meter.Id);
        var dialog = new Views.MeterReadingsDialog { DataContext = vm };
        dialog.ShowDialog();
        LoadMeters();
    }

    private void ApplyFilter()
    {
        using var db = new HcsDbContext();

        var filtered = _allMeters.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.ToLower();
            filtered = filtered.Where(m => m.MeterNumber.ToLower().Contains(q));
        }

        if (OnlyWithUnapprovedReadings)
        {
            var meterIdsWithUnapproved = db.MeterReadings
                .Where(r => !r.IsApproved)
                .Select(r => r.MeterId)
                .Distinct()
                .ToHashSet();

            filtered = filtered.Where(m => meterIdsWithUnapproved.Contains(m.Id));
        }

        FilteredMeters.Clear();
        foreach (var m in filtered)
            FilteredMeters.Add(m);
    }




    private void AddMeter()
    {
        var vm = new AddEditMeterDialogViewModel { IsEditMode = false };
        var dialog = new Views.AddEditMeterDialog { DataContext = vm };
        vm.CloseRequested += () =>
        {
            dialog.Close();
            LoadMeters();
        };
        dialog.ShowDialog();
    }

    private void EditMeter(MeterViewModel? meter)
    {
        if (meter == null) return;

        using var db = new HcsDbContext();
        var entity = db.Meters.Include(m => m.Address).FirstOrDefault(m => m.Id == meter.Id);
        if (entity == null) return;

        var vm = new AddEditMeterDialogViewModel
        {
            IsEditMode = true,
            Id = entity.Id,
            MeterNumber = entity.MeterNumber,
            InstallationDate = entity.InstallationDate,
            ServiceId = entity.ServiceId,
            AddressSearchText = AddressHelpers.ToFullAddress(entity.Address),
            SelectedAddress = new ClientAddressDto
            {
                AddressId = entity.AddressId,
                FullAddress = AddressHelpers.ToFullAddress(entity.Address)
            }
        };

        var dialog = new Views.AddEditMeterDialog { DataContext = vm };
        vm.CloseRequested += () =>
        {
            dialog.Close();
            LoadMeters();
        };
        dialog.ShowDialog();
    }


    private void DeleteMeter(MeterViewModel? meter)
    {
        if (meter == null) return;

        var result = MessageBox.Show($"Удалить счётчик {meter.MeterNumber}?", "Подтверждение", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        using var db = new HcsDbContext();
        var entity = db.Meters.FirstOrDefault(m => m.Id == meter.Id);
        if (entity == null) return;

        entity.IsDeleted = true;
        db.SaveChanges();
        LoadMeters();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
