using System.Diagnostics;
using EFCore.Console.Domain.Departments;
using EFCore.Console.Domain.Employees;
using Microsoft.EntityFrameworkCore;

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Dominando o EF Core!");
Console.ForegroundColor = ConsoleColor.White;

//EnsureceratedAndDeleted();
//HelthCheckDatabase();
//ManageStateConnection();
//SqlInjection();
//PendingMigrations();
//EagerLoading();
//ExplicitLoading();
//LazyLoading();
//GlobalFilterUsingEfCore();
//ConsultaProjetada();
//QueryParameter();
//QueryIInterpolated();
//QueryWithTAG();
QueryOneToManyManyToOne();


//warmup
// new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
// 	new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
// 	.Options).Departments.AsNoTracking().Any();

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

	foreach (var department in db.Departments.AsNoTracking())
	{
		Console.WriteLine($"Department Id: {department.Id}, Description: {department.Description}");
	}
}

static void PendingMigrations()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	var pendingMigrations = db.Database.GetPendingMigrations();

	Console.WriteLine($"Total pending migrations: {pendingMigrations.Count()}");

	if (pendingMigrations.Any())
	{
		Console.WriteLine("Pending migrations:");
		foreach (var migration in pendingMigrations)
		{
			Console.WriteLine(migration);
		}
	}
	else
	{
		Console.WriteLine("No pending migrations.");
	}

}

static void EagerLoading()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	SetupDatabase();

	var departments = db
		.Departments
		.AsNoTracking()
		.ToList();

	foreach (var department in departments)
	{

		if (department.Description == "Department 1")
		{
			db.Entry(department).Collection(d => d.Employees).Load();
		}

		if (department.Employees?.Any() ?? false)
		{
			foreach (var employee in department.Employees)
			{
				Console.WriteLine($"\tEmployee Id: {employee.Id}, Name: {employee.Name}");
			}
		}
	}
}

static void ExplicitLoading()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	SetupDatabase();

	var departments = db.Departments.ToList();

	foreach (var department in departments)
	{
		Console.WriteLine($"Department Id: {department.Id}, Description: {department.Description}");

		if (department.Description == "Department 1")
		{
			//db.Entry(department).Collection(d => d.Employees).Load();
			db.Entry(department).Collection(d => d.Employees).Query().Where(e => e.Name == "Employee 1").ToList();
		}

		if (department.Employees?.Any() ?? false)
		{
			foreach (var employee in department.Employees)
			{
				Console.WriteLine($"\tEmployee Id: {employee.Id}, Name: {employee.Name}");
			}
		}
	}
}

static void LazyLoading()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	SetupDatabase();

	//db.ChangeTracker.LazyLoadingEnabled = false;

	var departments = db.Departments.ToList();

	foreach (var department in departments)
	{
		Console.WriteLine($"Department Id: {department.Id}, Description: {department.Description}");

		if (department.Employees?.Any() ?? false)
		{
			foreach (var employee in department.Employees)
			{
				Console.WriteLine($"\tEmployee Id: {employee.Id}, Name: {employee.Name}");
			}
		}
	}
}


static void GlobalFilterUsingEfCore()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	SetupDatabase(db);

	var departments = db
			.Departments
			.AsNoTracking()
			//.IgnoreQueryFilters()
			.ToList();

	foreach (var department in departments)
	{
		Console.WriteLine($"Department Id: {department.Id}, Description: {department.Description}, Deleted: {department.Deleted}");
	}
}

static void ConsultaProjetada()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	SetupDatabase(db);

	var departments = db
			.Departments
			.AsNoTracking()
			.Select(d => new EFCore.Console.Applications.Departments.GetDepartments.DepatmentsDto(
				d.Description,
				d.Employees.Select(e => e.Name).ToArray()
			))
			.ToList();

	foreach (var department in departments)
	{
		Console.WriteLine(@$"Department Description: {department.Description}");
		foreach (var employeeName in department.EmploueeName)
		{
			Console.WriteLine($"\tEmployee Name: {employeeName}");
		}
	}
}

static void QueryParameter()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	SetupDatabase(db);

	bool deleted = false;

	var departments = db
			.Departments
			.FromSqlRaw("SELECT * FROM Departments where Deleted = {0}", deleted)
			.ToList();

	foreach (var department in departments)
	{
		Console.WriteLine(@$"Department Description: {department.Description}");
	}
}

static void QueryIInterpolated()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	SetupDatabase(db);

	bool deleted = false;
	var departments = db
			.Departments
			.FromSqlInterpolated($"SELECT * FROM Departments where Deleted = {deleted}")
			.ToList();

	foreach (var department in departments)
	{
		Console.WriteLine(@$"Department Description: {department.Description}");
	}
}

static void QueryWithTAG()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	SetupDatabase(db);

	var departments = db
			.Departments
			.AsNoTracking()
			.TagWith("Querying departments with AsNoTracking....")
			.ToList();

	foreach (var department in departments)
	{
		Console.WriteLine(@$"Department Description: {department.Description}");
	}
}

static void QueryOneToManyManyToOne()
{
	using var db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		.Options);

	SetupDatabase(db);

	//LEFT JOIN example using Include for one-to-many relationship
	// var departments = db
	// 		.Departments
	// 		.Include(d => d.Employees)
	// 		.AsNoTracking()
	// 		.ToList();

	// INNER JOIN example using Include for many-to-one relationship
	var employees = db
			.Employees
			.Include(e => e.Department)
			.AsNoTracking()
			.ToList();

	foreach (var employee in employees)
	{
		Console.WriteLine(@$"Employee Name: {employee.Name}");
		Console.WriteLine(@$"Department Description: {employee.Department.Description}");
	}
}

static void SetupDatabase(EFCore.Console.Infrastrucutures.Data.ApplicationContext? db = default)
{
	if (db is null)
		db = new EFCore.Console.Infrastrucutures.Data.ApplicationContext(
		 new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EFCore.Console.Infrastrucutures.Data.ApplicationContext>()
		 .Options);

	db.Database.EnsureDeleted();
	db.Database.EnsureCreated();

	if (!db.Departments.Any())
	{
		db.Departments.AddRange(
			 Department.Create("Department 1", true),
			 Department.Create("Department 2", true)
		);

		db.SaveChanges();

		foreach (var department in db.Departments)
		{
			if (department.Description == "Department 1")
			{
				db.Employees.Add(Employee.Create("Employee 1", "11122233387", department.Id));
				db.Employees.Add(Employee.Create("Employee 2", "11122233388", department.Id));
				department.MarkAsDeleted();
				continue;
			}
			db.Employees.Add(Employee.Create("Employee 3", "11122233387", department.Id));
			db.Employees.Add(Employee.Create("Employee 4", "11122233388", department.Id));
			db.Employees.Add(Employee.Create("Employee 5", "11122233389", department.Id));
		}

		db.SaveChanges();
		db.ChangeTracker.Clear();

		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine("Departments added successfully.");
		Console.ForegroundColor = ConsoleColor.White;
	}
}