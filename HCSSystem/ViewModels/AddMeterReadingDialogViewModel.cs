using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels;

public class AddMeterReadingDialogViewModel : INotifyPropertyChanged
{
    public event Action? CloseRequested;

    private readonly int _meterId;

    private DateTime _readingDate = DateTime.Today;
    private decimal _value;

    public DateTime ReadingDate
    {
        get => _readingDate;
        set { _readingDate = value; OnPropertyChanged(); }
    }

    public decimal Value
    {
        get => _value;
        set { _value = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddMeterReadingDialogViewModel(int meterId)
    {
        _meterId = meterId;

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());
    }

    private void Save()
    {
        if (Value <= 0)
        {
            MessageBox.Show("Значение должно быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = new HcsDbContext();

        // проверка на дубликат по дате
        bool alreadyExists = db.MeterReadings.Any(r =>
            r.MeterId == _meterId &&
            r.ReadingDate.Date == ReadingDate.Date);

        if (alreadyExists)
        {
            MessageBox.Show("Показание на эту дату уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var reading = new MeterReading
        {
            MeterId = _meterId,
            ReadingDate = ReadingDate,
            Value = Value,
            IsApproved = false,
            ApprovedByUserId = null
        };

        db.MeterReadings.Add(reading);
        db.SaveChanges();

        CloseRequested?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
