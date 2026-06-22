using Microsoft.EntityFrameworkCore;
using TmsCoreApi.Entities;
namespace TmsApi.Data;
public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Tender> Tenders => Set<Tender>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();
}