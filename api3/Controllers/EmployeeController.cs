using api3.Interface;
using api3.Models;
using api3.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;


namespace api3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : Controller
    {
        private readonly InterfaceEmployee _RepositoryEmployee;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public EmployeeController(InterfaceEmployee RepositoryEmployee,  IMapper mapper, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _RepositoryEmployee = RepositoryEmployee;
            _mapper = mapper;
        }
        /*
        [HttpGet("/employee")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Employee>))]
        public IActionResult GetEmployee()
        {
            var Employee = _mapper.Map<List<EmployeeDto>>(_RepositoryEmployee.GetEmployee());
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(Employee);
        }
        */

        [HttpGet("/employee")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Employee>))]
        public IActionResult GetEmployee(int page = 1, int pageSize = 10)
        {
            try
            {
                // Evitando valores negativos
                if (page < 1)
                {
                    page = 1; // Página mínima
                }

                if (pageSize < 1)
                {
                    pageSize = 10; // Tamaño de página predeterminado
                }

                // Utilizado para determinar donde comienza cada pagina
                int startIndex = (page - 1) * pageSize;
                //1 - 1 * 10 = comienza en 0
                //2 - 1 * 10 = comienza en 10
                IEnumerable<Employee> allEmployees;
             
                
                if (_memoryCache.TryGetValue("Empleado", out var cachedData))
                {
                    // Los datos están en caché, puedes usar cachedData
                    allEmployees = (IEnumerable<Employee>)cachedData;

                    // Almacena los datos en caché para futuras consultas.
                    Console.WriteLine("Cargando con cache de employee");
                }
                else
                {
                    // Obtengo todos los repositorios
                    allEmployees = _RepositoryEmployee.GetEmployee();
                    _memoryCache.Set("Empleado", allEmployees, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
                    });
                    Console.WriteLine("Cargando sin cache de employee");
                }



                // Aplicamos paginación utilizando LINQ para seleccionar los registros apropiados.
                // A nivel de rutas seria por ejemplo http://localhost:5204/employee?page=1&pageSize=10
                var pagedEmployees = allEmployees.Skip(startIndex).Take(pageSize).ToList();
                //.skip omite un numero de registro
                //.Take cantidad elemento que se van a tomar


                // Mapeo los empleados paginados en vez de todos
                var employeeDtoList = _mapper.Map<List<EmployeeDto>>(pagedEmployees);
                Response.Headers.Add("Cache-Control", "public, max-age=3600"); // Esto establece una caché pública de 1 hora
                    
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
        public IActionResult Post([FromBody] EmployeeDto EmployeeDTO)
        {
            // por si el DTO es null
            if (EmployeeDTO == null || !ModelState.IsValid) { return BadRequest(ModelState); }

            if (_RepositoryEmployee.EmployeeExist(EmployeeDTO.IdEmployee))
            {
                return StatusCode(666, "Empleado ya existe");
            }

            var Employee = _mapper.Map<Employee>(EmployeeDTO);

            if (!_RepositoryEmployee.CreateEmployee(Employee))
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
        public IActionResult UpdateEmployee(int employeeId, [FromBody] EmployeeUpdateDto updatedEmployee)
        {
            if (updatedEmployee == null)
                return BadRequest(ModelState);
            
            if (!_RepositoryEmployee.EmployeeExist(employeeId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var EmployeeMap = _mapper.Map<Employee>(updatedEmployee);
            EmployeeMap.IdEmployee = employeeId;
            if (!_RepositoryEmployee.UpdateEmployee(employeeId, EmployeeMap))
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
        public IActionResult DeleteEmployee(int employeeId)
        {
            if (!_RepositoryEmployee.EmployeeExist(employeeId))
            {
                return NotFound();
            }

            var employeeToDelete = _RepositoryEmployee.GetEmployee(employeeId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_RepositoryEmployee.DeleteEmployee(employeeToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting category");
            }

            return Ok("Se ha eliminado la tabla");
        }
        
    }
}