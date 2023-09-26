using CsvHelper.Configuration.Attributes;

namespace api3.Dto
{
    public class InventoryCsvDtoFirst
    {
        [Name("Store")]
        public string Store { get; set; }

        [Name(" Date")] // Espacio antes de Date
        public string Date { get; set; }

        [Name(" Flavor")] // Espacio antes de Flavor
        public string Flavor { get; set; }

        [Name(" Is Season Flavor")] // Espacio antes de Is Season Flavor
        public string IsSeasonFlavor { get; set; }

        [Name(" Quantity")] // Espacio antes de Quantity
        public string Quantity { get; set; }

        [Name(" Listed By")] // Espacio antes de Listed By
        public string ListedBy { get; set; }
    }
    public class InventoryCsvDtoSecond
    {
        [Name("Store")]
        public string Store { get; set; }

        [Name("Date")] // Espacio antes de Date
        public string Date { get; set; }

        [Name("Flavor")] // Espacio antes de Flavor
        public string Flavor { get; set; }

        [Name("Is Season Flavor")] // Espacio antes de Is Season Flavor
        public string IsSeasonFlavor { get; set; }

        [Name("Quantity")] // Espacio antes de Quantity
        public string Quantity { get; set; }

        [Name("Listed By")] // Espacio antes de Listed By
        public string ListedBy { get; set; }
    }
}
