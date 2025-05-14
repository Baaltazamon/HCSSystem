using HCSSystem.Data;
using HCSSystem.Helpers;
using HCSSystem.ViewModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels;

public class ServicesViewModel : INotifyPropertyChanged
{
    private string _searchQuery;
    private ServiceViewModel _selectedService;
    public ObservableCollection<ServiceViewModel> FilteredServices { get; set; } = new();
    private List<ServiceViewModel> _allServices = new();

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

    public ServiceViewModel SelectedService
    {
        get => _selectedService;
        set
        {
            _selectedService = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public ServicesViewModel()
    {
        AddCommand = new RelayCommand(_ => AddService());
        EditCommand = new RelayCommand(service => EditService(service as ServiceViewModel));
        DeleteCommand = new RelayCommand(service => DeleteService(service as ServiceViewModel));
        LoadServices();
    }

    private void LoadServices()
    {
        using var db = new HcsDbContext();
        _allServices = db.Services
            .Include(s => s.UnitOfMeasurement)
            .Where(s => !s.IsDeleted)
            .Select(s => new ServiceViewModel
            {
                Id = s.Id,
                Name = s.Name,
                UnitName = s.UnitOfMeasurement.Name
            })
            .ToList();

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = _allServices.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.ToLower();
            filtered = filtered.Where(s => s.Name.ToLower().Contains(query) || s.UnitName.ToLower().Contains(query));
        }

        FilteredServices.Clear();
        foreach (var s in filtered)
            FilteredServices.Add(s);
    }

    private void AddService()
    {
        var vm = new AddEditServiceDialogViewModel { IsEditMode = false };
        var dialog = new Views.AddEditServiceDialog { DataContext = vm };
        vm.CloseRequested += () =>
        {
            dialog.Close();
            LoadServices();
        };
        dialog.ShowDialog();
    }

    private void EditService(ServiceViewModel? service)
    {
        if (service == null) return;

        using var db = new HcsDbContext();
        var entity = db.Services.FirstOrDefault(s => s.Id == service.Id);
        if (entity == null) return;

        var dialog = new Views.AddEditServiceDialog();
        if (dialog.DataContext is AddEditServiceDialogViewModel vm)
        {
            vm.IsEditMode = true;
            vm.Id = entity.Id;
            vm.Name = entity.Name;
            vm.SelectedUnitId = entity.UnitOfMeasurementId;
            vm.CloseRequested += () =>
            {
                dialog.Close();
                LoadServices();
            };
        }

        dialog.ShowDialog();
    }

    private void DeleteService(ServiceViewModel? service)
    {
        if (service == null) return;

        var result = MessageBox.Show($"Удалить услугу '{service.Name}'?", "Подтверждение", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        using var db = new HcsDbContext();
        var entity = db.Services.FirstOrDefault(s => s.Id == service.Id);
        if (entity == null) return;

        entity.IsDeleted = true;
        db.SaveChanges();
        LoadServices();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}