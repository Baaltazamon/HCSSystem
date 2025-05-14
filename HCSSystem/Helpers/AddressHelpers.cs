using HCSSystem.Data.Entities;

namespace HCSSystem.Helpers
{
    public static class AddressHelpers
    {
        public static string ToFullAddress(Address address)
        {
            if (address == null)
                return string.Empty;

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(address.City))
                parts.Add(address.City);

            if (!string.IsNullOrWhiteSpace(address.Street))
                parts.Add($"{address.Street} ул.");

            if (!string.IsNullOrWhiteSpace(address.HouseNumber))
                parts.Add($"д. {address.HouseNumber}");

            if (!string.IsNullOrWhiteSpace(address.Building))
                parts.Add($"корп. {address.Building}");

            if (!string.IsNullOrWhiteSpace(address.ApartmentNumber))
                parts.Add($"кв. {address.ApartmentNumber}");

            return string.Join(", ", parts);
        }
    }
}