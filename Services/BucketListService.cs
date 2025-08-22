using KlondaikLyubvi.Data;
using Microsoft.EntityFrameworkCore;

namespace KlondaikLyubvi.Services;

public class BucketListService(AppDbContext db)
{
    public async Task<bool> CompleteGoalAsync(int bucketItemId)
    {
        var item = await db.BucketItems.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == bucketItemId);
        if (item == null || item.IsCompleted) return false;
        item.IsCompleted = true;
        if (item.User != null) item.User.LovePoints += 3;
        await db.SaveChangesAsync();
        return true;
    }
}