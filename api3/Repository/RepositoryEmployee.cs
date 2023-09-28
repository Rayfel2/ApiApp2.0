using api3.Interface;
using api3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api3.Repository
{
    public class RepositoryEmployee : InterfaceEmployee
    {
        private readonly PgAdminContext _context;

        public RepositoryEmployee(PgAdminContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateEmployeeAsync(Employee employee)
        {
            _context.Add(employee);
            return await SaveAsync();
        }

        public async Task<List<Employee>> GetEmployeeAsync()
        {
            return await _context.Employees.OrderBy(e => e.IdEmployee).ToListAsync();
        }

        public async Task<bool> EmployeeExistAsync(int idEmployee)
        {
            return await _context.Employees.AnyAsync(p => p.IdEmployee == idEmployee);
        }

        public async Task<bool> UpdateEmployeeAsync(int employeeID, Employee employee)
        {
            _context.Update(employee);
            return await SaveAsync();
        }

        public async Task<bool> DeleteEmployeeAsync(Employee employee)
        {
            _context.Remove(employee);
            return await SaveAsync();
        }

        public async Task<Employee> GetEmployeeAsync(int id)
        {
            return await _context.Employees.Where(e => e.IdEmployee == id).FirstOrDefaultAsync();
        }

        public async Task<int> GetNextEmployeeIdAsync()
        {
            var lastEmployee = await _context.Employees.OrderByDescending(e => e.IdEmployee).FirstOrDefaultAsync();

            if (lastEmployee != null)
            {
                return Convert.ToInt32(lastEmployee.IdEmployee + 1);
            }
            else
            {
                return 1;
            }
        }

        public async Task<int> GetEmployeeIdByNameAsync(string employeeName)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(s => s.Name == employeeName);

            if (employee != null)
            {
                return employee.IdEmployee;
            }

            return -1;
        }

        public async Task<List<int>> GetEmployeeIdsByPartialNamesAsync(List<string> partialNames)
        {
            var employeeIds = await _context.Employees
                .Where(employee => partialNames.Any(partialName => employee.Name.Contains(partialName)))
                .Select(employee => employee.IdEmployee)
                .ToListAsync();

            return employeeIds;
        }

        private async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
