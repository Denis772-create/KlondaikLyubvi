using KlondaikLyubvi.Data;
using Microsoft.EntityFrameworkCore;

namespace KlondaikLyubvi.Services;

public class LoveNoteService
{
    private readonly AppDbContext _db;
    //private readonly TelegramService _telegram;

    public LoveNoteService(AppDbContext db)
    {
        _db = db;
       // _telegram = telegram;
    }

    public async Task<List<LoveNote>> GetAllAsync() =>
        await _db.LoveNotes.Include(n => n.User).OrderByDescending(n => n.Date).ToListAsync();

    public async Task<LoveNote> AddAsync(int userId, string text)
    {
        var note = new LoveNote
        {
            UserId = userId,
            Text = text,
            Date = DateTime.UtcNow
        };
        _db.LoveNotes.Add(note);
        // +1 балл за признание
        var user = await _db.Users.FindAsync(userId);
        if (user != null) user.LovePoints++;
        await _db.SaveChangesAsync();
       // await _telegram.SendMessageAsync(userId, $"💌 Новая записка: {text}");
        return note;
    }
}