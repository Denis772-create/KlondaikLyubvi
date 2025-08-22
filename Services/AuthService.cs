using KlondaikLyubvi.Data;
using Microsoft.EntityFrameworkCore;

namespace KlondaikLyubvi.Services;

public class AuthService(AppDbContext db)
{
    public async Task<int?> ValidateUserAsync(string username, string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username && u.PasswordHash == password);
        if (user != null)
        {
            var today = DateTime.UtcNow.Date;
            if (user.LastVisit == null || user.LastVisit.Value.Date < today)
            {
                user.LovePoints++;
                user.LastVisit = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            return user.Id;
        }
        return null;
    }
}