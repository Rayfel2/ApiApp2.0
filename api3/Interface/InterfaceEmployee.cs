using api3.Models;

namespace api3.Interface
{
    public interface InterfaceEmployee
    {
        Task<List<Employee>> GetEmployeeAsync(); // GET
        Task<bool> UpdateEmployeeAsync(int employeeID, Employee employee); // PUT
        Task<bool> CreateEmployeeAsync(Employee employee); // POST
        Task<Employee> GetEmployeeAsync(int id);
        Task<bool> DeleteEmployeeAsync(Employee employee);
        Task<int> GetNextEmployeeIdAsync();
        Task<int> GetEmployeeIdByNameAsync(string employeeName);
        Task<List<int>> GetEmployeeIdsByPartialNamesAsync(List<string> partialNames);
        Task<bool> EmployeeExistAsync(int idEmployee);
    }
}
