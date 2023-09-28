using api3.Interface;
using api3.Models;
using api3.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : Controller
    {
        private readonly InterfaceEmployee _RepositoryEmployee;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public EmployeeController(InterfaceEmployee RepositoryEmployee, IMapper mapper, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _RepositoryEmployee = RepositoryEmployee;
            _mapper = mapper;
        }

        [HttpGet("/employee")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<EmployeeDto>))]
        public async Task<IActionResult> GetEmployee(int page = 1, int pageSize = 10)
        {
            try
            {
                if (page < 1)
                {
                    page = 1;
                }

                if (pageSize < 1)
                {
                    pageSize = 10;
                }

                int startIndex = (page - 1) * pageSize;
                IEnumerable<Employee> allEmployees;

                if (_memoryCache.TryGetValue("Empleado", out var cachedData))
                {
                    allEmployees = (IEnumerable<Employee>)cachedData;
                    Console.WriteLine("Cargando con cache de employee");
                }
                else
                {
                    allEmployees = await _RepositoryEmployee.GetEmployeeAsync();
                    _memoryCache.Set("Empleado", allEmployees, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
                    });
                    Console.WriteLine("Cargando sin cache de employee");
                }

                var pagedEmployees = allEmployees.Skip(startIndex).Take(pageSize).ToList();
                var employeeDtoList = _mapper.Map<List<EmployeeDto>>(pagedEmployees);
                Response.Headers.Add("Cache-Control", "public, max-age=3600");
                return Ok(employeeDtoList);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error al obtener los empleados: " + ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Post([FromBody] EmployeeDto EmployeeDTO)
        {
            if (EmployeeDTO == null || !ModelState.IsValid) { return BadRequest(ModelState); }

            if (await _RepositoryEmployee.EmployeeExistAsync(EmployeeDTO.IdEmployee))
            {
                return StatusCode(666, "Empleado ya existe");
            }

            var Employee = _mapper.Map<Employee>(EmployeeDTO);

            if (!await _RepositoryEmployee.CreateEmployeeAsync(Employee))
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }

            else
            {
                return Ok("Se ha registrado");
            }
        }

        [HttpPut("{employeeId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateEmployee(int employeeId, [FromBody] EmployeeUpdateDto updatedEmployee)
        {
            if (updatedEmployee == null)
                return BadRequest(ModelState);

            if (!await _RepositoryEmployee.EmployeeExistAsync(employeeId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var EmployeeMap = _mapper.Map<Employee>(updatedEmployee);
            EmployeeMap.IdEmployee = employeeId;
            if (!await _RepositoryEmployee.UpdateEmployeeAsync(employeeId, EmployeeMap))
            {
                ModelState.AddModelError("", "Something went wrong updating owner");
                return StatusCode(500, ModelState);
            }

            return Ok("Se ha actualizado con exito");
        }

        [HttpDelete("{employeeId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteEmployee(int employeeId)
        {
            if (!await _RepositoryEmployee.EmployeeExistAsync(employeeId))

            {
                return NotFound();
            }

            var employeeToDelete = await _RepositoryEmployee.GetEmployeeAsync(employeeId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _RepositoryEmployee.DeleteEmployeeAsync(employeeToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting category");
            }

            return Ok("Se ha eliminado la tabla");
        }
    }
}
