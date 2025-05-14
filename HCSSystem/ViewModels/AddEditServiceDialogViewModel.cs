using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels;

public class AddEditServiceDialogViewModel : INotifyPropertyChanged
{
    public event Action? CloseRequested;

    public bool IsEditMode { get; set; }

    private string _name = string.Empty;
    private int _selectedUnitId;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public int SelectedUnitId
    {
        get => _selectedUnitId;
        set { _selectedUnitId = value; OnPropertyChanged(); }
    }

    public List<UnitOfMeasurement> Units { get; set; } = new();

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddEditServiceDialogViewModel()
    {
        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());

        LoadUnits();
    }

    private void LoadUnits()
    {
        using var db = new HcsDbContext();
        Units = db.UnitOfMeasurements.OrderBy(u => u.Name).ToList();
        OnPropertyChanged(nameof(Units));
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("Название услуги обязательно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = new HcsDbContext();
        if (IsEditMode)
        {
            var entity = db.Services.FirstOrDefault(s => s.Id == Id);
            if (entity == null) return;

            entity.Name = Name;
            entity.UnitOfMeasurementId = SelectedUnitId;
        }
        else
        {
            var service = new Service
            {
                Name = Name,
                UnitOfMeasurementId = SelectedUnitId,
                IsDeleted = false
            };
            db.Services.Add(service);
        }

        db.SaveChanges();
        CloseRequested?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
