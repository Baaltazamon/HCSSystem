using ClosedXML.Excel;
using HCSSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows;

namespace HCSSystem.Helpers;

public static class ReportService
{
    public static void GenerateAddressReport()
    {
        using var db = new HcsDbContext();

        var data = db.ClientAddresses
            .Where(ca => !ca.Address.IsDeleted)
            .Select(ca => new
            {
                Address = ca.Address.City + ", " + ca.Address.Street + " " + ca.Address.HouseNumber,
                Owner = ca.Client.LastName + " " + ca.Client.FirstName + " " + ca.Client.MiddleName,
                Residents = db.Residents
                    .Where(r => r.AddressId == ca.AddressId && !r.IsDeleted)
                    .Select(r => r.LastName + " " + r.FirstName + " " + r.MiddleName)
                    .ToList()
            })
            .ToList();

        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Адреса");

        sheet.Cell(1, 1).Value = "№";
        sheet.Cell(1, 2).Value = "Адрес";
        sheet.Cell(1, 3).Value = "Владелец";
        sheet.Cell(1, 4).Value = "Зарегистрированные";

        int row = 2;
        for (int i = 0; i < data.Count; i++)
        {
            sheet.Cell(row, 1).Value = i + 1;
            sheet.Cell(row, 2).Value = data[i].Address;
            sheet.Cell(row, 3).Value = data[i].Owner;
            sheet.Cell(row, 4).Value = string.Join(", ", data[i].Residents);
            row++;
        }

        // Save
        var dialog = new SaveFileDialog
        {
            Filter = "Excel файлы (*.xlsx)|*.xlsx",
            FileName = "Отчет_по_адресам.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                workbook.SaveAs(dialog.FileName);
                MessageBox.Show("Отчёт успешно сохранён!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public static void GenerateMeterReadingsReport()
    {
        using var db = new HcsDbContext();

        var meters = db.Meters
            .Include(m => m.Service)
            .Include(m => m.Address)
            .Where(m => !m.IsDeleted)
            .ToList();

        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Показания");

        sheet.Cell(1, 1).Value = "№";
        sheet.Cell(1, 2).Value = "Адрес";
        sheet.Cell(1, 3).Value = "Счётчик";
        sheet.Cell(1, 4).Value = "Услуга";
        sheet.Cell(1, 5).Value = "Показания";

        int row = 2;
        int index = 1;

        foreach (var meter in meters)
        {
            var readings = db.MeterReadings
                .Where(r => r.MeterId == meter.Id)
                .OrderBy(r => r.ReadingDate)
                .ToList();

            string readingsText = string.Join(", ", readings.Select(r =>
                $"{r.ReadingDate:dd.MM.yyyy} - {r.Value} ({(r.IsApproved ? "Подтв." : "Нет")})"));

            sheet.Cell(row, 1).Value = index++;
            sheet.Cell(row, 2).Value = AddressHelpers.ToFullAddress(meter.Address);
            sheet.Cell(row, 3).Value = meter.MeterNumber;
            sheet.Cell(row, 4).Value = meter.Service.Name;
            sheet.Cell(row, 5).Value = readingsText;
            row++;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Excel файлы (*.xlsx)|*.xlsx",
            FileName = "Отчет_по_показаниям.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                workbook.SaveAs(dialog.FileName);
                MessageBox.Show("Отчёт успешно сохранён!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public static void GenerateMeterReadingsReportForClient(int userId)
    {
        using var db = new HcsDbContext();

        var addressIds = db.ClientAddresses
            .Where(ca => ca.Client.User.Id == userId)
            .Select(ca => ca.AddressId)
            .ToHashSet();

        var meters = db.Meters
            .Include(m => m.Service)
            .Include(m => m.Address)
            .Where(m => !m.IsDeleted && addressIds.Contains(m.AddressId))
            .ToList();

        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Показания");

        sheet.Cell(1, 1).Value = "№";
        sheet.Cell(1, 2).Value = "Адрес";
        sheet.Cell(1, 3).Value = "Счётчик";
        sheet.Cell(1, 4).Value = "Услуга";
        sheet.Cell(1, 5).Value = "Показания";

        int row = 2;
        int index = 1;

        foreach (var meter in meters)
        {
            var readings = db.MeterReadings
                .Where(r => r.MeterId == meter.Id)
                .OrderBy(r => r.ReadingDate)
                .ToList();

            string readingsText = string.Join(", ", readings.Select(r =>
                $"{r.ReadingDate:dd.MM.yyyy} - {r.Value} ({(r.IsApproved ? "Подтв." : "Нет")})"));

            sheet.Cell(row, 1).Value = index++;
            sheet.Cell(row, 2).Value = AddressHelpers.ToFullAddress(meter.Address);
            sheet.Cell(row, 3).Value = meter.MeterNumber;
            sheet.Cell(row, 4).Value = meter.Service.Name;
            sheet.Cell(row, 5).Value = readingsText;
            row++;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Excel файлы (*.xlsx)|*.xlsx",
            FileName = "Мои_показания.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                workbook.SaveAs(dialog.FileName);
                MessageBox.Show("Отчёт успешно сохранён!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public static void GeneratePaymentsReport()
    {
        using var db = new HcsDbContext();

        var payments = db.Payments
            .Include(p => p.MeterReading).ThenInclude(r => r.Meter).ThenInclude(m => m.Address)
            .Include(p => p.MeterReading).ThenInclude(r => r.Meter).ThenInclude(m => m.Service)
            .Include(p => p.PaymentStatus)
            .Include(p => p.ApprovedByUser)
            .ToList();

        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Платежи");

        sheet.Cell(1, 1).Value = "№";
        sheet.Cell(1, 2).Value = "Адрес";
        sheet.Cell(1, 3).Value = "Услуга";
        sheet.Cell(1, 4).Value = "Статус";
        sheet.Cell(1, 5).Value = "Счётчик";
        sheet.Cell(1, 6).Value = "Сумма";
        sheet.Cell(1, 7).Value = "Внесено";

        int row = 2;
        int index = 1;

        foreach (var p in payments)
        {
            sheet.Cell(row, 1).Value = index++;
            sheet.Cell(row, 2).Value = AddressHelpers.ToFullAddress(p.MeterReading.Meter.Address);
            sheet.Cell(row, 3).Value = p.MeterReading.Meter.Service.Name;
            sheet.Cell(row, 4).Value = p.PaymentStatus.Name;
            sheet.Cell(row, 5).Value = p.MeterReading.Meter.MeterNumber;
            sheet.Cell(row, 6).Value = p.AmountToPay;
            sheet.Cell(row, 7).Value = p.AmountPaid;
            row++;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Excel файлы (*.xlsx)|*.xlsx",
            FileName = "Отчет_по_платежам.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                workbook.SaveAs(dialog.FileName);
                MessageBox.Show("Отчёт успешно сохранён!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public static void GeneratePaymentsReportForClient(int userId)
    {
        using var db = new HcsDbContext();

        var addressIds = db.ClientAddresses
            .Where(ca => ca.Client.User.Id == userId)
            .Select(ca => ca.AddressId)
            .ToHashSet();

        var payments = db.Payments
            .Include(p => p.MeterReading).ThenInclude(r => r.Meter).ThenInclude(m => m.Address)
            .Include(p => p.MeterReading).ThenInclude(r => r.Meter).ThenInclude(m => m.Service)
            .Include(p => p.PaymentStatus)
            .Where(p => addressIds.Contains(p.MeterReading.Meter.AddressId))
            .ToList();

        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Мои платежи");

        sheet.Cell(1, 1).Value = "№";
        sheet.Cell(1, 2).Value = "Адрес";
        sheet.Cell(1, 3).Value = "Услуга";
        sheet.Cell(1, 4).Value = "Статус";
        sheet.Cell(1, 5).Value = "Счётчик";
        sheet.Cell(1, 6).Value = "Сумма";
        sheet.Cell(1, 7).Value = "Внесено";

        int row = 2;
        int index = 1;

        foreach (var p in payments)
        {
            sheet.Cell(row, 1).Value = index++;
            sheet.Cell(row, 2).Value = AddressHelpers.ToFullAddress(p.MeterReading.Meter.Address);
            sheet.Cell(row, 3).Value = p.MeterReading.Meter.Service.Name;
            sheet.Cell(row, 4).Value = p.PaymentStatus.Name;
            sheet.Cell(row, 5).Value = p.MeterReading.Meter.MeterNumber;
            sheet.Cell(row, 6).Value = p.AmountToPay;
            sheet.Cell(row, 7).Value = p.AmountPaid;
            row++;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Excel файлы (*.xlsx)|*.xlsx",
            FileName = "Мои_платежи.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                workbook.SaveAs(dialog.FileName);
                MessageBox.Show("Отчёт успешно сохранён!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

