using Microsoft.EntityFrameworkCore;
using SchoolMS.Models;

namespace SchoolMS.Data
{
    public class SchoolContext : DbContext
    {
        public SchoolContext(DbContextOptions<SchoolContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<EducationalStage> EducationalStages { get; set; }
        public DbSet<Fee> Fees { get; set; }
        public DbSet<Installment> Installments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure one-to-many relationship between EducationalStage and Grade
            modelBuilder.Entity<EducationalStage>()
                .HasMany(es => es.Grades)
                .WithOne(g => g.EducationalStage)
                .HasForeignKey(g => g.EducationalStageId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Specify the delete behavior

            // Configure one-to-many relationship between Grade and Student
            modelBuilder.Entity<Grade>()
                .HasMany(g => g.Students)
                .WithOne(s => s.Grade)
                .HasForeignKey(s => s.GradeId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Specify the delete behavior

            // Configure one-to-many relationship between Student and Fee
            modelBuilder.Entity<Student>()
                .HasMany(s => s.Fees)
                .WithOne(f => f.Student)
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Specify the delete behavior

            // Configure one-to-many relationship between Fee and Installment
            modelBuilder.Entity<Fee>()
                .HasMany(f => f.Installments)
                .WithOne(i => i.Fee)
                .HasForeignKey(i => i.FeeId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Specify the delete behavior

            base.OnModelCreating(modelBuilder);
        }
    }
}
