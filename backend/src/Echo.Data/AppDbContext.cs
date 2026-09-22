using System.Reflection;
using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Auth;
using Echo.Domain.Congregations;
using Echo.Domain.Events;
using Echo.Domain.Members;
using Echo.Domain.Organizations;
using Echo.Domain.Projects;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;
using Echo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Echo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Tables
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetCategory> AssetCategories { get; set; }
    public DbSet<Attendance> AttendanceRecords { get; set; }
    public DbSet<AttendanceType> AttendanceTypes { get; set; }
    public DbSet<AttendanceContext> AttendanceContexts { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventAttendance> EventAttendances { get; set; }
    public DbSet<EventRegistration> EventRegistrations { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationMember> OrganizationMembers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectCategory> ProjectCategories { get; set; }
    public DbSet<ProjectContribution> ProjectContributions { get; set; }
    public DbSet<Tithe> Tithes { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionCategory> TransactionCategories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Congregation> Congregations { get; set; }
    public DbSet<InvitationToken> InvitationTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordVerificationTokens { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
        configurationBuilder.Properties<Enum?>().HaveConversion<string>();
        configurationBuilder.Properties<string>().HaveMaxLength(255);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
