﻿using System;
using System.Collections.Generic;

namespace api3.Models;

public partial class Employee
{
    public int IdEmployee { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
