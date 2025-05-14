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

public class MeterReadingsViewModel : INotifyPropertyChanged
{
    public ObservableCollection<MeterReadingViewModel> Readings { get; set; } = new();

    private int _meterId;
    private string _meterNumber = "";
    public string MeterNumber
    {
        get => _meterNumber;
        set { _meterNumber = value; OnPropertyChanged(); }
    }
    public ICommand UnapproveCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ApproveCommand { get; }
    public bool ShowApprovedByColumn => App.CurrentUser?.Role?.Name is "Администратор" or "Сотрудник";
    public bool CanApprove => App.CurrentUser?.Role?.Name is "Администратор" or "Сотрудник"
                              && SelectedReading != null
                              && !SelectedReading.IsApproved;

    public bool CanUnapprove => App.CurrentUser?.Role?.Name == "Администратор"
                                && SelectedReading?.IsApproved == true;

    private MeterReadingViewModel? _selectedReading;
    public MeterReadingViewModel? SelectedReading
    {
        get => _selectedReading;
        set
        {
            _selectedReading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanApprove));
        }
    }


    public MeterReadingsViewModel(int meterId)
    {
        _meterId = meterId;
        AddCommand = new RelayCommand(_ => AddReading());
        DeleteCommand = new RelayCommand(obj => DeleteReading(obj as MeterReadingViewModel));
        ApproveCommand = new RelayCommand(obj => ApproveReading(obj as MeterReadingViewModel));
        UnapproveCommand = new RelayCommand(obj => UnapproveReading(obj as MeterReadingViewModel));
        LoadReadings();
    }


    private void UnapproveReading(MeterReadingViewModel? reading)
    {
        if (reading == null || !reading.IsApproved) return;

        using var db = new HcsDbContext();
        var entity = db.MeterReadings.FirstOrDefault(r => r.Id == reading.Id);
        if (entity == null) return;

        entity.IsApproved = false;
        entity.ApprovedByUserId = null;
        db.SaveChanges();

        LoadReadings();
    }

    private void LoadReadings()
    {
        using var db = new HcsDbContext();

        var meter = db.Meters.FirstOrDefault(m => m.Id == _meterId);
        MeterNumber = meter?.MeterNumber ?? "Неизвестен";

        Readings.Clear();

        var readings = db.MeterReadings
            .Include(r => r.ApprovedByUser)
            .Where(r => r.MeterId == _meterId)
            .OrderByDescending(r => r.ReadingDate)
            .Select(r => new MeterReadingViewModel
            {
                Id = r.Id,
                ReadingDate = r.ReadingDate,
                Value = r.Value,
                IsApproved = r.IsApproved,
                ApprovedByLogin = r.ApprovedByUser != null ? r.ApprovedByUser.Username : null
            })
            .ToList();

        foreach (var r in readings)
            Readings.Add(r);
    }

    private void ApproveReading(MeterReadingViewModel? reading)
    {
        if (reading == null || reading.IsApproved || App.CurrentUser == null) return;

        using var db = new HcsDbContext();
        var entity = db.MeterReadings.FirstOrDefault(r => r.Id == reading.Id);
        if (entity == null) return;

        entity.IsApproved = true;
        entity.ApprovedByUserId = App.CurrentUser.Id;
        db.SaveChanges();

        LoadReadings();
    }

    private void AddReading()
    {
        var dialog = new Views.AddMeterReadingDialog();
        var vm = new AddMeterReadingDialogViewModel(_meterId);
        dialog.DataContext = vm;

        vm.CloseRequested += () =>
        {
            dialog.Close();
            LoadReadings();
        };

        dialog.ShowDialog();
    }

    private void DeleteReading(MeterReadingViewModel? reading)
    {
        if (reading == null) return;

        if (reading.IsApproved)
        {
            MessageBox.Show("Нельзя удалить подтверждённое показание.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"Удалить показание за {reading.ReadingDate:dd.MM.yyyy}?", "Подтверждение", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        using var db = new HcsDbContext();
        var entity = db.MeterReadings.FirstOrDefault(r => r.Id == reading.Id);
        if (entity == null) return;

        db.MeterReadings.Remove(entity);
        db.SaveChanges();
        LoadReadings();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
