namespace HCSSystem.Data.Entities
{
    public class UnitOfMeasurement
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Service> Services { get; set; }
    }
}