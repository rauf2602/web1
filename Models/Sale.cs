using System.ComponentModel.DataAnnotations;

namespace BlazorApp4.Models;

public sealed class Sale
{
    public int Id { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.Now;

    public decimal TotalPrice { get; set; }

    public List<SaleItem> Items { get; set; } = new();
}

public sealed class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = "";

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}