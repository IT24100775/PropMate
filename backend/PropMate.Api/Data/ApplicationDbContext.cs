using Microsoft.EntityFrameworkCore;
using PropMate.Api.Models.Maintenance;

namespace PropMate.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

        public DbSet<Technician> Technicians { get; set; }

        public DbSet<MaintenanceAssignment> MaintenanceAssignments { get; set; }

        public DbSet<RepairSchedule> RepairSchedules { get; set; }

        public DbSet<MaintenanceExpense> MaintenanceExpenses { get; set; }

        public DbSet<MaintenanceStatusHistory> MaintenanceStatusHistories { get; set; }

        public DbSet<MaintenanceNotification> MaintenanceNotifications { get; set; }

        
    }
}