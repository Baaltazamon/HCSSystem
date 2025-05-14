namespace HCSSystem.Data.Entities
{
    public class Meter
    {
        public int Id { get; set; }
        public int AddressId { get; set; }
        public int ServiceId { get; set; }
        public string MeterNumber { get; set; }
        public DateTime InstallationDate { get; set; }
        public bool IsDeleted { get; set; }

        public Address Address { get; set; }
        public Service Service { get; set; }
        public ICollection<MeterReading> MeterReadings { get; set; }
    }
}