﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace CoinCollection.Models;

public partial class Cdate
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public virtual ICollection<CoinDate> CoinDates { get; set; } = new List<CoinDate>();
}