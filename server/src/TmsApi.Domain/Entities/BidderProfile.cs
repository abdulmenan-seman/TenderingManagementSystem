namespace TmsApi.Domain.Entities;

public class BidderProfile
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string CompanyName { get; private set; } = default!;
    public string ContactPerson { get; private set; } = default!;
    public string BusinessLicenseNo { get; private set; } = default!;
    public string TaxId { get; private set; } = default!; 
    public string Address { get; private set; } = default!;

    private BidderProfile() { }

    public BidderProfile(int userId, string companyName, string contactPerson, string businessLicenseNo, string taxId, string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactPerson);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessLicenseNo);
        ArgumentException.ThrowIfNullOrWhiteSpace(taxId);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        UserId = userId;
        CompanyName = companyName;
        ContactPerson = contactPerson;
        BusinessLicenseNo = businessLicenseNo;
        TaxId = taxId;
        Address = address;
    }
}
