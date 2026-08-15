namespace TmsApi.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using System.Reflection;
using BCrypt.Net;

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
                new Role("Bidder")
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Seed or ensure Users & UserRoles (idempotent)
        var adminEmail = "admin@tms.com";
        var defaultAdminPassword = "Admin123!"; // Development-only default password

        // Ensure Admin user exists
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

        var hashedAdminPassword = BCrypt.HashPassword(defaultAdminPassword);

        if (adminUser == null)
        {
            adminUser = new User("System Admin", adminEmail, hashedAdminPassword);
            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync(); // Persist to get Id
        }
        else
        {
            // Update password hash to known dev password (idempotent and safe for local dev)
            var pwProp = typeof(User).GetProperty("PasswordHash", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (pwProp != null)
            {
                pwProp.SetValue(adminUser, hashedAdminPassword);
                context.Users.Update(adminUser);
                await context.SaveChangesAsync();
            }
        }

        // Ensure Admin role assignment
        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
        var alreadyAssigned = await context.UserRoles.AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
        if (!alreadyAssigned)
        {
            await context.UserRoles.AddAsync(new UserRole(adminUser.Id, adminRole.Id));
            await context.SaveChangesAsync();
        }

        // Ensure other seed users exist only if no users besides admin exist
        var otherUsersExist = await context.Users.CountAsync() > 1;
        if (!otherUsersExist)
        {
            var officer = new User("Abebe Kebede", "officer@tms.com", BCrypt.HashPassword("Officer123!"));
            var evaluator = new User("Dr. Tigist Haile", "evaluator@tms.com", BCrypt.HashPassword("Eval123!"));
            var bidderUser = new User("Ethio Tech Solutions", "bidder@ethiotech.com", BCrypt.HashPassword("Bidder123!"));

            await context.Users.AddRangeAsync(officer, evaluator, bidderUser);
            await context.SaveChangesAsync(); // Generates User IDs

            var officerRole = await context.Roles.FirstAsync(r => r.Name == "TenderOfficer");
            var evaluatorRole = await context.Roles.FirstAsync(r => r.Name == "Evaluator");
            var bidderRole = await context.Roles.FirstAsync(r => r.Name == "Bidder");

            var userRoles = new List<UserRole>
            {
                new(officer.Id, officerRole.Id),
                new(evaluator.Id, evaluatorRole.Id),
                new(bidderUser.Id, bidderRole.Id)
            };

            await context.UserRoles.AddRangeAsync(userRoles);

            // 3. Seed Supplier Profile linked to bidderUser
            var supplierProfile = new SupplierProfile(
                userId: bidderUser.Id,
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