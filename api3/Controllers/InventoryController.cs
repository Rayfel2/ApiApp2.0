using api3.Dto;
using api3.Interface;
using api3.Models;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;


namespace api3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : Controller
    {
        private readonly InterfaceInventory _RepositoryInventory;
        private readonly InterfaceStore _RepositoryStore;
        private readonly InterfaceEmployee _RepositoryEmployee;
        private readonly IMapper _mapper;

        public InventoryController(InterfaceInventory RepositoryInventory, InterfaceStore RepositoryStore, InterfaceEmployee RepositoryEmployee, IMapper mapper)
        {
            _RepositoryInventory = RepositoryInventory;
            _RepositoryStore = RepositoryStore;
            _RepositoryEmployee = RepositoryEmployee;
            _mapper = mapper;
        }
        [HttpGet("/inventory")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Inventory>))]
        public IActionResult GetInventory(
           int page = 1, 
           int pageSize = 10,
           [FromQuery] List<string> nameFilter = null,
           [FromQuery] List<string> flavorFilter = null,
           [FromQuery] bool? isSeasonFlavorFilter = null,
           [FromQuery] string? quantityFilter = null,
           [FromQuery] string? dateFilter = null)
            
        {
            try
            {
                
                if (page < 1) page = 1; 
                
                if (pageSize < 1) pageSize = 10; 


                int startIndex = (page - 1) * pageSize;

                var allInventory = _RepositoryInventory.GetInventory();

                // Aplica filtros según los valores proporcionados
                if (nameFilter != null && nameFilter.Any())
                {
                    /* No exacto
                    // Filtre el nombre, y me trae el ID
                    var employeeIds = nameFilter
                        .Select(name => _RepositoryEmployee.GetEmployeeIdByName(name))
                        .Where(id => id != -1) // Filtrar empleados válidos
                        .ToList();
                    allInventory = allInventory
                        .Where(i => employeeIds.Contains(Convert.ToInt32(i.IdEmployee)))
                        .ToList();
                    */

                    var employeeIds = _RepositoryEmployee.GetEmployeeIdsByPartialNames(nameFilter);

                    allInventory = allInventory
                        .Where(i => employeeIds.Contains(Convert.ToInt32(i.IdEmployee)))
                        .ToList();
                }

                if (flavorFilter != null && flavorFilter.Any())
                {
                    allInventory = allInventory
                        .Where(i => flavorFilter.Any(filter => i.Flavor.Contains(filter)))
                        .ToList();
                    // allInventory = allInventory.Where(i => flavorFilter.Contains(i.Flavor)).ToList();
                  
                }

if (isSeasonFlavorFilter != null)
{

 allInventory = allInventory.Where(i =>
 (isSeasonFlavorFilter == true && i.IsSeasonFlavor == "Yes") ||
 (isSeasonFlavorFilter == false && i.IsSeasonFlavor == "No")).ToList();

}
                /* Es la forma mas comoda que encontre a nivel de ruta
                  /inventory?quantityFilter=>:20 (mayor que 20)
                /inventory?quantityFilter=<:30 (menor que 30)
                /inventory?quantityFilter=>=:10 (mayor o igual que 10)
                /inventory?quantityFilter=<=:25 (menor o igual que 25)
                /inventory?quantityFilter==:15 (igual a 15)
                 */
                
                if (!string.IsNullOrEmpty(quantityFilter))
                {
                    var regex = new Regex(@"^(>|<|>=|<=|=):(\d+)$");
                    var match = regex.Match(quantityFilter);

                    if (match.Success) 
                    {
                        var operaciones = match.Groups[1].Value;
                        var value = int.Parse(match.Groups[2].Value);

                        switch (operaciones)
                            {
                            case ">": // Mayor que
                                allInventory = allInventory.Where(i => i.Quantity > value).ToList();
                                break;
                            case "<": // Menor que
                                allInventory = allInventory.Where(i => i.Quantity < value).ToList();
                                break;
                            case ">=": // Mayor o igual que
                                allInventory = allInventory.Where(i => i.Quantity >= value).ToList();
                                break;
                            case "<=": // Menor o igual que
                                allInventory = allInventory.Where(i => i.Quantity <= value).ToList();
                                break;
                            case "=": // Igual a
                                allInventory = allInventory.Where(i => i.Quantity == value).ToList();
                                break;
                            }
                        }
                    }

                if (!string.IsNullOrEmpty(dateFilter))
                {
                    if (DateTime.TryParse(dateFilter, out DateTime parsedDate))
                    {
                       
                        allInventory = allInventory
                            .Where(i => i.Date == parsedDate.Date) // Filtro para fecha exacta
                            .ToList();
                    }
                    else  // Primero probamos si es exacta y despues si es un rango
                    {
                       
                        // El formato es "yyyy-MM-dd|yyyy-MM-dd" para un rango
                        var dateParts = dateFilter.Split('|');
                        if (dateParts.Length == 2 && DateTime.TryParse(dateParts[0], out DateTime startDate) && DateTime.TryParse(dateParts[1], out DateTime endDate))
                        {
                            allInventory = allInventory
                                .Where(i => i.Date >= startDate.Date && i.Date <= endDate.Date) // Filtro para rango
                                .ToList();
                        }
                    }
                }

                

                // A nivel de rutas sería por ejemplo http://localhost:5204/inventory?page=1&pageSize=10
                var pagedInventory = allInventory.Skip(startIndex).Take(pageSize).ToList();


var inventoryDtoList = _mapper.Map<List<InventoryDto>>(pagedInventory);

return Ok(inventoryDtoList);
}
catch (Exception ex)
{
ModelState.AddModelError("", "Ocurrió un error al obtener los inventarios: " + ex.Message);
return BadRequest(ModelState);
}
}


