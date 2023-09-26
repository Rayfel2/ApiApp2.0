using api3.Interface;
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
}
