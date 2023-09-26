using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace api3.Models;

public partial class PgAdminContext : DbContext
{
    public PgAdminContext()
    {
    }

    public PgAdminContext(DbContextOptions<PgAdminContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=LAPTOP-JBL04GLS\\SQLEXPRESS01;Database=PgAdmin;Integrated Security=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        var dateTimeToTimestampConverter = new ValueConverter<DateTime, DateTime>(
            v => v, // Convertir DateTime a sí mismo
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // Convertir timestamp a DateTime
            );
                modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.IdEmployee).HasName("PK__Employee__D9EE4F362C37AD8C");

            entity.ToTable("Employee");

            entity.Property(e => e.IdEmployee)
                .ValueGeneratedNever()
                .HasColumnName("ID_Employee");
            entity.Property(e => e.Name).HasMaxLength(60);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
           

            entity.HasKey(e => e.IdInventory).HasName("PK__Inventor__2210F49E6563AFD3");

            entity.ToTable("Inventory");

            entity.Property(e => e.IdInventory)
                .ValueGeneratedNever()
                .HasColumnName("ID_Inventory");
            entity.Property(e => e.Date).HasColumnType("timestamp with time zone").HasConversion(dateTimeToTimestampConverter); 
            entity.Property(e => e.Flavor).HasMaxLength(60);
            entity.Property(e => e.IdEmployee).HasColumnName("ID_Employee");
            entity.Property(e => e.IdStore).HasColumnName("ID_Store");
            entity.Property(e => e.IsSeasonFlavor)
                .HasMaxLength(60)
                .HasColumnName("Is_season_flavor");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.IdEmployee)
                .HasConstraintName("FK__Inventory__ID_Em__412EB0B6");

            entity.HasOne(d => d.IdStoreNavigation).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.IdStore)
                .HasConstraintName("FK__Inventory__ID_St__403A8C7D");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.IdStore).HasName("PK__Store__99D83D2C0FA271F8");

            entity.ToTable("Store");

            entity.Property(e => e.IdStore)
                .ValueGeneratedNever()
                .HasColumnName("ID_Store");
            entity.Property(e => e.Name).HasMaxLength(60);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
