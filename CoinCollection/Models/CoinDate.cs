﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace CoinCollection.Models;

public partial class CoinDate
{
    public int Id { get; set; }

    public int DateId { get; set; }

    public int CoinId { get; set; }

    public virtual Coin Coin { get; set; }

    public virtual Cdate Date { get; set; }
}