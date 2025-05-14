namespace HCSSystem.Data.Entities
{
    public class MeterReading
    {
        public int Id { get; set; }
        public int MeterId { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
        public bool IsApproved { get; set; }
        public int? ApprovedByUserId { get; set; }

        public Meter Meter { get; set; }
        public User ApprovedByUser { get; set; }
    }
}