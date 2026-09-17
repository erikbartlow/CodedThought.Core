using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace GoogleCalendarPi.Services;

public sealed record CalendarStorage(string Path);

// Encrypted, server-only sessions survive process restarts. Single-instance storage.
public sealed class TokenVault(IHttpClientFactory clients, IConfiguration configuration,
    IDataProtectionProvider protection, CalendarStorage storage)
{
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(30);
    private readonly IDataProtector protector = protection.CreateProtector("Daylight.GoogleTokens.v1");
    private readonly SemaphoreSlim gate = new(1, 1);
    private sealed record Session(string Access, string? Refresh, DateTimeOffset Expires, DateTimeOffset SessionExpires);
    private string FilePath(string id)
    {
        if (!Guid.TryParseExact(id, "N", out _)) throw new InvalidOperationException("Please reconnect Google Calendar.");
        return System.IO.Path.Combine(storage.Path, "sessions", id + ".protected");
    }
    private void Write(string id, Session session)
    {
        var path = FilePath(id);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        var temp = path + ".tmp";
        try
        {
            File.WriteAllText(temp, protector.Protect(JsonSerializer.Serialize(session)));
            File.Move(temp, path, overwrite: true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    public void Add(string id, string access, string? refresh, DateTimeOffset expires)
    {
        gate.Wait();
        try { Write(id, new(access, refresh, expires, DateTimeOffset.UtcNow.Add(SessionLifetime))); }
        finally { gate.Release(); }
    }
    public void Remove(string? id)
    {
        if (id is null) return;
        gate.Wait();
        try { File.Delete(FilePath(id)); }
        finally { gate.Release(); }
    }
    public async Task<string> GetAsync(string? id)
    {
        if (id is null) throw new InvalidOperationException("Please reconnect Google Calendar.");
        await gate.WaitAsync();
        try
        {
            var path = FilePath(id);
            Session? session;
            try
            {
                if (!File.Exists(path)) throw new InvalidOperationException("Please reconnect Google Calendar once to save your connection.");
                session = JsonSerializer.Deserialize<Session>(protector.Unprotect(await File.ReadAllTextAsync(path)));
            }
            catch (Exception ex) when (ex is CryptographicException or JsonException)
            { throw new InvalidOperationException("Your saved Google connection could not be read. Please reconnect.", ex); }
            if (session is null || session.SessionExpires <= DateTimeOffset.UtcNow)
            {
                File.Delete(path);
                throw new InvalidOperationException("Your connection has expired. Please reconnect Google Calendar.");
            }
            if (session.Expires > DateTimeOffset.UtcNow.AddMinutes(2)) return session.Access;
            if (session.Refresh is null) throw new InvalidOperationException("Please reconnect Google Calendar to renew access.");
            using var response = await clients.CreateClient().PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = configuration["Google:ClientId"]!, ["client_secret"] = configuration["Google:ClientSecret"]!,
                ["refresh_token"] = session.Refresh, ["grant_type"] = "refresh_token"
            }));
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Google access could not be renewed. Please reconnect.");
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            session = session with {
                Access = json.RootElement.GetProperty("access_token").GetString()!,
                Expires = DateTimeOffset.UtcNow.AddSeconds(json.RootElement.GetProperty("expires_in").GetInt32()),
                Refresh = json.RootElement.TryGetProperty("refresh_token", out var refreshed) ? refreshed.GetString() : session.Refresh
            };
            Write(id, session);
            return session.Access;
        }
        finally { gate.Release(); }
    }
}
