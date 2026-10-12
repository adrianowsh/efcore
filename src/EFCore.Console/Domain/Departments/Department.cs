using System;
using EFCore.Console.Domain.Employees;

namespace EFCore.Console.Domain.Departments;

public class Department
{
	private Department()
	{

	}
	private Department(string id, string description, bool active)
	{
		Id = id;
		Description = description;
		Active = active;
	}
	public string Id { get; private set; }
	public string Description { get; private set; }
	public bool Active { get; private set; }
	public bool Deleted { get; private set; }
	public virtual List<Employee> Employees { get; private set; } = [];

	public string SetNewId() => $"dep_{Guid.CreateVersion7()}";
	public static Department Create(string description, bool active)
	{
		return new Department($"dep_{Guid.CreateVersion7()}", description, active);
	}

	public void MarkAsDeleted() => Deleted = true;
}
