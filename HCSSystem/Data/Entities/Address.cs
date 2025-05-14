namespace HCSSystem.Data.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string? Street { get; set; }
        public string HouseNumber { get; set; }
        public string? Building { get; set; }
        public string ApartmentNumber { get; set; }
        public decimal PropertyArea { get; set; }
        public bool IsResidential { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<ClientAddress> ClientsAddresses { get; set; }
        public ICollection<Resident> Residents { get; set; }
    }
}