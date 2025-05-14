namespace HCSSystem.Data.Entities
{
    public class Rate
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public decimal PricePerUnit { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }

        public Service Service { get; set; }
        public User CreatedByUser { get; set; }
    }
}