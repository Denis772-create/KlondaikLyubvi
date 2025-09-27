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
builder.Services.AddScoped<RomanceExchangeService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<WishlistService>();
// HttpClient for Blazor Server: resolve BaseAddress from NavigationManager within scoped lifetime
builder.Services.AddScoped<HttpClient>(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

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

app.MapPost("/api/lovenotes", async (HttpContext ctx, KlondaikLyubvi.Shared.LoveNoteDto dto, LoveNoteService service, TelegramService telegram, AppDbContext db) =>
{
    // TODO: получить userId из куки/сессии
    int userId = dto.UserId;
    var note = await service.AddAsync(userId, dto.Text);

    // Telegram notification to partner with link to the site
    try
    {
        var partnerId = userId == 1 ? 2 : 1;
        var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
        var link = $"{baseUrl}/love";
        // Cute message without revealing the note text
        var sender = await db.Users.FindAsync(userId);
        var senderName = sender?.DisplayName ?? "Партнёр";
        var msg = $"💌 У тебя новое признание от {senderName}!\nОткрой, когда будешь готов(а) улыбнуться. 🫶";
        await telegram.SendMessageWithUrlButtonAsync(partnerId, msg, "Открыть признания", link);
    }
    catch { }

    return new LoveNoteDto
    {
        Id = note.Id,
        Text = note.Text,
        Date = note.Date,
        UserId = note.UserId,
        UserDisplayName = note.User?.DisplayName
    };
});

app.MapPut("/api/lovenotes/{id}", async (int id, [FromBody] KlondaikLyubvi.Shared.LoveNoteDto dto, AppDbContext db) =>
{
    var note = await db.LoveNotes.FindAsync(id);
    if (note == null) return Results.NotFound();
    note.Text = dto.Text ?? string.Empty;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/login", async (HttpContext ctx, [FromBody] LoginRequest req, AuthService auth) =>
{
    var userId = await auth.ValidateUserAsync(req.UserName, req.Password);
    if (userId != null)
    {
        ctx.Response.Cookies.Append("userId", userId?.ToString() ?? "", new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.Now + TimeSpan.FromDays(1)});
        return Results.Ok(userId);
    }
    return Results.Unauthorized();
});

app.MapGet("/api/storeitems", async (AppDbContext db) =>
    await db.ServiceOffers.Select(x => new { x.Id, x.Name, x.Description, x.Emoji }).ToListAsync()
);

// Убрали систему кредитов - теперь прямой обмен услугами

// Legacy endpoint - deprecated
app.MapPost("/api/buy", () => Results.BadRequest("This endpoint is deprecated. Use /api/exchanges/request instead."));

// Legacy endpoint - use /api/exchanges/history/{userId} instead
app.MapGet("/api/history/{userId}", () => Results.BadRequest("This endpoint is deprecated. Use /api/exchanges/history/{userId} instead."));

app.MapGet("/api/users", async (AppDbContext db) =>
    await db.Users.Select(u => new { u.Id, u.UserName, u.DisplayName }).ToListAsync()
);

app.MapDelete("/api/lovenotes/{id}", async (int id, AppDbContext db) =>
{
    var note = await db.LoveNotes.FindAsync(id);
    if (note == null) return Results.NotFound();
    db.LoveNotes.Remove(note);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// Photo gallery endpoints f
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
    // Убрали систему кредитов
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/admin/subtractpoints", async ([FromBody] PointsRequest req, AppDbContext db) =>
{
    int userId = req.UserId;
    int amount = req.Amount;
    var user = await db.Users.FindAsync(userId);
    if (user == null) return Results.BadRequest("User not found");
    // Убрали систему кредитов
    await db.SaveChangesAsync();
    return Results.Ok();
});

// Legacy endpoint - deprecated
app.MapGet("/api/gifts/{userId}", () => Results.BadRequest("This endpoint is deprecated. Use /api/exchanges/pending/{userId} instead."));

// Legacy endpoint - deprecated
app.MapPost("/api/gifts/execute/{giftId}", () => Results.BadRequest("This endpoint is deprecated. Use /api/exchanges/complete instead."));

// Legacy endpoint - deprecated
app.MapPost("/api/gifts/exchange/{giftId}", () => Results.BadRequest("This endpoint is deprecated."));

app.MapGet("/api/myitems/{userId}", async (int userId, AppDbContext db) =>
    await db.ServiceOffers.Where(x => x.UserId == userId).Select(x => new { x.Id, x.Name, x.Description, x.Emoji, x.Category }).ToListAsync()
);

app.MapPost("/api/storeitems", async (AppDbContext db, [FromBody] StoreItemCreateDto dto) =>
{
    var item = new ServiceOffer
    {
        Name = dto.Name,
        Description = dto.Description,
        // Убрали TrustCost 
        Emoji = dto.Emoji,
        UserId = dto.UserId,
        Category = "Общее"
    };
    db.ServiceOffers.Add(item);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPut("/api/storeitems/{id}", async (int id, AppDbContext db, [FromBody] StoreItemCreateDto dto) =>
{
    var item = await db.ServiceOffers.FindAsync(id);
    if (item == null) return Results.NotFound();
    item.Name = dto.Name;
    item.Description = dto.Description;
    // Убрали TrustCost
    item.Emoji = dto.Emoji;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapDelete("/api/storeitems/{id}", async (int id, AppDbContext db) =>
{
    var item = await db.ServiceOffers.FindAsync(id);
    if (item == null) return Results.NotFound();
    db.ServiceOffers.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/api/storeitems/for/{userId}", async (int userId, AppDbContext db) =>
    await db.ServiceOffers.Where(x => x.UserId != userId && x.IsActive).Select(x => new { x.Id, x.Name, x.Description, x.Emoji, x.Category }).ToListAsync()
);

// Calendar events (purchases execution dates, invites, active gifts)
app.MapGet("/api/calendar/{userId}", async (int userId, int year, int month, AppDbContext db) =>
{
    var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
    var end = start.AddMonths(1);

    // Active exchanges with scheduled dates
    var exchanges = await db.ServiceExchanges
        .Include(e => e.RequestedService)
        .Where(e => (e.RequesterId == userId || e.ProviderId == userId) && 
                   e.Status == ExchangeStatus.Accepted && 
                   e.ScheduledDate != null && 
                   e.ScheduledDate >= start && e.ScheduledDate < end)
        .Select(e => new {
            Id = e.Id,
            Date = e.ScheduledDate!.Value,
            Title = "Обмен: " + (e.RequestedService != null ? e.RequestedService.Name : "услуга"),
            Emoji = e.RequestedService != null ? e.RequestedService.Emoji : "💝",
            Type = "exchange",
            Description = "Запланированный обмен услугами"
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

    var all = exchanges.Cast<object>().Concat(invites)
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

// Wishlist endpoints
app.MapGet("/api/wishlist/{userId}", async (int userId, WishlistService svc) =>
{
    var list = await svc.GetForUserAsync(userId);
    return Results.Ok(list.Select(w => new {
        w.Id,
        w.UserId,
        w.Occasion,
        w.Url,
        w.Title,
        w.Note,
        w.CreatedAt,
        w.MetaTitle,
        w.MetaDescription,
        w.MetaImage,
        w.MetaSiteName,
        w.MetaUrl
    }));
});

app.MapPost("/api/wishlist", async ([FromBody] KlondaikLyubvi.Models.WishlistCreateDto dto, WishlistService svc) =>
{
    var item = await svc.AddAsync(dto.UserId, dto.Occasion, dto.Url, dto.Title, dto.Note);
    return Results.Ok(item);
});

app.MapPut("/api/wishlist/{id}", async (int id, [FromBody] KlondaikLyubvi.Models.WishlistCreateDto dto, WishlistService svc) =>
{
    var ok = await svc.UpdateAsync(id, dto.Occasion, dto.Title, dto.Note);
    return ok ? Results.Ok() : Results.NotFound();
});

app.MapPost("/api/wishlist/{id}/refresh", async (int id, WishlistService svc) =>
{
    var ok = await svc.RefreshMetadataAsync(id);
    return ok ? Results.Ok() : Results.NotFound();
});

app.MapDelete("/api/wishlist/{id}", async (int id, WishlistService svc) =>
{
    var ok = await svc.DeleteAsync(id);
    return ok ? Results.Ok() : Results.NotFound();
});

// Romance Exchange API endpoints
app.MapGet("/api/services/available/{userId}", async (int userId, RomanceExchangeService service) =>
{
    var services = await service.GetAvailableServicesAsync(userId);
    return Results.Ok(services.Select(s => new {
        s.Id, s.Name, s.Description, s.Emoji, s.UserId, 
        UserDisplayName = s.User?.DisplayName, s.Category, s.IsActive, s.CreatedAt
    }));
});

app.MapGet("/api/services/my/{userId}", async (int userId, RomanceExchangeService service) =>
{
    var services = await service.GetMyServicesAsync(userId);
    return Results.Ok(services.Select(s => new {
        s.Id, s.Name, s.Description, s.Emoji, s.UserId, 
        UserDisplayName = s.User?.DisplayName, s.Category, s.IsActive, s.CreatedAt
    }));
});

app.MapGet("/api/exchanges/pending/{userId}", async (int userId, RomanceExchangeService service) =>
{
    var exchanges = await service.GetPendingRequestsAsync(userId);
    return Results.Ok(exchanges.Select(e => new {
        e.Id, e.RequesterId, RequesterName = e.Requester?.DisplayName, 
        e.ProviderId, ProviderName = e.Provider?.DisplayName,
        RequestedServiceName = e.RequestedService?.Name, RequestedServiceEmoji = e.RequestedService?.Emoji,
        OfferedServiceName = e.OfferedService?.Name, OfferedServiceEmoji = e.OfferedService?.Emoji,
        e.RequestDate, e.ScheduledDate, Status = e.Status.ToString(),
        e.RequestMessage, e.ResponseMessage, e.CompletedDate, e.Rating, e.Review
    }));
});

app.MapGet("/api/exchanges/active/{userId}", async (int userId, RomanceExchangeService service) =>
{
    var exchanges = await service.GetActiveExchangesAsync(userId);
    return Results.Ok(exchanges.Select(e => new {
        e.Id, e.RequesterId, RequesterName = e.Requester?.DisplayName, 
        e.ProviderId, ProviderName = e.Provider?.DisplayName,
        RequestedServiceName = e.RequestedService?.Name, RequestedServiceEmoji = e.RequestedService?.Emoji,
        OfferedServiceName = e.OfferedService?.Name, OfferedServiceEmoji = e.OfferedService?.Emoji,
        e.RequestDate, e.ScheduledDate, Status = e.Status.ToString(),
        e.RequestMessage, e.ResponseMessage, e.CompletedDate, e.Rating, e.Review
    }));
});

app.MapGet("/api/exchanges/history/{userId}", async (int userId, RomanceExchangeService service) =>
{
    var exchanges = await service.GetMyExchangesAsync(userId);
    return Results.Ok(exchanges.Select(e => new {
        e.Id, e.RequesterId, RequesterName = e.Requester?.DisplayName, 
        e.ProviderId, ProviderName = e.Provider?.DisplayName,
        RequestedServiceName = e.RequestedService?.Name, RequestedServiceEmoji = e.RequestedService?.Emoji,
        OfferedServiceName = e.OfferedService?.Name, OfferedServiceEmoji = e.OfferedService?.Emoji,
        e.RequestDate, e.ScheduledDate, Status = e.Status.ToString(),
        e.RequestMessage, e.ResponseMessage, e.CompletedDate, e.Rating, e.Review
    }));
});

app.MapPost("/api/exchanges/request", async ([FromBody] ExchangeRequestDto req, RomanceExchangeService service, AppDbContext db) =>
{
    try
    {
        // Проверяем, что пользователь существует
        var user = await db.Users.FindAsync(req.RequesterId);
        if (user == null)
            return Results.BadRequest($"Пользователь с ID {req.RequesterId} не найден");
            
        // Проверяем, что услуга существует
        var requestedService = await db.ServiceOffers.FindAsync(req.RequestedServiceId);
        if (requestedService == null)
            return Results.BadRequest($"Услуга с ID {req.RequestedServiceId} не найдена");
            
        if (!requestedService.IsActive)
            return Results.BadRequest("Услуга неактивна");
        
        var success = await service.RequestServiceAsync(req.RequesterId, req.RequestedServiceId, req.OfferedServiceId, req.ScheduledDate, req.Message);
        return success ? Results.Ok() : Results.BadRequest("Не удалось создать запрос на обмен");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Ошибка: {ex.Message}");
    }
});

app.MapPost("/api/exchanges/respond", async ([FromBody] ExchangeResponseDto req, RomanceExchangeService service) =>
{
    try
    {
        var success = await service.RespondToRequestAsync(req.ExchangeId, req.Accept, req.ResponseMessage, req.NewScheduledDate);
        return success ? Results.Ok() : Results.BadRequest("Ошибка обработки ответа");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Ошибка: {ex.Message}");
    }
});

app.MapPost("/api/exchanges/complete", async ([FromBody] ExchangeCompleteDto req, RomanceExchangeService service) =>
{
    try
    {
        var success = await service.CompleteExchangeAsync(req.ExchangeId, req.Rating, req.Review);
        return success ? Results.Ok() : Results.BadRequest("Ошибка завершения обмена");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Ошибка: {ex.Message}");
    }
});

// Debug endpoint для проверки данных
app.MapGet("/api/debug/data/{userId}", async (int userId, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(userId);
    var services = await db.ServiceOffers.Where(s => s.UserId != userId).ToListAsync();
    var myServices = await db.ServiceOffers.Where(s => s.UserId == userId).ToListAsync();
    
    return Results.Ok(new {
        User = user != null ? new { user.Id, user.DisplayName } : null,
        AvailableServices = services.Count,
        MyServices = myServices.Count,
        Services = services.Select(s => new { s.Id, s.Name, s.IsActive, s.UserId }).ToList()
    });
});

// Service management endpoints
app.MapPost("/api/services", async ([FromBody] ServiceOfferCreateDto req, RomanceExchangeService service) =>
{
    try
    {
        var serviceOffer = await service.CreateServiceOfferAsync(req.UserId, req.Name, req.Description, req.Emoji, req.Category);
        return Results.Ok(new { serviceOffer.Id, serviceOffer.Name, serviceOffer.Description, serviceOffer.Emoji, serviceOffer.Category });
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Ошибка: {ex.Message}");
    }
});

app.MapPut("/api/services/{serviceId}", async (int serviceId, [FromBody] ServiceOfferCreateDto req, RomanceExchangeService service) =>
{
    try
    {
        var success = await service.UpdateServiceOfferAsync(serviceId, req.UserId, req.Name, req.Description, req.Emoji, req.Category);
        return success ? Results.Ok() : Results.BadRequest("Услуга не найдена или нет прав на редактирование");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Ошибка: {ex.Message}");
    }
});

app.MapDelete("/api/services/{serviceId}/{userId}", async (int serviceId, int userId, RomanceExchangeService service) =>
{
    try
    {
        var success = await service.DeleteServiceOfferAsync(serviceId, userId);
        return success ? Results.Ok() : Results.BadRequest("Услуга не найдена или нет прав на удаление");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Ошибка: {ex.Message}");
    }
});

app.MapPost("/api/services/{serviceId}/toggle/{userId}", async (int serviceId, int userId, RomanceExchangeService service) =>
{
    try
    {
        var success = await service.ToggleServiceActiveAsync(serviceId, userId);
        return success ? Results.Ok() : Results.BadRequest("Услуга не найдена или нет прав на изменение");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Ошибка: {ex.Message}");
    }
});

app.Run();