using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lingya.Pagination.Tests.Mock;

public class Order {
    [Key]
    public int Id { get; set; }
    public StreetAddress Address { get; set; }
}


[Owned]
public class StreetAddress {
    public string Street { get; set; }
    public string City { get; set; }
}