using HCSSystem.Data;
using HCSSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HCSSystem.Helpers
{
    public static class PaymentGenerator
    {
        public static Payment? TryGeneratePaymentForReading(MeterReading reading, HcsDbContext db)
        {
            if (!reading.IsApproved)
                return null;

            var meter = db.Meters
                .Include(m => m.Service)
                    .ThenInclude(s => s.UnitOfMeasurement)
                .FirstOrDefault(m => m.Id == reading.MeterId);

            if (meter == null)
                return null;

            var service = meter.Service;
            var unit = service.UnitOfMeasurement.Name;

            var alreadyExists = db.Payments.Any(p => p.MeterReadingId == reading.Id);
            if (alreadyExists)
                return null;

            var lastRate = db.Rates
                .Where(r => r.ServiceId == service.Id && r.EffectiveFrom <= reading.ReadingDate)
                .OrderByDescending(r => r.EffectiveFrom)
                .FirstOrDefault();

            if (lastRate == null)
                return null;

            var currentValue = reading.Value;

            var previousReading = db.MeterReadings
                .Where(r => r.MeterId == meter.Id && r.IsApproved && r.ReadingDate < reading.ReadingDate)
                .OrderByDescending(r => r.ReadingDate)
                .FirstOrDefault();

            var volume = previousReading != null
                ? currentValue - previousReading.Value
                : currentValue;

            if (volume < 0)
                return null;

            if (unit is "услуга" or "чел." or "руб.")
                volume = 1;

            var rawAmount = Math.Round(volume * lastRate.PricePerUnit, 2);

            var allPayments = db.Payments
                .Where(p => p.MeterReading.MeterId == meter.Id)
                .ToList();

            var totalPaid = allPayments.Sum(p => p.AmountPaid);
            var totalBilled = allPayments.Sum(p => p.AmountToPay);
            var overpayment = Math.Max(totalPaid - totalBilled, 0);

            var amountToPay = Math.Max(rawAmount - overpayment, 0);

            var status = amountToPay == 0
                ? 3 // Оплачено
                : overpayment > 0
                    ? 2 // Частично оплачено
                    : 1; // Не оплачено

            var payment = new Payment
            {
                MeterReadingId = reading.Id,
                PeriodStart = reading.ReadingDate,
                PeriodEnd = reading.ReadingDate.AddMonths(1).AddDays(-1),
                AmountToPay = rawAmount,
                AmountPaid = rawAmount - amountToPay,
                PaymentStatusId = status,
                PaymentDate = DateTime.Now,
                ApprovedByUserId = App.CurrentUser?.Id
            };

            db.Payments.Add(payment);
            db.SaveChanges();

            return payment;
        }
    }

}
