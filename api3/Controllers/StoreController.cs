using api3.Interface;
using api3.Models;
using api3.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using api3.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;


namespace api3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoreController : Controller
    {
        private readonly InterfaceStore _RepositoryStore;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public StoreController(InterfaceStore RepositoryStore,  IMapper mapper, IMemoryCache memoryCache)
        {
            _RepositoryStore = RepositoryStore;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }
        [HttpGet("/store")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Store>))]
        public IActionResult GetStore(int page = 1, int pageSize = 10)
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

                
               // var allStores = _RepositoryStore.GetStore();

                IEnumerable<Store> allStores;


                if (_memoryCache.TryGetValue("Store", out var cachedData))
                {
                    // Los datos est�n en cach�, puedes usar cachedData
                    allStores = (IEnumerable<Store>)cachedData;

                    // Almacena los datos en cach� para futuras consultas.
                    Console.WriteLine("Cargando con cache de store");
                }
                else
                {
                    // Obtengo todos los repositorios
                    allStores = _RepositoryStore.GetStore();
                    _memoryCache.Set("Store", allStores, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
                    });
                    Console.WriteLine("Cargando sin cache de store");
                }

                // A nivel de rutas ser�a por ejemplo http://localhost:5204/store?page=1&pageSize=10
                var pagedStores = allStores.Skip(startIndex).Take(pageSize).ToList();

                var storeDtoList = _mapper.Map<List<StoreDto>>(pagedStores);
                Response.Headers.Add("Cache-Control", "public, max-age=3600"); // Esto establece una cach� p�blica de 1 hora
                return Ok(storeDtoList);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurri� un error al obtener las tiendas: " + ex.Message);
                return BadRequest(ModelState);
            }
        }





        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult Post([FromBody] StoreDto StoreDTO)
        {
            // por si el DTO es null
            if (StoreDTO == null || !ModelState.IsValid) { return BadRequest(ModelState); }
            if (_RepositoryStore.StoreExist(StoreDTO.IdStore))
            {
                return StatusCode(666, "Store ya existe");
            }

            var Store = _mapper.Map<Store>(StoreDTO);

            if (!_RepositoryStore.CreateStore(Store))
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }

            else
            {
                return Ok("Se ha registrado");
            }

        }


        [HttpPut("{StoreId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateStore(int StoreId, [FromBody] StoreUpdateDto updatedStore)
        {
            if (updatedStore == null)
                return BadRequest(ModelState);

            if (!_RepositoryStore.StoreExist(StoreId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var StoreMap = _mapper.Map<Store>(updatedStore);
            StoreMap.IdStore = StoreId;
            if (!_RepositoryStore.UpdateStore(StoreId, StoreMap))
            {
                ModelState.AddModelError("", "Something went wrong updating owner");
                return StatusCode(500, ModelState);
            } 

            return Ok("Se ha actualizado con exito");
        }
        
        [HttpDelete("{StoreId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteStore(int StoreId)
        {
            if (!_RepositoryStore.StoreExist(StoreId))
            {
                return NotFound();
            }

            var StoreToDelete = _RepositoryStore.GetStore(StoreId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_RepositoryStore.DeleteStore(StoreToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting category");
            }

            return Ok("Se ha eliminado la tabla");
        }
        
    }
}