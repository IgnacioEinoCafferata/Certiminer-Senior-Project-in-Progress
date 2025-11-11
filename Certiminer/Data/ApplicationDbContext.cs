using Certiminer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // ====== TUS ENTIDADES EXISTENTES ======


        public DbSet<Question> Questions => Set<Question>();
        public DbSet<AnswerQuestion> AnswerQuestions => Set<AnswerQuestion>();
        public DbSet<TestAttempt> TestAttempts => Set<TestAttempt>();
        // (si tienes Answer, Result, etc., puedes declararlas acá también:)
        // public DbSet<Answer> Answers => Set<Answer>();

        // ====== LAS QUE VENIMOS USANDO ======
        public DbSet<Test> Tests => Set<Test>();
        public DbSet<Video> Videos => Set<Video>();
        public DbSet<Folder> Folders => Set<Folder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------- Folder (Chapter) --------
            modelBuilder.Entity<Folder>(b =>
            {
                b.Property(f => f.Name).HasColumnType("nvarchar(128)").IsRequired();
                b.Property(f => f.Kind).HasColumnType("int").IsRequired();
                b.HasIndex(f => new { f.Kind, f.Name }).IsUnique(false);
            });

            // -------- Test -> Chapter --------
            modelBuilder.Entity<Test>(b =>
            {
                b.Property(t => t.Title).HasColumnType("nvarchar(256)").IsRequired();
                b.Property(t => t.IsActive).HasColumnType("bit").IsRequired();
                b.Property(t => t.FolderId).HasColumnType("int");

                b.HasOne(t => t.Folder)
                 .WithMany()
                 .HasForeignKey(t => t.FolderId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // -------- Video -> Chapter --------
            modelBuilder.Entity<Video>(b =>
            {
                b.Property(v => v.Title).HasColumnType("nvarchar(256)").IsRequired();
                b.Property(v => v.Url).HasColumnType("nvarchar(2048)").IsRequired();
                b.Property(v => v.SourceType).HasColumnType("int").IsRequired();
                b.Property(v => v.IsActive).HasColumnType("bit").IsRequired();
                b.Property(v => v.FolderId).HasColumnType("int");

                b.HasOne(v => v.Folder)
                 .WithMany()
                 .HasForeignKey(v => v.FolderId)
                 .OnDelete(DeleteBehavior.SetNull);

                // Si tu clase Video aún tiene TestId/Test, no configuramos relación (ya no se usa).
            });

            // ====== (Opcional) Config extra para tus otras entidades ======
            // Si AnswerQuestion NO tiene Id y usa clave compuesta, agrega algo como:
            // modelBuilder.Entity<AnswerQuestion>()
            //     .HasKey(x => new { x.AnswerId, x.QuestionId });
        }
    }
}
