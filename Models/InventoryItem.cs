using System.ComponentModel.DataAnnotations;

namespace BlazorApp4.Models;

public sealed class InventoryItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = "";

    public string ImageUrl { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Price { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime Date { get; set; }

    public int MinLevel { get; set; } = 5;
}