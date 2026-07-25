namespace TmsApi.Domain.Entities;

public class SupplierProfile
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string CompanyName { get; private set; } = default!;
    public string TaxIdNumber { get; private set; } = default!; // TIN
    public string BusinessLicenseNumber { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;

    // Navigation property
    public User User { get; private set; } = default!;

    private SupplierProfile() { }

    public SupplierProfile(int userId, string companyName, string taxIdNumber, string businessLicenseNumber, string address, string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(taxIdNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessLicenseNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);

        UserId = userId;
        CompanyName = companyName;
        TaxIdNumber = taxIdNumber;
        BusinessLicenseNumber = businessLicenseNumber;
        Address = address;
        PhoneNumber = phoneNumber;
    }
}