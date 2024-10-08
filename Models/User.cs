﻿using System;
using System.Collections.Generic;

namespace Service.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string UserLogin { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