[HttpGet("/inventory/{InventoryId}")]
[ProducesResponseType(200, Type = typeof(InventoryDto))]

public IActionResult Get(int InventoryId)
{
var Inventory = _mapper.Map<InventoryDto>(_RepositoryInventory.GetInventory(InventoryId));
if (Inventory == null || !ModelState.IsValid)
{
ModelState.AddModelError("", "No se puedo encontro la habitación");
return StatusCode(404, ModelState);
}
return Ok(Inventory);
}





[HttpPost("/inventory")]
[ProducesResponseType(200)]
[ProducesResponseType(400)]
public IActionResult Post([FromBody] InventoryDto InventoryDTO)
{
// por si el DTO es null
if (InventoryDTO == null || !ModelState.IsValid) { return BadRequest(ModelState); }

// por si las tablas de las llaves forraneas no existen
if (!_RepositoryStore.StoreExist(InventoryDTO.IdStore)) { return NotFound("No se encontro la tabla del IdStore que proporcionaste"); }

if (!_RepositoryEmployee.EmployeeExist(InventoryDTO.IdEmployee)) { return NotFound("No se encontro la tabla del IdEmployee que proporcionaste"); }

var Inventory = _mapper.Map<Inventory>(InventoryDTO);


if (!_RepositoryInventory.CreateInventory(Inventory))
{
ModelState.AddModelError("", "Something went wrong while saving");
return StatusCode(500, ModelState);
}

else
{
return Ok("Se ha registrado");
}

}


[HttpPut("/inventory/{InventoryId}")]
[ProducesResponseType(400)]
[ProducesResponseType(204)]
[ProducesResponseType(404)]
public IActionResult UpdateInventory(int InventoryId, [FromBody] InventoryUpdateDto updatedInventory)
{
if (updatedInventory == null)
return BadRequest(ModelState);
if (!_RepositoryInventory.InventoryExist(InventoryId))
return NotFound();

if (!ModelState.IsValid)
return BadRequest();

int employeeId = _RepositoryEmployee.GetEmployeeIdByName(updatedInventory.ListedBy);
//if (!_RepositoryStore.StoreExist(storeId)) { return NotFound("No se encontro la tabla del IdStore que proporcionaste"); }

if (!_RepositoryEmployee.EmployeeExist(employeeId)) { return NotFound("No se encontro la tabla del IdEmployee que proporcionaste"); }
var filterUpdate = _RepositoryInventory.GetInventory(InventoryId); // Obtener los datos para que solo actualice lo que quiero

var InventoryMap = _mapper.Map<Inventory>(filterUpdate);
InventoryMap.Quantity = updatedInventory.Quantity; // Actualizar quantity
InventoryMap.IdEmployee = employeeId; // Actualizar id de store

if (!_RepositoryInventory.UpdateInventory(InventoryId, InventoryMap))
{
ModelState.AddModelError("", "Something went wrong updating owner");
return StatusCode(500, ModelState);
}

return Ok("Se ha actualizado con exito");
}

