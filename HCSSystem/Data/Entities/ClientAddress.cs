namespace HCSSystem.Data.Entities
{
    public class ClientAddress
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int AddressId { get; set; }
        public DateTime OwnershipStartDate { get; set; }
        public DateTime? OwnershipEndDate { get; set; }
        public string PersonalAccountNumber { get; set; }

        public Client Client { get; set; }
        public Address Address { get; set; }
    }
}