namespace HCSSystem.ViewModels.Models;

public class PaymentViewModel
{
    public string Period { get; set; }
    public decimal AmountToPay { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; }
    public string ApprovedBy { get; set; }
    public string? PaymentDate { get; set; }
    public string MeterNumber { get; set; }
}