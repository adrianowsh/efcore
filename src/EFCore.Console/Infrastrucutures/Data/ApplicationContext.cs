using System;
using Microsoft.EntityFrameworkCore;
using EFCore.Console.Domain;
using EFCore.Console.Domain.Departments;
using EFCore.Console.Domain.Employees;
using Microsoft.Extensions.Logging;

namespace EFCore.Console.Infrastrucutures.Data;

public sealed class ApplicationContext : DbContext
{
	public DbSet<Employee> Employees { get; set; }
	public DbSet<Department> Departments { get; set; }
	public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
	{
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		string connectionString = "Data Source=localhost;Initial Catalog=EfCore;User Id=sa;Password=Yk2jdii32!@#;TrustServerCertificate=True;pooling=True;";
		optionsBuilder
		//.EnableSensitiveDataLogging()
		//.LogTo(System.Console.WriteLine, LogLevel.Information)
		.UseSqlServer(connectionString);
	}
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}
}
