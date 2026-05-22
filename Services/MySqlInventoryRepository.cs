using BlazorApp4.Data;
using BlazorApp4.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp4.Services;

public sealed class MySqlInventoryRepository(IDbContextFactory<AppDbContext> contextFactory) : IInventoryRepository
{
    public async Task<List<InventoryItem>> GetAllAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.InventoryItems
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<InventoryItem?> GetByIdAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.InventoryItems.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<InventoryItem> AddAsync(InventoryItem item)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        item.TotalAmount = item.Quantity * item.Price;
        item.Date = DateTime.Now;
        db.InventoryItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> UpdateAsync(InventoryItem item)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var existing = await db.InventoryItems.FirstOrDefaultAsync(x => x.Id == item.Id);
        if (existing is null) return false;

        existing.Name = item.Name;
        existing.Quantity = item.Quantity;
        existing.Price = item.Price;
        existing.TotalAmount = item.Quantity * item.Price;
        existing.Date = DateTime.Now;
        existing.MinLevel = item.MinLevel;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var existing = await db.InventoryItems.FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return false;

        db.InventoryItems.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
