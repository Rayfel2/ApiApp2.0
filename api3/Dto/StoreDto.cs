using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;

namespace api3.Dto;

public partial class StoreDto
{
    public int IdStore { get; set; }

    public string? Name { get; set; }

    //public virtual ICollection<InventoryDto> Inventories { get; set; } = new List<InventoryDto>();
}
public partial class StoreUpdateDto
{
    
    public string Name { get; set; }

}

