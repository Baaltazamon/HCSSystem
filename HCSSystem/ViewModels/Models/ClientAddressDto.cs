namespace HCSSystem.ViewModels.Models;

public class ClientAddressDto
{
    public int AddressId { get; set; }
    public string FullAddress { get; set; }
    public string? PersonalAccountNumber { get; set; }
    public DateTime? OwnershipStartDate { get; set; }

}