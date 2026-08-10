namespace TmsApi.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

public static class DbInitializer
{
    public static async Task SeedAsync(TmsDbContext context)
    {
        // Apply any pending EF migrations automatically
        await context.Database.MigrateAsync();

        // 1. Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role("Admin"),
                new Role("TenderOfficer"),
                new Role("Evaluator"),
                new Role("Supplier")
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Seed Users & UserRoles
        if (!await context.Users.AnyAsync())
        {
            var admin = new User("System Admin", "admin@tms.com", "$2a$11$e8O0aK7X0Kz4Z0LqR...");
            var officer = new User("Abebe Kebede", "officer@tms.com", "$2a$11$e8O0aK7X0Kz4Z0LqR...");
            var evaluator = new User("Dr. Tigist Haile", "evaluator@tms.com", "$2a$11$e8O0aK7X0Kz4Z0LqR...");
            var supplierUser = new User("Ethio Tech Solutions", "supplier@ethiotech.com", "$2a$11$e8O0aK7X0Kz4Z0LqR...");

            await context.Users.AddRangeAsync(admin, officer, evaluator, supplierUser);
            await context.SaveChangesAsync(); // Generates User IDs

            // Retrieve role IDs
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var officerRole = await context.Roles.FirstAsync(r => r.Name == "TenderOfficer");
            var evaluatorRole = await context.Roles.FirstAsync(r => r.Name == "Evaluator");
            var supplierRole = await context.Roles.FirstAsync(r => r.Name == "Supplier");

            // Assign User Roles via UserRole join entity
            var userRoles = new List<UserRole>
            {
                new(admin.Id, adminRole.Id),
                new(officer.Id, officerRole.Id),
                new(evaluator.Id, evaluatorRole.Id),
                new(supplierUser.Id, supplierRole.Id)
            };

            await context.UserRoles.AddRangeAsync(userRoles);

            // 3. Seed Supplier Profile linked to supplierUser
            var supplierProfile = new SupplierProfile(
                userId: supplierUser.Id,
                companyName: "Ethio Tech Solutions PLC",
                taxIdNumber: "TIN-9876543210",
                businessLicenseNumber: "BL-2026-00412",
                address: "Bole Sub-City, Addis Ababa, Ethiopia",
                phoneNumber: "+251911223344"
            );

            await context.SupplierProfiles.AddAsync(supplierProfile);
            await context.SaveChangesAsync();
        }

        // 4. Seed Tenders
        if (!await context.Tenders.AnyAsync())
        {
            var officer = await context.Users.FirstAsync(u => u.Email == "officer@tms.com");

            var tender1 = new Tender(
                referenceNumber: "TMS-2026-001",
                title: "Procurement of Enterprise Server Infrastructure",
                description: "Supply, installation, and commissioning of rack servers and SAN storage.",
                estimatedBudget: 1500000.00m,
                submissionDeadline: DateTime.UtcNow.AddDays(30),
                createdByOfficerId: officer.Id
            );
            tender1.Publish(); // Transition status from Draft to Published for testing

            var tender2 = new Tender(
                referenceNumber: "TMS-2026-002",
                title: "Cloud Migration & Consulting Services",
                description: "Migration of legacy web services to PostgreSQL and Kubernetes environment.",
                estimatedBudget: 850000.00m,
                submissionDeadline: DateTime.UtcNow.AddDays(15),
                createdByOfficerId: officer.Id
            );
            tender2.Publish();

            await context.Tenders.AddRangeAsync(tender1, tender2);
            await context.SaveChangesAsync();
        }
    }
}