using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api3.Dto;

public partial class EmployeeDto
{
    public int IdEmployee { get; set; }

    public string? Name { get; set; }

    //public virtual ICollection<InventoryDto> Inventories { get; set; } = new List<InventoryDto>();
}
public partial class EmployeeUpdateDto
{
    public string Name { get; set; }

}
