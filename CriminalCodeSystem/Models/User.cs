using System;
using System.Collections.Generic;

namespace CriminalCodeSystem.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
