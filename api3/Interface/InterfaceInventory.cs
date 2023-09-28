using api3.Models;

namespace api3.Interface
{
    public interface InterfaceInventory
    {
        /*
        ICollection<Inventory> GetInventory(); // get
     
        bool UpdateInventory(int InventoryID, Inventory inventory); // put
        bool save(); // guardar
        bool InventoryExist(int idInventory); // put y post

        bool CreateInventory(Inventory inventory); //post

        Inventory GetInventory(int id);
        bool DeleteInventory(Inventory Inventory);
        int GetNextInventoryId();
        */
        Task<bool> CreateInventoryAsync(Inventory inventory);
        Task<List<Inventory>> GetInventoryAsync();
        Task<bool> InventoryExistAsync(int idInventory);
        Task<bool> UpdateInventoryAsync(int InventoryID, Inventory inventory);
        Task<bool> DeleteInventoryAsync(Inventory Inventory);
        Task<Inventory> GetInventoryAsync(int id);
        Task<int> GetNextInventoryIdAsync();

    }
}
