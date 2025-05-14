namespace HCSSystem.ViewModels.Models;

public class MeterViewModel
{
    public int Id { get; set; }
    public string MeterNumber { get; set; }
    public DateTime InstallationDate { get; set; }
    public string ServiceName { get; set; }

    public string AddressString { get; set; }

    public int UnapprovedReadingsCount { get; set; }

    public string MeterNumberWithBadge =>
        UnapprovedReadingsCount > 0
            ? $"{MeterNumber} ({UnapprovedReadingsCount})"
            : MeterNumber;
}