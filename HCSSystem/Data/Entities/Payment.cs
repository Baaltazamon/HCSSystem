namespace HCSSystem.Data.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        
        public int MeterReadingId { get; set; }

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public decimal AmountToPay { get; set; }
        public decimal AmountPaid { get; set; }

        public int PaymentStatusId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int? ApprovedByUserId { get; set; }

        public MeterReading MeterReading { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public User ApprovedByUser { get; set; }
    }
}