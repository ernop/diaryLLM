using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace DiaryUI
{
    public class DiaryDbContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Transcript> Transcripts { get; set; }
        public DbSet<TagInstance> TagInstances { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Chunk> Chunks { get; set; }
        public DbSet<Query> Queries { get; set; }


        public static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddEventLog();
        });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(loggerFactory)
            .EnableSensitiveDataLogging()
            .UseSqlite("Data Source=../diaryUI5.db");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //fks so that the shadow item actually works.
            //also cascade delete from transcripts.
            modelBuilder.Entity<Transcript>()
                .Property<int>("RecorderForeignKey");

            modelBuilder.Entity<Transcript>()
                .HasOne(p => p.Recorder)
                .WithMany(b => b.Transcripts)
                .HasForeignKey("RecorderForeignKey")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade); ;

            modelBuilder.Entity<Chunk>()
                .Property<int>("ChunkForeignKey");

            modelBuilder.Entity<Chunk>()
                .HasOne(p => p.Transcript)
                .WithMany(b => b.Chunks)
                .HasForeignKey("ChunkForeignKey")
                .IsRequired(false);

            modelBuilder.Entity<Query>()
                .Property<int>("QueryForeignKey");

            modelBuilder.Entity<Query>()
                .HasOne(p => p.Chunk)
                .WithMany(b => b.Queries)
                .HasForeignKey("QueryForeignKey")
                .IsRequired(false);
        }

    }
}