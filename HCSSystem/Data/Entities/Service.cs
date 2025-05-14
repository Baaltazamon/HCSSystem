namespace HCSSystem.Data.Entities
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UnitOfMeasurementId { get; set; }
        public bool IsDeleted { get; set; }

        public UnitOfMeasurement UnitOfMeasurement { get; set; }
        public ICollection<Rate> Rates { get; set; }
        public ICollection<Meter> Meters { get; set; }
    }
}