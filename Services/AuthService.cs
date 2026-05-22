using System.Security.Cryptography;
using System.Text;
using BlazorApp4.Data;
using BlazorApp4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorApp4.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string username, string password);
    Task<User?> LoginAsync(string username, string password);
    Task<bool> UserExistsAsync(string username);
}

public sealed class AuthService(AppDbContext dbContext, ILogger<AuthService> logger) : IAuthService
{
    public async Task<bool> RegisterAsync(string username, string password)
    {
        logger.LogInformation("RegisterAsync called for: {Username}", username);
        
        try
        {
            var exists = await dbContext.Users.AnyAsync(u => u.Username == username);
            logger.LogInformation("User exists check result: {Exists}", exists);
            
            if (exists)
                return false;

            var passwordHash = HashPassword(password);
            logger.LogInformation("Password hashed successfully");

            var user = new User { Username = username, PasswordHash = passwordHash };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("User saved successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in RegisterAsync");
            throw;
        }
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;

        return VerifyPassword(password, user.PasswordHash) ? user : null;
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await dbContext.Users.AnyAsync(u => u.Username == username);
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var stored = Convert.FromBase64String(parts[1]);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, stored);
    }
}