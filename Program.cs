using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using KlondaikLyubvi.Data;
using KlondaikLyubvi.Models;
using KlondaikLyubvi.Services;
using KlondaikLyubvi.Shared;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add EF Core with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddSingleton<TelegramService>();
builder.Services.AddScoped<LoveNoteService>();
builder.Services.AddScoped<LoveStoreService>();
builder.Services.AddScoped<AuthService>();
// HttpClient for Blazor Server: resolve BaseAddress from NavigationManager within scoped lifetime
builder.Services.AddScoped<HttpClient>(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGet("/api/lovenotes", async (LoveNoteService service) =>
{
    var notes = await service.GetAllAsync();
    return notes.Select(n => new KlondaikLyubvi.Shared.LoveNoteDto
    {
        Id = n.Id,
        Text = n.Text,
        Date = n.Date,
        UserId = n.UserId,
        UserDisplayName = n.User?.DisplayName
    });
});

app.MapPost("/api/lovenotes", async (KlondaikLyubvi.Shared.LoveNoteDto dto, LoveNoteService service) =>
{
    // TODO: получить userId из куки/сессии
    int userId = dto.UserId;
    var note = await service.AddAsync(userId, dto.Text);
    return new KlondaikLyubvi.Shared.LoveNoteDto
    {
        Id = note.Id,
        Text = note.Text,
        Date = note.Date,
        UserId = note.UserId,
        UserDisplayName = note.User?.DisplayName
    };
});

app.MapPost("/api/login", async (HttpContext ctx, [FromBody] LoginRequest req, AuthService auth) =>
{
    var userId = await auth.ValidateUserAsync(req.UserName, req.Password);
    if (userId != null)
    {
        ctx.Response.Cookies.Append("userId", userId.ToString(), new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.Now + TimeSpan.FromDays(1)});
        return Results.Ok(userId);
    }
    return Results.Unauthorized();
});

app.MapGet("/api/storeitems", async (AppDbContext db) =>
    await db.StoreItems.Select(x => new { x.Id, x.Name, x.Description, x.Price }).ToListAsync()
);

app.MapGet("/api/balance/{userId}", async (int userId, LoveStoreService store) => await store.GetBalanceAsync(userId));

app.MapPost("/api/buy",
    async (BuyRequest req, LoveStoreService store) =>
        await store.BuyAsync(req.UserId, req.StoreItemId, req.IsGift, req.ToUserId, req.ExecutionDate, req.GiftStartDate, req.GiftEndDate, req.GiftCount));

app.MapGet("/api/history/{userId}", async (int userId, LoveStoreService store, AppDbContext db) =>
{
    var history = await store.GetHistoryAsync(userId);
    var result = history.Select(t => new {
        t.Id,
        StoreItemName = t.StoreItem != null ? t.StoreItem.Name : null,
        UserName = t.User != null ? t.User.DisplayName : null,
        ToUserName = t.ToUser != null ? t.ToUser.DisplayName : null,
        t.Date,
        t.IsGift,
        t.ExecutionDate,
        t.IsExecuted,
        t.GiftStartDate,
        t.GiftEndDate,
        t.GiftCount
    });
    return Results.Ok(result);
});

app.MapGet("/api/users", async (AppDbContext db) =>
    await db.Users.Select(u => new { u.Id, u.UserName, u.LovePoints }).ToListAsync()
);

