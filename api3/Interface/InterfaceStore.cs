using api3.Models; // ES UN USING

namespace api3.Interface // nombre de la interfaz
{
    public interface InterfaceStore // PUPU
    {
        ICollection<Store> GetStore(); // get
        //Facturas GetFacturas(int id);
        bool UpdateStore(int StoreID, Store Store); // put
        bool save(); // guardar
        bool StoreExist(int IdStore ); // put y post
        bool CreateStore(Store Store); //post
        Store GetStore(int id);
        bool DeleteStore(Store Store);
        int GetNextStoreId();

        int GetStoreIdByName(string storeName);
    }
}
