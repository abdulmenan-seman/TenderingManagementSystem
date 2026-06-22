// using System;

// namespace TmsCoreApi.Models;

// public enum UserRole
// {
//     Administrator,
//     ProcurementOfficer,
//     Supplier,
//     EvaluationCommitteeMember    
// }

// public class User
// {
//     public required string Id { get; init; }
//     public required string Email { get; set; }
//     public required string Name { get; set; }
//     public required UserRole Role { get; set; }
//     public bool IsActive { get; set; } = true;
//     public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
// }