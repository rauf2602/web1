using BlazorApp4.Data;
using BlazorApp4.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp4.Services;

public sealed class MySqlInventoryRepository(AppDbContext dbContext) : IInventoryRepository
{
    public Task<List<InventoryItem>> GetAllAsync() =>
        dbContext.InventoryItems
            .OrderByDescending(x => x.Id)
            .ToListAsync();

    public Task<InventoryItem?> GetByIdAsync(int id) =>
        dbContext.InventoryItems.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<InventoryItem> AddAsync(InventoryItem item)
    {
        item.TotalAmount = item.Quantity * item.Price;
        item.Date = DateTime.Now;

        dbContext.InventoryItems.Add(item);
        await dbContext.SaveChangesAsync();
        return item;
    }

    public async Task<bool> UpdateAsync(InventoryItem item)
    {
        var existing = await dbContext.InventoryItems.FirstOrDefaultAsync(x => x.Id == item.Id);
        if (existing is null)
        {
            return false;
        }

        existing.Name = item.Name;
        existing.Quantity = item.Quantity;
        existing.Price = item.Price;
        existing.TotalAmount = item.Quantity * item.Price;
        existing.Date = DateTime.Now;
        existing.MinLevel = item.MinLevel;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await dbContext.InventoryItems.FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null)
        {
            return false;
        }

        dbContext.InventoryItems.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
