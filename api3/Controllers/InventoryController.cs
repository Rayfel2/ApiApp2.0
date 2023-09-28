using api3.Dto;
using api3.Interface;
using api3.Models;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public InventoryController(
            InterfaceInventory RepositoryInventory,
            InterfaceStore RepositoryStore,
            InterfaceEmployee RepositoryEmployee,
            IMapper mapper)
        {
            _RepositoryInventory = RepositoryInventory;
            _RepositoryStore = RepositoryStore;
            _RepositoryEmployee = RepositoryEmployee;
            _mapper = mapper;
        }

        [HttpGet("/inventory")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Inventory>))]
        public async Task<IActionResult> GetInventory(
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

                var allInventory = await _RepositoryInventory.GetInventoryAsync();

                // Obtener todos los empleados en memoria
                var allEmployees = await _RepositoryEmployee.GetEmployeeAsync();

                // Aplicar filtro de nombre en memoria
                if (nameFilter != null && nameFilter.Any())
                {
                    var matchingInventoryIds = allInventory
                        .Where(i => allEmployees.Any(e => nameFilter.Any(filter => e.Name.Contains(filter) && e.IdEmployee == i.IdEmployee)))
                        .Select(i => i.IdInventory)
                        .ToList();

                    allInventory = allInventory
                        .Where(i => matchingInventoryIds.Contains(i.IdInventory))
                        .ToList();
                }


                if (flavorFilter != null && flavorFilter.Any())
                {
                    allInventory = allInventory
                        .Where(i => flavorFilter.Any(filter => i.Flavor.Contains(filter)))
                        .ToList();
                }

                if (isSeasonFlavorFilter != null)
                {
                    allInventory = allInventory
                        .Where(i =>
                            (isSeasonFlavorFilter == true && i.IsSeasonFlavor == "Yes") ||
                            (isSeasonFlavorFilter == false && i.IsSeasonFlavor == "No"))
                        .ToList();
                }

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
                    else  // Primero probamos si es exacta y después si es un rango
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
        public async Task<IActionResult> Get(int InventoryId)
        {
            var Inventory = _mapper.Map<InventoryDto>(await _RepositoryInventory.GetInventoryAsync(InventoryId));
            if (Inventory == null || !ModelState.IsValid)
            {
                ModelState.AddModelError("", "No se pudo encontrar el inventario");
                return StatusCode(404, ModelState);
            }
            return Ok(Inventory);
        }

        [HttpPost("/inventory")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Post([FromBody] InventoryPostDto InventoryDTO)
        {
            InventoryDto PostInventory = new InventoryDto();
            if (InventoryDTO == null || !ModelState.IsValid) { return BadRequest(ModelState); }

            

            int storeId = await _RepositoryStore.GetStoreIdByNameAsync(InventoryDTO.Store);
            if (!await _RepositoryStore.StoreExistAsync(storeId)) { return NotFound("No se encontró la tienda con el IdStore proporcionado"); }


            int employeeId = await _RepositoryEmployee.GetEmployeeIdByNameAsync(InventoryDTO.ListedBy);
            if (!await _RepositoryEmployee.EmployeeExistAsync(employeeId)) { return NotFound("No se encontró el empleado con el IdEmployee proporcionado"); }

            

            var Inventory = _mapper.Map<Inventory>(PostInventory);
            Inventory.IdInventory = await _RepositoryInventory.GetNextInventoryIdAsync();
            Inventory.IdEmployee = employeeId;
            Inventory.IdStore = storeId;
            Inventory.Flavor = InventoryDTO.Flavor;
            Inventory.IsSeasonFlavor = InventoryDTO.IsSeasonFlavor;
            Inventory.Quantity = InventoryDTO.Quantity;
            Inventory.Date = InventoryDTO.Date;

            if (!await _RepositoryInventory.CreateInventoryAsync(Inventory))
            {
                ModelState.AddModelError("", "Algo salió mal al guardar");
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
        public async Task<IActionResult> UpdateInventory(int InventoryId, [FromBody] InventoryUpdateDto updatedInventory)
        {
            if (updatedInventory == null)
                return BadRequest(ModelState);

            if (!await _RepositoryInventory.InventoryExistAsync(InventoryId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            int employeeId = await _RepositoryEmployee.GetEmployeeIdByNameAsync(updatedInventory.ListedBy);

            if (!await _RepositoryEmployee.EmployeeExistAsync(employeeId)) { return NotFound("No se encontró el empleado con el IdEmployee proporcionado"); }

            var filterUpdate = await _RepositoryInventory.GetInventoryAsync(InventoryId);

            var InventoryMap = _mapper.Map<Inventory>(filterUpdate);
            InventoryMap.Quantity = updatedInventory.Quantity;
            InventoryMap.IdEmployee = employeeId;

            if (!await _RepositoryInventory.UpdateInventoryAsync(InventoryId, InventoryMap))
            {
                ModelState.AddModelError("", "Algo salió mal al actualizar");
                return StatusCode(500, ModelState);
            }

            return Ok("Se ha actualizado con éxito");
        }

        [HttpDelete("/inventory/{InventoryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteInventory(int InventoryId)
        {
            if (!await _RepositoryInventory.InventoryExistAsync(InventoryId))
            {
                return NotFound();
            }

            var InventoryToDelete = await _RepositoryInventory.GetInventoryAsync(InventoryId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _RepositoryInventory.DeleteInventoryAsync(InventoryToDelete))
            {
                ModelState.AddModelError("", "Algo salió mal al eliminar");
            }

            return Ok("Se ha eliminado la tabla");
        }

        [HttpPost("/inventory/upload")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UploadInventory([FromForm] IFormFile csvFile)
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
                    Delimiter = ","
                }))
                {
                    var records = csvReader.GetRecords<InventoryCsvDtoFirst>()
                        .Where(record => !string.IsNullOrWhiteSpace(record.Store))
                        .ToList();

                    foreach (var record in records)
                    {
                        StoreDto StoreDTO = new StoreDto(); // Crear una nueva instancia
                        EmployeeDto EmployeeDTO = new EmployeeDto(); // Crear una nueva instancia
                        InventoryDto InventoryDTO = new InventoryDto(); // Crear una nueva instancia

                        // Agregar el store
                        int storeId = await _RepositoryStore.GetStoreIdByNameAsync(record.Store.Trim());

                        if (storeId == -1)
                        {
                            StoreDTO.IdStore = 0;
                            StoreDTO.IdStore = await _RepositoryStore.GetNextStoreIdAsync();

                            if (await _RepositoryStore.StoreExistAsync(StoreDTO.IdStore))
                            {
                                return StatusCode(666, "Store ya existe");
                            }

                            StoreDTO.Name = record.Store.Trim();

                            var Store = _mapper.Map<Store>(StoreDTO);

                            if (!await _RepositoryStore.CreateStoreAsync(Store))
                            {
                                ModelState.AddModelError("", "Something went wrong while saving");
                                return StatusCode(500, ModelState);
                            }
                        }
                        else { StoreDTO.IdStore = storeId; }




                        //Agregar el empleado
                        int EmployeeId = await _RepositoryEmployee.GetEmployeeIdByNameAsync(record.ListedBy.Trim());

                        if (EmployeeId == -1)
                        {
                            EmployeeDTO.IdEmployee = 0;
                            EmployeeDTO.IdEmployee = await _RepositoryEmployee.GetNextEmployeeIdAsync();

                            if (await _RepositoryEmployee.EmployeeExistAsync(EmployeeDTO.IdEmployee))
                            {
                                return StatusCode(666, "Employee ya existe");
                            }

                            EmployeeDTO.Name = record.ListedBy.Trim();

                            var Employee = _mapper.Map<Employee>(EmployeeDTO);

                            if (!await _RepositoryEmployee.CreateEmployeeAsync(Employee))
                            {
                                ModelState.AddModelError("", "Something went wrong while saving");
                                return StatusCode(500, ModelState);
                            }
                        }
                        else { EmployeeDTO.IdEmployee = EmployeeId; }





                        InventoryDTO.IdInventory = await _RepositoryInventory.GetNextInventoryIdAsync();
                        InventoryDTO.IdEmployee = EmployeeDTO.IdEmployee;
                        InventoryDTO.IdStore = StoreDTO.IdStore;
                        //InventoryDTO.Date = Convert.ToDateTime(record.Date);
                        InventoryDTO.Date = DateTime.SpecifyKind(Convert.ToDateTime(record.Date), DateTimeKind.Utc); // Por el tema del timeswamp
                        InventoryDTO.Flavor = record.Flavor.Trim();
                        InventoryDTO.IsSeasonFlavor = record.IsSeasonFlavor.Trim();
                        InventoryDTO.Quantity = Convert.ToInt32(record.Quantity);


                        var Inventory = _mapper.Map<Inventory>(InventoryDTO);


                        if (!await _RepositoryInventory.CreateInventoryAsync(Inventory)) // Cambio aquí
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