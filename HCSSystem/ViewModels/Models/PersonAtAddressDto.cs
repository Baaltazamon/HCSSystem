namespace HCSSystem.ViewModels.Models
{
    public class PersonAtAddressDto
    {
        public int Id { get; set; }                     
        public string FullName { get; set; }              
        public DateTime BirthDate { get; set; }          
        public bool IsOwner { get; set; }                  
        public DateTime? RegistrationDate { get; set; }    
        public DateTime? EndRegistrationDate { get; set; } 
    }
}
