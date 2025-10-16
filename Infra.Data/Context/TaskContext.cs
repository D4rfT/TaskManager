using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TaskManager.Domain.Entities;

namespace TaskManager.Infra.Data.Context
{
    public class TaskContext : DbContext
    {
        public TaskContext(DbContextOptions<TaskContext> options) : base(options)
        {

        }
        public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskContext>
        {
            public TaskContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<TaskContext>();
                var dbPath = @"C:\Users\Daniel\source\repos\TaskManager\Infra.Data\bin\Debug\net8.0\TaskManager.db";
                optionsBuilder.UseSqlite($"Data Source={dbPath}");

                return new TaskContext(optionsBuilder.Options);
            }
        }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.UserName).IsUnique();

                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.PasswordHash).IsRequired();

                entity.Property(e => e.PasswordSalt).IsRequired();

                entity.Property(e => e.CreatedAt).IsRequired();

                entity.Property(e => e.UpdatedAt).IsRequired();

                // Relacionamento: 1 User → Muitas Tasks
                entity.HasMany(u => u.Tasks)
                    .WithOne(t => t.User)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

            });

            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);

                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.DueDate).IsRequired();

                entity.Property(e => e.IsCompleted).IsRequired();

                entity.Property(e => e.CreatedAt).IsRequired();

                entity.Property(e => e.UpdatedAt);
                //Chave estrangeira
                entity.Property(e => e.UserId).IsRequired();
            });

            base.OnModelCreating(modelBuilder);

        }
    }
}