using HCSSystem.Data;
using HCSSystem.Data.Entities;
using HCSSystem.Helpers;
using HCSSystem.ViewModels.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace HCSSystem.ViewModels;

public class EnterPaymentAmountViewModel : INotifyPropertyChanged
{
    private readonly PaymentViewModel _paymentDto;
    public event Action? CloseRequested;

    public decimal AmountToPay { get; }
    public decimal CurrentPaid { get; }

    private decimal _enteredAmount;
    public decimal EnteredAmount
    {
        get => _enteredAmount;
        set
        {
            _enteredAmount = value;
            OnPropertyChanged();
        }
    }

    public string MeterNumber => _paymentDto.MeterNumber;
    public string Period => _paymentDto.Period;

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public EnterPaymentAmountViewModel(PaymentViewModel paymentDto)
    {
        _paymentDto = paymentDto;

        AmountToPay = paymentDto.AmountToPay;
        CurrentPaid = paymentDto.AmountPaid;
        EnteredAmount = 0;

        ConfirmCommand = new RelayCommand(_ => Confirm());
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());
    }

    private void Confirm()
    {
        if (EnteredAmount <= 0)
        {
            MessageBox.Show("Сумма должна быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = new HcsDbContext();

        var entity = db.Payments
            .Include(p => p.MeterReading)
            .ThenInclude(r => r.Meter)
            .ToList() // 👉 делаем всё ниже на клиенте
            .FirstOrDefault(p =>
                p.MeterReading.Meter.MeterNumber == _paymentDto.MeterNumber &&
                p.PeriodStart.ToString("MM.yyyy") == _paymentDto.Period);


        if (entity == null)
        {
            MessageBox.Show("Платёж не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        entity.AmountPaid += EnteredAmount;

        if (entity.AmountPaid >= entity.AmountToPay)
        {
            entity.PaymentStatusId = 3; // Оплачено
        }
        else
        {
            entity.PaymentStatusId = 2; // Частично оплачено
        }

        entity.PaymentDate = DateTime.Now;
        entity.ApprovedByUserId = App.CurrentUser?.Id;

        db.SaveChanges();
        CloseRequested?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