[HttpDelete("/inventory/{InventoryId}")]
[ProducesResponseType(400)]
[ProducesResponseType(204)]
[ProducesResponseType(404)]
public IActionResult DeleteInventory(int InventoryId)
{
if (!_RepositoryInventory.InventoryExist(InventoryId))
{
return NotFound();
}

var InventoryToDelete = _RepositoryInventory.GetInventory(InventoryId);

if (!ModelState.IsValid)
return BadRequest(ModelState);

if (!_RepositoryInventory.DeleteInventory(InventoryToDelete))
{
ModelState.AddModelError("", "Something went wrong deleting category");
}

return Ok("Se ha eliminado la tabla");
}



[HttpPost("/inventory/upload")]
[ProducesResponseType(201)]
[ProducesResponseType(400)]
public IActionResult UploadInventory([FromForm] IFormFile csvFile)
{
if (csvFile == null || csvFile.Length == 0)
{
ModelState.AddModelError("", "No se proporcionó ningún archivo CSV.");
return BadRequest(ModelState);
}

try
{
using (var streamReader = new StreamReader(csvFile.OpenReadStream()))

using (var csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)
{
 // Configurar el separador de campo
 Delimiter = ","
}))

{   

     var records = csvReader.GetRecords<InventoryCsvDtoFirst>()
         .Where(record => !string.IsNullOrWhiteSpace(record.Store)) // Eliminar espacios en blanco
         .ToList(); // lista de registros


 foreach (var record in records)
 {
     StoreDto StoreDTO = new StoreDto(); // Crear una nueva instancia
     EmployeeDto EmployeeDTO = new EmployeeDto(); // Crear una nueva instancia
     InventoryDto InventoryDTO = new InventoryDto(); // Crear una nueva instancia

     // Agregar el store
     int storeId = _RepositoryStore.GetStoreIdByName(record.Store.Trim());

     if (storeId == -1) {
         StoreDTO.IdStore = 0;
         StoreDTO.IdStore = _RepositoryStore.GetNextStoreId();

         if (_RepositoryStore.StoreExist(StoreDTO.IdStore))
         {
             return StatusCode(666, "Store ya existe");
         }

         StoreDTO.Name = record.Store.Trim();

         var Store = _mapper.Map<Store>(StoreDTO);

         if (!_RepositoryStore.CreateStore(Store))
         {
             ModelState.AddModelError("", "Something went wrong while saving");
             return StatusCode(500, ModelState);
         }
     } else { StoreDTO.IdStore = storeId; }




     //Agregar el empleado
     int EmployeeId = _RepositoryEmployee.GetEmployeeIdByName(record.ListedBy.Trim());

     if (EmployeeId == -1)
     {
         EmployeeDTO.IdEmployee = 0;
         EmployeeDTO.IdEmployee = _RepositoryEmployee.GetNextEmployeeId();

         if (_RepositoryEmployee.EmployeeExist(EmployeeDTO.IdEmployee))
         {
             return StatusCode(666, "Employee ya existe");
         }

         EmployeeDTO.Name = record.ListedBy.Trim();

         var Employee = _mapper.Map<Employee>(EmployeeDTO);

         if (!_RepositoryEmployee.CreateEmployee(Employee))
         {
             ModelState.AddModelError("", "Something went wrong while saving");
             return StatusCode(500, ModelState);
         }
     }
     else { EmployeeDTO.IdEmployee = EmployeeId; }





     InventoryDTO.IdInventory = _RepositoryInventory.GetNextInventoryId();
     InventoryDTO.IdEmployee = EmployeeDTO.IdEmployee;
     InventoryDTO.IdStore = StoreDTO.IdStore;
     //InventoryDTO.Date = Convert.ToDateTime(record.Date);
     InventoryDTO.Date = DateTime.SpecifyKind(Convert.ToDateTime(record.Date), DateTimeKind.Utc); // Por el tema del timeswamp
     InventoryDTO.Flavor = record.Flavor.Trim();
     InventoryDTO.IsSeasonFlavor = record.IsSeasonFlavor.Trim();
     InventoryDTO.Quantity = Convert.ToInt32(record.Quantity);


     var Inventory = _mapper.Map<Inventory>(InventoryDTO);


     if (!_RepositoryInventory.CreateInventory(Inventory))
     {
         ModelState.AddModelError("", "Something went wrong while saving");
         return StatusCode(500, ModelState);
     }
 }
}

return StatusCode(201); // Carga exitosa
}
catch (Exception ex)
{
ModelState.AddModelError("", "Ocurrió un error al procesar el archivo CSV: " + ex.Message);

if (ex.InnerException != null)
{
 ModelState.AddModelError("InnerException", ex.InnerException.Message);
}

return BadRequest(ModelState);
}
}
}
}