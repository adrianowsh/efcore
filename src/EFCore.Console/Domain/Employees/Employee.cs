using System;
using EFCore.Console.Domain.Departments;

namespace EFCore.Console.Domain.Employees;

public class Employee
{
	private Employee()
	{

	}
	private Employee(string id, string name, string documentNumber, string departmentId)
	{
		Id = id;
		Name = name;
		DocumentNumber = documentNumber;
		DepartmentId = departmentId;
	}

	public string Id { get; private set; }
	public string Name { get; private set; }
	public string DocumentNumber { get; private set; }
	public string DepartmentId { get; private set; }
	public virtual Department Department { get; private set; }
	public string SetNewId() => $"emp_{Guid.CreateVersion7()}";
	public static Employee Create(string name, string documentNumber, string departmentId)
	{
		return new Employee($"emp_{Guid.CreateVersion7()}", name, documentNumber, departmentId);
	}
}