app.MapDelete("/api/lovenotes/{id}", async (int id, AppDbContext db) =>
{
    var note = await db.LoveNotes.FindAsync(id);
    if (note == null) return Results.NotFound();
    db.LoveNotes.Remove(note);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// Photo gallery endpoints
app.MapGet("/api/photos", async (AppDbContext db) =>
{
    var photos = await db.Photos.OrderByDescending(p => p.UploadedAt).ToListAsync();
    return Results.Ok(photos.Select(p => new { p.Id, p.FileName, p.UploadedAt, p.UserId }));
});

app.MapPost("/api/photos", async (HttpRequest request, AppDbContext db, IWebHostEnvironment env) =>
{
    if (!request.HasFormContentType) return Results.BadRequest("No form data");
    var form = await request.ReadFormAsync();
    var files = form.Files;
    if (files.Count == 0) return Results.BadRequest("No files");
    var uploadsDir = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
    Directory.CreateDirectory(uploadsDir);
    foreach (var file in files)
    {
        if (file.Length == 0) continue;
        var ext = Path.GetExtension(file.FileName);
        var name = $"{Guid.NewGuid():N}{ext}";
        var full = Path.Combine(uploadsDir, name);
        await using (var fs = new FileStream(full, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }
        // userId из cookie
        int.TryParse(request.Cookies["userId"], out var uid);
        db.Photos.Add(new Photo { FileName = name, UploadedAt = DateTime.UtcNow, UserId = uid == 0 ? 1 : uid });
    }
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapDelete("/api/photos/{id}", async (int id, AppDbContext db, IWebHostEnvironment env) =>
{
    var photo = await db.Photos.FindAsync(id);
    if (photo == null) return Results.NotFound();
    var uploadsDir = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
    var full = Path.Combine(uploadsDir, photo.FileName);
    if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
    db.Photos.Remove(photo);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/admin/addpoints", async ([FromBody] PointsRequest req, AppDbContext db) =>
{
    int userId = req.UserId;
    int amount = req.Amount;
    var user = await db.Users.FindAsync(userId);
    if (user == null) return Results.BadRequest("User not found");
    user.LovePoints += amount;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/admin/subtractpoints", async ([FromBody] PointsRequest req, AppDbContext db) =>
{
    int userId = req.UserId;
    int amount = req.Amount;
    var user = await db.Users.FindAsync(userId);
    if (user == null) return Results.BadRequest("User not found");
    user.LovePoints -= amount;
    if (user.LovePoints < 0) user.LovePoints = 0;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/api/gifts/{userId}", async (int userId, LoveStoreService store) =>
{
    var gifts = await store.GetHistoryAsync(userId);
    return Results.Ok(gifts.Where(t => t.IsGift && t.ToUserId == userId).Select(t => new {
        t.Id,
        StoreItemName = t.StoreItem != null ? t.StoreItem.Name : null,
        UserName = t.User != null ? t.User.DisplayName : null,
        ToUserName = t.ToUser != null ? t.ToUser.DisplayName : null,
        t.Date,
        t.IsGift,
        t.ExecutionDate,
        t.IsExecuted,
        t.GiftStartDate,
        t.GiftEndDate,
        t.GiftCount
    }));
});

app.MapPost("/api/gifts/execute/{giftId}", async (int giftId, AppDbContext db) =>
{
    var gift = await db.LoveCoinTransactions.FindAsync(giftId);
    if (gift == null || !gift.IsGift) return Results.NotFound();
    gift.IsExecuted = true;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/gifts/exchange/{giftId}", async (int giftId, AppDbContext db) =>
{
    var gift = await db.LoveCoinTransactions.Include(t => t.StoreItem).FirstOrDefaultAsync(t => t.Id == giftId);
    if (gift == null || !gift.IsGift || gift.IsExecuted) return Results.NotFound();
    if (gift.ToUserId == null || gift.StoreItem == null) return Results.BadRequest();
    var recipient = await db.Users.FindAsync(gift.ToUserId);
    if (recipient == null) return Results.BadRequest();
    recipient.LovePoints += gift.StoreItem.Price;
    gift.IsExecuted = true;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/api/myitems/{userId}", async (int userId, AppDbContext db) =>
    await db.StoreItems.Where(x => x.UserId == userId).Select(x => new { x.Id, x.Name, x.Description, x.Price, x.Emoji }).ToListAsync()
);

app.MapPost("/api/storeitems", async (AppDbContext db, [FromBody] StoreItemCreateDto dto) =>
{
    var item = new StoreItem
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        Emoji = dto.Emoji,
        UserId = dto.UserId
    };
    db.StoreItems.Add(item);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPut("/api/storeitems/{id}", async (int id, AppDbContext db, [FromBody] StoreItemCreateDto dto) =>
{
    var item = await db.StoreItems.FindAsync(id);
    if (item == null) return Results.NotFound();
    item.Name = dto.Name;
    item.Description = dto.Description;
    item.Price = dto.Price;
    item.Emoji = dto.Emoji;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapDelete("/api/storeitems/{id}", async (int id, AppDbContext db) =>
{
    var item = await db.StoreItems.FindAsync(id);
    if (item == null) return Results.NotFound();
    db.StoreItems.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/api/storeitems/for/{userId}", async (int userId, AppDbContext db) =>
    await db.StoreItems.Where(x => x.UserId != userId).Select(x => new { x.Id, x.Name, x.Description, x.Price, x.Emoji }).ToListAsync()
);

// Calendar events (purchases execution dates, invites, active gifts)
app.MapGet("/api/calendar/{userId}", async (int userId, int year, int month, AppDbContext db) =>
{
    var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
    var end = start.AddMonths(1);

    // Purchases: not gifts, execution date in month, not executed
    var purchases = await db.LoveCoinTransactions
        .Include(t => t.StoreItem)
        .Where(t => !t.IsGift && t.UserId == userId && t.ExecutionDate != null && t.ExecutionDate >= start && t.ExecutionDate < end && !t.IsExecuted)
        .Select(t => new {
            Id = t.Id,
            Date = t.ExecutionDate!.Value,
            Title = "Исполнить: " + (t.StoreItem != null ? t.StoreItem.Name : "услуга"),
            Emoji = t.StoreItem != null ? t.StoreItem.Emoji : "💋",
            Type = "purchase",
            Description = "Запланированное исполнение покупки"
        })
        .ToListAsync();

    // Invites
    var invites = await db.Events
        .Where(e => e.Date >= start && e.Date < end)
        .Select(e => new {
            Id = e.Id,
            Date = e.Date,
            Title = e.Title,
            Emoji = "🎟️",
            Type = "invite",
            Description = e.Description
        })
        .ToListAsync();

    // Gifts for me: active range days
    var giftRanges = await db.LoveCoinTransactions
        .Include(t => t.StoreItem)
        .Where(t => t.IsGift && t.ToUserId == userId && t.GiftStartDate != null && t.GiftEndDate != null && t.GiftEndDate >= start && t.GiftStartDate < end)
        .Select(t => new {
            Id = t.Id,
            Start = t.GiftStartDate!.Value,
            End = t.GiftEndDate!.Value,
            Title = "Подарок: " + (t.StoreItem != null ? t.StoreItem.Name : "приятность"),
            Emoji = t.StoreItem != null ? t.StoreItem.Emoji : "🎁",
            Description = "Подарок можно использовать в этот день"
        })
        .ToListAsync();

    var giftDayEvents = new List<object>();
    foreach (var g in giftRanges)
    {
        var day = new DateTime(Math.Max(g.Start.Ticks, start.Ticks));
        var last = new DateTime(Math.Min(g.End.Ticks, end.AddDays(-1).Ticks));
        for (var d = day.Date; d <= last.Date; d = d.AddDays(1))
        {
            giftDayEvents.Add(new { Date = d, Title = g.Title, Emoji = g.Emoji, Type = "gift", Description = g.Description, g.Id });
        }
    }

    var all = purchases.Cast<object>().Concat(invites).Concat(giftDayEvents)
        .OrderBy(x => (DateTime)x.GetType().GetProperty("Date")!.GetValue(x)!)
        .Select(x => new {
            id = (int)(x.GetType().GetProperty("Id")?.GetValue(x) ?? 0),
            date = ((DateTime)x.GetType().GetProperty("Date")!.GetValue(x)!).ToString("o"),
            title = (string)x.GetType().GetProperty("Title")!.GetValue(x)!,
            emoji = (string)x.GetType().GetProperty("Emoji")!.GetValue(x)!,
            type = (string)x.GetType().GetProperty("Type")!.GetValue(x)!,
            description = (string?)x.GetType().GetProperty("Description")?.GetValue(x)
        });

    return Results.Ok(all);
});

// Invites
app.MapGet("/api/invites", async (AppDbContext db) =>
    await db.Events.OrderBy(e => e.Date).Select(e => new { e.Id, Title = e.Title, Description = e.Description, e.Date, Emoji = "🎟️", CreatedByUserId = e.UserId }).ToListAsync()
);

app.MapPost("/api/invites", async ([FromBody] Event e, AppDbContext db, TelegramService telegram) =>
{
    var ev = new Event
    {
        Title = e.Title,
        Description = e.Description,
        Date = e.Date,
        UserId = e.UserId
    };
    db.Events.Add(ev);
    await db.SaveChangesAsync();

    // Telegram notifications
    var creatorId = ev.UserId;
    var partnerId = creatorId == 1 ? 2 : 1;
    var creator = await db.Users.FindAsync(creatorId);
    var partner = await db.Users.FindAsync(partnerId);
    var dateStr = ev.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    var descriptionLine = string.IsNullOrWhiteSpace(ev.Description) ? string.Empty : $"— {ev.Description}\n";
    await telegram.SendMessageAsync(partnerId, $"🎟️ Приглашение!\n{creator?.DisplayName ?? "Любимый(ая)"} зовёт тебя на «{ev.Title}» {descriptionLine}🗓 {dateStr}\nЗагляни в раздел ‘Приглашения’ 💞");
    await telegram.SendMessageAsync(creatorId, $"🎟️ Ты пригласил(а) {partner?.DisplayName ?? "партнёра"} на «{ev.Title}».\n🗓 {dateStr}");

    return Results.Ok();
});

app.MapDelete("/api/invites/{id}", async (int id, HttpRequest request, AppDbContext db, TelegramService telegram) =>
{
    var ev = await db.Events.FindAsync(id);
    if (ev == null) return Results.NotFound();

    var reason = request.Query["reason"].ToString();
    var creatorId = ev.UserId;
    var partnerId = creatorId == 1 ? 2 : 1;
    var creator = await db.Users.FindAsync(creatorId);
    var partner = await db.Users.FindAsync(partnerId);
    var dateStr = ev.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    db.Events.Remove(ev);
    await db.SaveChangesAsync();

    var reasonLine = string.IsNullOrWhiteSpace(reason) ? "без указания причины" : reason;
    await telegram.SendMessageAsync(partnerId, $"🙏 Небольшое изменение планов.\n{creator?.DisplayName ?? "Партнёр"} отменил(а) приглашение «{ev.Title}» (🗓 {dateStr}).\nПричина: {reasonLine}");
    await telegram.SendMessageAsync(creatorId, $"❗ Ты отменил(а) «{ev.Title}». Мы предупредили {partner?.DisplayName ?? "партнёра"}.");

    return Results.Ok();
});
app.Run();