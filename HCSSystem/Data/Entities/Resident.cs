namespace HCSSystem.Data.Entities
{
    public class Resident
    {
        public int Id { get; set; }
        public int AddressId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? EndRegistrationDate { get; set; }
        public bool IsDeleted { get; set; }

        public Address Address { get; set; }
    }
}