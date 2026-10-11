using System.Diagnostics;
using EFCore.Console.Domain.Departments;
using Microsoft.EntityFrameworkCore;

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Dominando o EF Core!");
Console.ForegroundColor = ConsoleColor.White;


//EnsureceratedAndDeleted();
//HelthCheckDatabase();
//ManageStateConnection();
SqlInjection();


//warmup
new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
	new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
	.Options).Departments.AsNoTracking().Any();

// int _count = 0;
// ManageStateConnection(false, _count);
// _count = 0;
// ManageStateConnection(true, _count);


static void EnsureceratedAndDeleted()
{
	using var context = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);
	context.Database.EnsureDeleted();

	context.Database.EnsureCreated();
}


static void HelthCheckDatabase()
{
	using var context = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	bool canConnect = context.Database.CanConnect();
	Console.ForegroundColor = canConnect ? ConsoleColor.Green : ConsoleColor.Red;
	Console.WriteLine(canConnect ? "Database is reachable." : "Database is not reachable.");
	Console.ForegroundColor = ConsoleColor.White;
}



static void ManageStateConnection(bool canManageConnection, int _count)
{
	var time = Stopwatch.StartNew();
	using var context = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
	new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
	.Options);

	var connection = context.Database.GetDbConnection();
	connection.StateChange += (sender, e) => ++_count;

	if (canManageConnection)
	{
		connection.Open();
	}

	for (int i = 0; i < 200; i++)
	{
		context.Departments.AsNoTracking().Any();
	}

	time.Stop();
	Console.WriteLine($"Elapsed time: {time.ElapsedMilliseconds} ms, isManageConnection: {canManageConnection}, count: {_count}");
}


static void ExevuteSql()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	//Primeira opcao
	using (var cmd = db.Database.GetDbConnection().CreateCommand())
	{
		cmd.CommandText = "SELECT 1";
		db.Database.OpenConnection();
		cmd.ExecuteNonQuery();
	}

	// Segunda opcao
	var description = "Teste update via sql raw";
	db.Database.ExecuteSqlRaw("UPDATE Departments SET Description = '{description}' WHERE Id = 1", description);

	//Terceira opcao
	db.Database.ExecuteSqlInterpolated($"UPDATE Departments SET Description = '{description}' WHERE Id = 1");
}

static void SqlInjection()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);
	db.Database.EnsureDeleted();
	db.Database.EnsureCreated();

	db.Departments.AddRange(
		 Department.Create("Department 1", true),
		 Department.Create("Department 2", true)
	);

	db.SaveChanges();
	Console.WriteLine("Departments added successfully.");

	var descricao = "Departamento hackeado' or 1='1";
	db.Database.ExecuteSqlRaw($"UPDATE Departments SET Description = 'Teste' WHERE Description = '{descricao}'");

	foreach (var department in db.Departments.AsNoTracking())
	{
		Console.WriteLine($"Department Id: {department.Id}, Description: {department.Description}");
	}
}