using Microsoft.EntityFrameworkCore;
using HCSSystem.Data.Entities;

namespace HCSSystem.Data
{
    public class HcsDbContext : DbContext
    {
        // DbSet для каждой таблицы
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ClientAddress> ClientAddresses { get; set; }
        public DbSet<Resident> Residents { get; set; }
        public DbSet<UnitOfMeasurement> UnitOfMeasurements { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Meter> Meters { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Указываем строку подключения к локальной SQLite базе
            optionsBuilder.UseSqlite("Data Source=hcs_system.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Администратор" },
                new Role { Id = 2, Name = "Сотрудник" },
                new Role { Id = 3, Name = "Клиент" }
            );

            modelBuilder.Entity<PaymentStatus>().HasData(
                new PaymentStatus { Id = 1, Name = "Не оплачено" },
                new PaymentStatus { Id = 2, Name = "Частично оплачено" },
                new PaymentStatus { Id = 3, Name = "Оплачено" }
            );

            modelBuilder.Entity<UnitOfMeasurement>().HasData(
                new UnitOfMeasurement { Id = 1, Name = "м³" },
                new UnitOfMeasurement { Id = 2, Name = "кВт⋅ч" },
                new UnitOfMeasurement { Id = 3, Name = "Гкал" }
            );

            // User - Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // Employee - User
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Employee>(e => e.Id);

            // Client - User
            modelBuilder.Entity<Client>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Client>(c => c.Id);

            // Service - UnitOfMeasurement
            modelBuilder.Entity<Service>()
                .HasOne(s => s.UnitOfMeasurement)
                .WithMany(u => u.Services)
                .HasForeignKey(s => s.UnitOfMeasurementId);

            // Rate - Service
            modelBuilder.Entity<Rate>()
                .HasOne(r => r.Service)
                .WithMany(s => s.Rates)
                .HasForeignKey(r => r.ServiceId);

            // Rate - CreatedByUser
            modelBuilder.Entity<Rate>()
                .HasOne(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ClientAddress - Client
            modelBuilder.Entity<ClientAddress>()
                .HasOne(ca => ca.Client)
                .WithMany()
                .HasForeignKey(ca => ca.ClientId);

            // ClientAddress - Address
            modelBuilder.Entity<ClientAddress>()
                .HasOne(ca => ca.Address)
                .WithMany(a => a.ClientsAddresses)
                .HasForeignKey(ca => ca.AddressId);

            // Resident - Address
            modelBuilder.Entity<Resident>()
                .HasOne(r => r.Address)
                .WithMany(a => a.Residents)
                .HasForeignKey(r => r.AddressId);

            // Meter - Address
            modelBuilder.Entity<Meter>()
                .HasOne(m => m.Address)
                .WithMany() // если нет ICollection<Meter> в Address
                .HasForeignKey(m => m.AddressId)
                .OnDelete(DeleteBehavior.Cascade);

            // Meter - Service
            modelBuilder.Entity<Meter>()
                .HasOne(m => m.Service)
                .WithMany()
                .HasForeignKey(m => m.ServiceId);

            // Client - ClientAddresses
            modelBuilder.Entity<Client>()
                .HasMany(c => c.ClientAddresses)
                .WithOne(ca => ca.Client)
                .HasForeignKey(ca => ca.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // MeterReading - Meter
            modelBuilder.Entity<MeterReading>()
                .HasOne(mr => mr.Meter)
                .WithMany()
                .HasForeignKey(mr => mr.MeterId);

            // MeterReading - ApprovedByUser
            modelBuilder.Entity<MeterReading>()
                .HasOne(mr => mr.ApprovedByUser)
                .WithMany()
                .HasForeignKey(mr => mr.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.MeterReading)
                .WithMany() 
                .HasForeignKey(p => p.MeterReadingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment - PaymentStatus
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentStatus)
                .WithMany(ps => ps.Payments)
                .HasForeignKey(p => p.PaymentStatusId);

            // Payment - ApprovedByUser
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.ApprovedByUser)
                .WithMany()
                .HasForeignKey(p => p.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
