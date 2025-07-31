using Microsoft.EntityFrameworkCore;

namespace DataBaseGenerator.Core.Data
{
    public class BaseGenerateContext : DbContext
    {
        public DbSet<Patient> Patient { get; set; }

        public DbSet<WorkList> WorkList { get; set; }


        public BaseGenerateContext(DbContextOptions<BaseGenerateContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            #region Map entities to tables


            modelBuilder.Entity<Patient>().ToTable("Patient");

            modelBuilder.Entity<WorkList>().ToTable("WorkList");


            #endregion


            #region Configure Primary Keys


            modelBuilder.Entity<Patient>().HasKey(patient => patient.PatientID).HasName("PK_Patient");

            modelBuilder.Entity<Patient>().HasCharSet("Utf8");

            modelBuilder.Entity<WorkList>().HasKey(worklist => worklist.WorkListID).HasName("PK_WorkList");

            modelBuilder.Entity<WorkList>().HasCharSet("Utf8");


            #endregion


            #region Configure indexes

            //Patient index 

            modelBuilder.Entity<Patient>().HasIndex(patient => patient.PatientID).HasDatabaseName("Idx_Primary");

            modelBuilder.Entity<Patient>().HasIndex(patient => patient.PatientID).HasDatabaseName("PatientID_Unique").IsUnique();

            modelBuilder.Entity<Patient>().HasIndex(patient => patient.LastName).HasDatabaseName("LastName").IsFullText();

            modelBuilder.Entity<Patient>().HasIndex(patient => patient.FirstName).HasDatabaseName("FirstName").IsFullText();

            modelBuilder.Entity<Patient>().HasIndex(patient => patient.MiddleName).HasDatabaseName("MiddleName").IsFullText();

            
            //WorkList index

            modelBuilder.Entity<WorkList>().HasIndex(worklist => worklist.ID_Patient).HasDatabaseName("ID_Patient");


            #endregion


            #region Configure columns table Patient


            modelBuilder.Entity<Patient>().Property(patient => patient.ID_Patient).HasColumnType("INT UNSIGNED NOT NULL").IsRequired();

            modelBuilder.Entity<Patient>().Property(patient => patient.LastName).HasColumnType("varchar(64)").IsRequired();

            modelBuilder.Entity<Patient>().Property(patient => patient.FirstName).HasColumnType("varchar(64)").IsRequired();

            modelBuilder.Entity<Patient>().Property(patient => patient.MiddleName).HasColumnType("varchar(64)").IsRequired();

            modelBuilder.Entity<Patient>().Property(patient => patient.PatientID).HasColumnType("varchar(64)").IsRequired();

            modelBuilder.Entity<Patient>().Property(patient => patient.BirthDate).HasColumnType("date").IsRequired();

            modelBuilder.Entity<Patient>().Property(patient => patient.Sex).HasColumnType("varchar(1)").IsRequired();

            modelBuilder.Entity<Patient>().Property(patient => patient.Address).HasColumnType("varchar(64)").ValueGeneratedNever();

            modelBuilder.Entity<Patient>().Property(patient => patient.AddInfo).HasColumnType("text").ValueGeneratedNever();

            modelBuilder.Entity<Patient>().Property(patient => patient.Occupation).HasColumnType("varchar(64)").ValueGeneratedNever();


            #endregion


            #region Configure columns table WorkList

            
            modelBuilder.Entity<WorkList>().Property(patient => patient.WorkListID).HasColumnType("INT UNSIGNED NOT NULL").IsRequired();

            modelBuilder.Entity<WorkList>().Property(patient => patient.ID_WorkList).HasColumnType("INT UNSIGNED NOT NULL").IsRequired();

            modelBuilder.Entity<WorkList>().Property(patient => patient.ID_Patient).HasColumnType("INT UNSIGNED NOT NULL").IsRequired();

            modelBuilder.Entity<WorkList>().Property(patient => patient.CreateDate).HasColumnType("DATE").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.CreateTime).HasColumnType("TIME").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.CompleteDate).HasColumnType("DATE").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.CompleteTime).HasColumnType("TIME").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.State).HasColumnType("varchar(16)").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.SOPInstanceUID).HasColumnType("varchar(64)").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.Modality).HasColumnType("varchar(16)").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.StationAeTitle).HasColumnType("varchar(16)").IsRequired();

            modelBuilder.Entity<WorkList>().Property(patient => patient.ProcedureStepStartDateTime).HasColumnType("DATETIME").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.PerformingPhysiciansName).HasColumnType("varchar(324)").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.StudyDescription).HasColumnType("varchar(64)").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.ReferringPhysiciansName).HasColumnType("varchar(324)").ValueGeneratedNever();

            modelBuilder.Entity<WorkList>().Property(patient => patient.RequestingPhysician).HasColumnType("varchar(324)").ValueGeneratedNever();


            #endregion


        }
    }
}
