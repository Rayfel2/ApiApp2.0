using api3.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api3.Interface
{
    public interface InterfaceStore
    {
        Task<ICollection<Store>> GetStoreAsync(); // GET
        Task<bool> UpdateStoreAsync(int StoreID, Store Store); // PUT
       // Task<bool> SaveAsync(); // Guardar
        Task<bool> StoreExistAsync(int IdStore); // PUT y POST
        Task<bool> CreateStoreAsync(Store Store); // POST
        Task<Store> GetStoreAsync(int id);
        Task<bool> DeleteStoreAsync(Store Store);
        Task<int> GetNextStoreIdAsync();

        Task<int> GetStoreIdByNameAsync(string storeName);
    }
}
