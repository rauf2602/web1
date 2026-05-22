using System.ComponentModel.DataAnnotations;

namespace BlazorApp4.Models;

public sealed class ProductLog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string ProductName { get; set; } = "";

    public int ProductId { get; set; }

    public string Action { get; set; } = "";

    public int PreviousQty { get; set; }

    public int ChangeQty { get; set; }

    public int NewQty { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;
}
