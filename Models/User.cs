using System.ComponentModel.DataAnnotations;

namespace BlazorApp4.Models;

public sealed class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = "";

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}