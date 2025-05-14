using HCSSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HCSSystem.ViewModels.Models;
using System.Windows.Input;
using HCSSystem.Helpers;
using System.Windows;

namespace HCSSystem.ViewModels;

public class PaymentsViewModel : INotifyPropertyChanged
{
    private string _searchQuery = string.Empty;
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
    public bool CanEdit => App.CurrentUser?.Role?.Name is "Администратор" or "Сотрудник";
    public ICommand OpenPaymentDialogCommand { get; }
    public ObservableCollection<PaymentViewModel> FilteredPayments { get; set; } = new();
    private List<PaymentViewModel> _allPayments = new();
    public ICommand ExportPaymentsReportCommand { get; }
    public PaymentsViewModel()
    {
        OpenPaymentDialogCommand = new RelayCommand(p => OpenPaymentDialog(p as PaymentViewModel), _ => CanEdit);
        ExportPaymentsReportCommand = new RelayCommand(_ =>
        {
            if (App.CurrentUser?.Role?.Name is "Администратор" or "Сотрудник")
                ReportService.GeneratePaymentsReport();
            else
                ReportService.GeneratePaymentsReportForClient(App.CurrentUser!.Id);

            MessageBox.Show("Отчёт по платежам сформирован.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
        });
        LoadPayments();
    }

    private void LoadPayments()
    {
        using var db = new HcsDbContext();

        var isClient = App.CurrentUser?.Role?.Name == "Клиент";

        var query = db.Payments
            .Include(p => p.MeterReading)
                .ThenInclude(r => r.Meter)
                    .ThenInclude(m => m.Address)
            .Include(p => p.MeterReading.Meter.Service)
            .Include(p => p.PaymentStatus)
            .Include(p => p.ApprovedByUser)
            .AsQueryable();

        if (isClient && App.CurrentUser != null)
        {
            var addressIds = db.ClientAddresses
                .Where(ca => ca.ClientId == App.CurrentUser.Id &&
                             ca.OwnershipStartDate <= DateTime.Today &&
                             (ca.OwnershipEndDate == null || ca.OwnershipEndDate >= DateTime.Today))
                .Select(ca => ca.AddressId)
                .ToList();

            query = query.Where(p => addressIds.Contains(p.MeterReading.Meter.AddressId));
        }

        _allPayments = query
            .OrderByDescending(p => p.PeriodStart)
            .Select(p => new PaymentViewModel
            {
                Period = $"{p.PeriodStart:MM.yyyy}",
                AmountToPay = p.AmountToPay,
                AmountPaid = p.AmountPaid,
                Status = p.PaymentStatus.Name,
                ApprovedBy = p.ApprovedByUserId != null ? p.ApprovedByUser.Username : string.Empty,
                PaymentDate = p.PaymentDate != null ? p.PaymentDate.Value.ToString("dd.MM.yyyy") : null,
                MeterNumber = p.MeterReading.Meter.MeterNumber
            })
            .ToList();

        ApplyFilter();
    }

    private void OpenPaymentDialog(PaymentViewModel? payment)
    {
        if (payment == null) return;

        var dialogVm = new EnterPaymentAmountViewModel(payment);
        var dialog = new Views.EnterPaymentAmountDialog { DataContext = dialogVm };
        dialogVm.CloseRequested += () =>
        {
            dialog.Close();
            LoadPayments();
        };
        dialog.ShowDialog();
    }


    private void ApplyFilter()
    {
        var filtered = _allPayments.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.ToLower();
            filtered = filtered.Where(p => p.MeterNumber.ToLower().Contains(q));
        }

        FilteredPayments.Clear();
        foreach (var p in filtered)
            FilteredPayments.Add(p);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
