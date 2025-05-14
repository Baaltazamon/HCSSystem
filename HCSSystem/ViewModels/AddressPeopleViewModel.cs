using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using HCSSystem.ViewModels.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using HCSSystem.Views;

namespace HCSSystem.ViewModels;

public class AddressPeopleViewModel : INotifyPropertyChanged
{
    private readonly int _addressId;

    public ObservableCollection<PersonAtAddressDto> People { get; set; } = new();
    private PersonAtAddressDto _selectedPerson;
    public PersonAtAddressDto SelectedPerson
    {
        get => _selectedPerson;
        set
        {
            _selectedPerson = value;
            OnPropertyChanged();
            RaiseCommands();
        }
    }
    public int AddressId => _addressId;
    public ICommand DeleteCommand { get; }
    public ICommand RegisterOwnerCommand { get; }
    public ICommand OpenRegisterResidentCommand { get; }
    public AddressPeopleViewModel(int addressId)
    {
        _addressId = addressId;
        OpenRegisterResidentCommand = new RelayCommand(_ => OpenRegisterResident());
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedPerson != null);
        RegisterOwnerCommand = new RelayCommand(_ => RegisterOwner(), _ => SelectedPerson?.IsOwner == true);

        LoadPeople();
    }

    public void RefreshPeople() => LoadPeople();

    private void RaiseCommands()
    {
        (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RegisterOwnerCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void OpenRegisterResident()
    {
        var dialog = new AddResidentDialog(_addressId);
        dialog.ShowDialog();
        LoadPeople(); // обновим список после закрытия
    }

    private void LoadPeople()
    {
        using var db = new HcsDbContext();

        People.Clear();

        var owners = db.ClientAddresses
            .Where(ca => ca.AddressId == _addressId)
            .Include(ca => ca.Client)
            .ToList();

        foreach (var owner in owners)
        {
            People.Add(new PersonAtAddressDto
            {
                Id = owner.Id,
                FullName = $"{owner.Client.LastName} {owner.Client.FirstName} {owner.Client.MiddleName}",
                BirthDate = owner.Client.BirthDate,
                IsOwner = true,
                RegistrationDate = null,
                EndRegistrationDate = null
            });
        }

        var residents = db.Residents
            .Where(r => r.AddressId == _addressId && !r.IsDeleted)
            .ToList();

        foreach (var r in residents)
        {
            People.Add(new PersonAtAddressDto
            {
                Id = r.Id,
                FullName = $"{r.LastName} {r.FirstName} {r.MiddleName}",
                BirthDate = r.BirthDate,
                IsOwner = false,
                RegistrationDate = r.RegistrationDate,
                EndRegistrationDate = r.EndRegistrationDate
            });
        }
    }

    private void DeleteSelected()
    {
        if (SelectedPerson == null) return;

        using var db = new HcsDbContext();

        if (SelectedPerson.IsOwner)
        {
            var entity = db.ClientAddresses.FirstOrDefault(x => x.Id == SelectedPerson.Id);
            if (entity != null)
            {
                db.ClientAddresses.Remove(entity);
                db.SaveChanges();
            }
        }
        else
        {
            var entity = db.Residents.FirstOrDefault(x => x.Id == SelectedPerson.Id);
            if (entity != null)
            {
                db.Residents.Remove(entity);
                db.SaveChanges();
            }
        }

        LoadPeople();
    }

    private void RegisterOwner()
    {
        if (SelectedPerson == null || !SelectedPerson.IsOwner) return;

        using var db = new HcsDbContext();

        var ca = db.ClientAddresses
            .Include(ca => ca.Client)
            .FirstOrDefault(x => x.Id == SelectedPerson.Id);

        if (ca == null) return;

        db.Residents.Add(new Resident
        {
            AddressId = _addressId,
            FirstName = ca.Client.FirstName,
            LastName = ca.Client.LastName,
            MiddleName = ca.Client.MiddleName,
            BirthDate = ca.Client.BirthDate,
            RegistrationDate = DateTime.Today,
            IsDeleted = false
        });

        db.SaveChanges();
        LoadPeople();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
