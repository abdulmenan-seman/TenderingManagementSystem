namespace TmsApi.Domain.Entities;

public class Role
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!; // e.g., Administrator, ProcurementOfficer, Supplier, EvaluationCommittee
    public string? Description { get; private set; }

    private Role() { }

    public Role(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

        Name = name;
        Description = description;
    }
}