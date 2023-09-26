using api3.Models;

namespace api3.Interface
{
    public interface InterfaceEmployee
    {
        ICollection<Employee> GetEmployee(); // get
        
        bool UpdateEmployee(int EmployeeID, Employee employee); // put
        bool save(); // guardar
        bool EmployeeExist(int idEmployee); // put y post
        bool CreateEmployee(Employee employee); //post
        Employee GetEmployee(int id);
        bool DeleteEmployee(Employee Employee);
        int GetNextEmployeeId();

        int GetEmployeeIdByName(string EmployeeName);

        List<int> GetEmployeeIdsByPartialNames(List<string> partialNames);
    }
}
