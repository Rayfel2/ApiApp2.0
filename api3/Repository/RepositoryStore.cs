using api3.Interface;
using api3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api3.Repository
{
    public class RepositoryStore : InterfaceStore
    {
        private readonly PgAdminContext _context;

        public RepositoryStore(PgAdminContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateStoreAsync(Store Store)
        {
            _context.Add(Store);
            return await SaveAsync();
        }

        public async Task<ICollection<Store>> GetStoreAsync()
        {
            return await _context.Stores.OrderBy(H => H.IdStore).ToListAsync();
        }

        public async Task<int> GetStoreIdByNameAsync(string storeName)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Name == storeName);

            if (store != null)
            {
                return store.IdStore;
            }

            return -1;
        }

        public async Task<bool> StoreExistAsync(int IdStore)
        {
            return await _context.Stores.AnyAsync(p => p.IdStore == IdStore);
        }

        public async Task<bool> UpdateStoreAsync(int StoreID, Store Store)
        {
            _context.Update(Store);
            return await SaveAsync();
        }

        public async Task<bool> DeleteStoreAsync(Store Store)
        {
            _context.Remove(Store);
            return await SaveAsync();
        }

        public async Task<Store> GetStoreAsync(int id)
        {
            return await _context.Stores.FirstOrDefaultAsync(e => e.IdStore == id);
        }

        public async Task<int> GetNextStoreIdAsync()
        {
            var lastStore = await _context.Stores.OrderByDescending(e => e.IdStore).FirstOrDefaultAsync();

            if (lastStore != null)
            {
                return Convert.ToInt32(lastStore.IdStore + 1);
            }
            else
            {
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
