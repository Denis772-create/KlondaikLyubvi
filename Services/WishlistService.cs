using System.Net.Http.Headers;
using HtmlAgilityPack;
using KlondaikLyubvi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KlondaikLyubvi.Services;

public class WishlistService(AppDbContext db)
{
    public async Task<List<WishlistItem>> GetForUserAsync(int userId)
        => await db.WishlistItems.Where(w => w.UserId == userId).OrderByDescending(w => w.CreatedAt).ToListAsync();

    public async Task<WishlistItem> AddAsync(int userId, string occasion, string url, string? title, string? note)
    {
        var item = new WishlistItem
        {
            UserId = userId,
            Occasion = occasion,
            Url = url.Trim(),
            Title = title,
            Note = note,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var meta = await FetchUrlMetadataAsync(url);
            item.MetaTitle = meta.Title;
            item.MetaDescription = meta.Description;
            item.MetaImage = meta.Image;
            item.MetaSiteName = meta.SiteName;
            item.MetaUrl = meta.Url;
        }
        catch
        {
            // If direct fetch failed (403, cloudflare, etc.) — fallback to screenshot
            try { item.MetaImage = await TryMakeScreenshotAsync(url); item.MetaUrl = url; }
            catch { }
        }

        db.WishlistItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> UpdateAsync(int id, string? occasion, string? title, string? note)
    {
        var item = await db.WishlistItems.FindAsync(id);
        if (item == null) return false;
        if (!string.IsNullOrWhiteSpace(occasion)) item.Occasion = occasion;
        if (title != null) item.Title = title;
        if (note != null) item.Note = note;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RefreshMetadataAsync(int id)
    {
        var item = await db.WishlistItems.FindAsync(id);
        if (item == null) return false;
        try
        {
            var meta = await FetchUrlMetadataAsync(item.Url);
            item.MetaTitle = meta.Title;
            item.MetaDescription = meta.Description;
            item.MetaImage = meta.Image;
            item.MetaSiteName = meta.SiteName;
            item.MetaUrl = meta.Url;
            await db.SaveChangesAsync();
            return true;
        }
        catch
        {
            try
            {
                item.MetaImage = await TryMakeScreenshotAsync(item.Url);
                item.MetaUrl = item.Url;
                await db.SaveChangesAsync();
                return item.MetaImage != null;
            }
            catch { return false; }
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await db.WishlistItems.FindAsync(id);
        if (item == null) return false;
        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }

    private record UrlMetadata(string? Title, string? Description, string? Image, string? SiteName, string? Url);

    private async Task<UrlMetadata> FetchUrlMetadataAsync(string url)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36"));
        req.Headers.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        try { req.Headers.Referrer = new Uri(url); } catch { }
        req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        using var resp = await http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var html = await resp.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        string? GetMeta(string name)
            => doc.DocumentNode.SelectSingleNode($"//meta[@property='{name}']")?.GetAttributeValue("content", null)
            ?? doc.DocumentNode.SelectSingleNode($"//meta[@name='{name}']")?.GetAttributeValue("content", null);

        string? GetLink(string rel)
            => doc.DocumentNode.SelectSingleNode($"//link[translate(@rel,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{rel}']")?.GetAttributeValue("href", null);

        string MakeAbsolute(string? maybeRelative, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(maybeRelative)) return maybeRelative ?? baseUrl;
            if (maybeRelative.StartsWith("//"))
            {
                var scheme = new Uri(baseUrl).Scheme;
                return scheme + ":" + maybeRelative;
            }
            if (Uri.TryCreate(maybeRelative, UriKind.Absolute, out var abs)) return abs.ToString();
            var bu = new Uri(baseUrl);
            return new Uri(bu, maybeRelative).ToString();
        }

        var title = GetMeta("og:title") ?? doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim();
        var description = GetMeta("og:description") ?? GetMeta("description");
        var image = GetMeta("og:image:secure_url") ?? GetMeta("og:image") ?? GetMeta("twitter:image") ?? GetLink("image_src");
        var siteName = GetMeta("og:site_name");
        var canonical = GetLink("canonical");

        var baseUrl = canonical ?? url;

        // Try JSON-LD (common for shops) for image
        if (string.IsNullOrWhiteSpace(image))
        {
            var jsonLdNodes = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
            if (jsonLdNodes != null)
            {
                foreach (var node in jsonLdNodes)
                {
                    try
                    {
                        using var jdoc = JsonDocument.Parse(node.InnerText);
                        if (jdoc.RootElement.TryGetProperty("image", out var imgProp))
                        {
                            if (imgProp.ValueKind == JsonValueKind.String)
                            {
                                image = imgProp.GetString();
                                break;
                            }
                            if (imgProp.ValueKind == JsonValueKind.Array && imgProp.GetArrayLength() > 0)
                            {
                                image = imgProp[0].GetString();
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        // Fall back to first meaningful <img>
        if (string.IsNullOrWhiteSpace(image))
        {
            var imgNode = doc.DocumentNode.SelectSingleNode("//img[@src]");
            image = imgNode?.GetAttributeValue("src", null);
        }

        image = MakeAbsolute(image, baseUrl);

        // As last resort – generate a screenshot (Telegram-like) 1200x630
        if (string.IsNullOrWhiteSpace(image))
        {
            image = await TryMakeScreenshotAsync(baseUrl);
        }

        if (string.IsNullOrWhiteSpace(siteName))
        {
            try { siteName = new Uri(baseUrl).Host; } catch { }
        }

        return new UrlMetadata(title, description, image, siteName, baseUrl);
    }

    private Task<string?> TryMakeScreenshotAsync(string targetUrl)
    {
        // Use WordPress mShots as a lightweight remote screenshot fallback (like Telegram)
        try
        {
            var shot = $"https://s0.wp.com/mshots/v1/{Uri.EscapeDataString(targetUrl)}?w=1200&h=630";
            return Task.FromResult<string?>(shot);
        }
        catch { return Task.FromResult<string?>(null); }
    }
}


