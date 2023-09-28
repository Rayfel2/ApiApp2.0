/*using api3.Interface;
using api3.Models;
using Microsoft.EntityFrameworkCore;

namespace api3.Repository
{
    public class RepositoryInventory : InterfaceInventory
    {
        private readonly PgAdminContext _context;
        public RepositoryInventory(PgAdminContext context)
        {
            _context = context;
        }
        public bool CreateInventory(Inventory inventory)
        {
            _context.Add(inventory);
            return save();
        }

        public ICollection<Inventory> GetInventory()
        {
          return _context.Inventories.OrderBy(H => H.IdInventory).ToList();
     /*     return _context.Inventories
    .Include(i => i.IdEmployeeNavigation) // Carga ansiosa para IdEmployeeNavigation
    .Include(i => i.IdStoreNavigation)    // Carga ansiosa para IdStoreNavigation
    .ToList();*/
/*
        }

        public bool save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;

        }

        public bool InventoryExist(int idInventory)
        {
            return _context.Inventories.Any(p => p.IdInventory == idInventory);

        }

        public bool UpdateInventory(int InventoryID, Inventory inventory)
        {
            _context.Update(inventory);
            return save();

        }
        public bool DeleteInventory(Inventory Inventory)
        {
            _context.Remove(Inventory);
            return save();
        }

        public Inventory GetInventory(int id)
        {
            return _context.Inventories.Where(e => e.IdInventory == id).FirstOrDefault();
        }

        public int GetNextInventoryId()
        {
            var lastInventory = _context.Inventories
                .OrderByDescending(e => e.IdInventory)
                .FirstOrDefault();

            if (lastInventory != null)
            {
                // Se encontró el último registro, devuelve el siguiente ID.
                int id = Convert.ToInt32(lastInventory.IdInventory + 1);
                return id;
            }
            else
            {
                // No se encontraron registros en la tabla, devuelve 1 como el primer ID.
                return 1;
            }
        }

    }
}*/
using api3.Interface;
using api3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api3.Repository
{
    public class RepositoryInventory : InterfaceInventory
    {
        private readonly PgAdminContext _context;

        public RepositoryInventory(PgAdminContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateInventoryAsync(Inventory inventory)
        {
            _context.Add(inventory);
            return await SaveAsync();
        }

        public async Task<List<Inventory>> GetInventoryAsync()
        {
            return await _context.Inventories.OrderBy(H => H.IdInventory).ToListAsync();
            /* 
            También puedes cargar ansiosamente las relaciones de esta manera:
            return await _context.Inventories
                .Include(i => i.IdEmployeeNavigation)
                .Include(i => i.IdStoreNavigation)
                .ToListAsync();
            */
        }

        public async Task<bool> InventoryExistAsync(int idInventory)
        {
            return await _context.Inventories.AnyAsync(p => p.IdInventory == idInventory);
        }

        public async Task<bool> UpdateInventoryAsync(int InventoryID, Inventory inventory)
        {
            _context.Update(inventory);
            return await SaveAsync();
        }

        public async Task<bool> DeleteInventoryAsync(Inventory Inventory)
        {
            _context.Remove(Inventory);
            return await SaveAsync();
        }

        public async Task<Inventory> GetInventoryAsync(int id)
        {
            return await _context.Inventories.FirstOrDefaultAsync(e => e.IdInventory == id);
        }

        public async Task<int> GetNextInventoryIdAsync()
        {
            var lastInventory = await _context.Inventories
                .OrderByDescending(e => e.IdInventory)
                .FirstOrDefaultAsync();

            if (lastInventory != null)
            {
                // Se encontró el último registro, devuelve el siguiente ID.
                int id = Convert.ToInt32(lastInventory.IdInventory + 1);
                return id;
            }
            else
            {
                // No se encontraron registros en la tabla, devuelve 1 como el primer ID.
                return 1;
            }
        }

        private async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
