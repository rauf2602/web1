using BlazorApp4.Models;

namespace BlazorApp4.Services;

public interface IInventoryRepository
{
    Task<List<InventoryItem>> GetAllAsync();
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<InventoryItem> AddAsync(InventoryItem item);
    Task<bool> UpdateAsync(InventoryItem item);
    Task<bool> DeleteAsync(int id);
}
