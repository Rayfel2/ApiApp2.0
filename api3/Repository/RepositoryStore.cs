using api3.Interface;
using api3.Models;

namespace api3.Repository
{
    public class RepositoryStore : InterfaceStore
    {
        private readonly PgAdminContext _context;
        public RepositoryStore(PgAdminContext context)
        {
            _context = context;
        }
        public bool CreateStore(Store Store)
        {
            _context.Add(Store);
            return save();
        }

        public ICollection<Store> GetStore()
        {
            return _context.Stores.OrderBy(H => H.IdStore).ToList();

        }

        public bool save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;

        }
        public int GetStoreIdByName(string storeName)
        {
            var store = _context.Stores.FirstOrDefault(s => s.Name == storeName);
            if (store != null)
            {
                return store.IdStore;
            }
            // Si no se encuentra la tienda, puedes manejarlo de la manera que desees, por ejemplo, devolver -1.
            return -1;
        }

        public bool StoreExist(int IdStore)
        {
            return _context.Stores.Any(p => p.IdStore == IdStore);

        }

        public bool UpdateStore(int StoreID, Store Store)
        {
            _context.Update(Store);
            return save();

        }
        public bool DeleteStore(Store Store)
        {
            _context.Remove(Store);
            return save();
        }

        public Store GetStore(int id)
        {
            return _context.Stores.Where(e => e.IdStore == id).FirstOrDefault();
        }
        public int GetNextStoreId()
        {
            var lastStore = _context.Stores
                .OrderByDescending(e => e.IdStore)
                .FirstOrDefault();

            if (lastStore != null)
            {
                // Se encontró el último registro, devuelve el siguiente ID.
                int id = Convert.ToInt32(lastStore.IdStore + 1);
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
