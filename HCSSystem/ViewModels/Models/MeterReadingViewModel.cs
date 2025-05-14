namespace HCSSystem.ViewModels.Models
{
    public class MeterReadingViewModel
    {
        public int Id { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovedByLogin { get; set; }
    }
}
