using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
namespace GoogleCalendarPi.Services;

public record CalendarSource(string Id, string Name, string Color, bool Writable);
public record CalendarEvent(string Id, string CalendarId, string Title, DateTime Start, DateTime End, bool AllDay, string Location);
public static class CalendarDates
{
    public static DateTime WeekStart(DateTime date) => date.Date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
    public static bool OccursOn(CalendarEvent item, DateTime date) => item.Start < date.Date.AddDays(1) && item.End > date.Date;
}
public sealed class CalendarService(IHttpClientFactory clients, TokenVault vault, AuthenticationStateProvider authentication)
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null)
    {
        var user = (await authentication.GetAuthenticationStateAsync()).User;
        var token = await vault.GetAsync(user.FindFirst("calendar-session")?.Value);
        using var request = new HttpRequestMessage(method, "https://www.googleapis.com/calendar/v3/" + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body);
        var response = await clients.CreateClient().SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var status = (int)response.StatusCode;
            response.Dispose();
            throw new InvalidOperationException(status switch { 401 => "Please reconnect your Google account.", 403 => "Google denied access. Check Calendar API permissions and calendar write access.", 429 => "Google is busy. Please try again shortly.", _ => $"Google Calendar request failed ({status}). Please try again." });
        }
        return response;
    }
    private async Task<List<JsonElement>> ListAsync(string path)
    {
        var items = new List<JsonElement>();
        string? page = null;
        do
        {
            using var response = await SendAsync(HttpMethod.Get, path + (string.IsNullOrEmpty(page) ? "" : (path.Contains('?') ? "&" : "?") + "pageToken=" + Uri.EscapeDataString(page)));
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("items", out var array)) items.AddRange(array.EnumerateArray().Select(x => x.Clone()));
            page = Text(doc.RootElement, "nextPageToken");
        } while (!string.IsNullOrEmpty(page));
        return items;
    }
    private static string Text(JsonElement element, string key) => element.TryGetProperty(key, out var value) ? value.GetString() ?? "" : "";
    public async Task<List<CalendarSource>> CalendarsAsync() => (await ListAsync("users/me/calendarList?maxResults=250"))
        .Select((c, i) => new CalendarSource(Text(c, "id"), Text(c, "summary"), new[] { "sage", "lavender", "peach", "blue" }[i % 4], Text(c, "accessRole") is "owner" or "writer")).ToList();
    public async Task<List<CalendarEvent>> EventsAsync(IEnumerable<CalendarSource> calendars, DateTime start, DateTime end, TimeZoneInfo zone)
    {
        string Stamp(DateTime d) => Uri.EscapeDataString(new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(d, DateTimeKind.Unspecified), zone)).ToString("O"));
        var result = new List<CalendarEvent>();
        foreach (var calendar in calendars)
        {
            var items = await ListAsync($"calendars/{Uri.EscapeDataString(calendar.Id)}/events?singleEvents=true&orderBy=startTime&maxResults=2500&timeMin={Stamp(start)}&timeMax={Stamp(end)}");
            foreach (var e in items.Where(e => Text(e, "status") != "cancelled"))
            {
                var s = e.GetProperty("start"); var t = e.GetProperty("end");
                var allDay = s.TryGetProperty("date", out _);
                DateTime Parse(JsonElement v) => allDay ? DateTime.ParseExact(Text(v, "date"), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
                    : TimeZoneInfo.ConvertTime(DateTimeOffset.Parse(Text(v, "dateTime")), zone).DateTime;
                result.Add(new(Text(e, "id"), calendar.Id, Text(e, "summary") is { Length: > 0 } title ? title : "Untitled event", Parse(s), Parse(t), allDay, Text(e, "location")));
            }
        }
        return result;
    }
    public async Task CreateAsync(CalendarSource calendar, string title, DateTime start, DateTime end, bool allDay, string location, TimeZoneInfo zone)
    {
        if (!calendar.Writable) throw new InvalidOperationException("This calendar is read-only.");
        if (string.IsNullOrWhiteSpace(title) || end <= start) throw new InvalidOperationException("Enter a title and an end after the start.");
        object Point(DateTime date) => allDay ? new { date = date.ToString("yyyy-MM-dd") } : (object)new { dateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date, DateTimeKind.Unspecified), zone).ToString("O") };
        using var response = await SendAsync(HttpMethod.Post, $"calendars/{Uri.EscapeDataString(calendar.Id)}/events", new { summary = title.Trim(), start = Point(start), end = Point(end), location });
    }
}
