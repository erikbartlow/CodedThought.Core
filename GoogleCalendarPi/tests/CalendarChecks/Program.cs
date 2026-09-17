using GoogleCalendarPi.Services;

var count = 0;
void Check(bool value, string name) { if (!value) throw new Exception(name); count++; Console.WriteLine("PASS " + name); }
Check(CalendarDates.WeekStart(new(2027, 1, 1)) == new DateTime(2026, 12, 28), "Week crosses year boundary");
Check(CalendarDates.WeekStart(new(2026, 9, 20)) == new DateTime(2026, 9, 14), "Sunday belongs to preceding Monday");
Check(CalendarDates.WeekStart(new(2026, 9, 14)) == new DateTime(2026, 9, 14), "Monday remains Monday");
Check(CalendarDates.WeekStart(new(2024, 2, 29)) == new DateTime(2024, 2, 26), "Leap day week");
var allDay = new CalendarEvent("1", "primary", "Trip", new(2026, 9, 16), new(2026, 9, 19), true, "");
Check(CalendarDates.OccursOn(allDay, new(2026, 9, 16)), "All-day starts inclusively");
Check(CalendarDates.OccursOn(allDay, new(2026, 9, 18)), "Multi-day includes last occupied day");
Check(!CalendarDates.OccursOn(allDay, new(2026, 9, 19)), "All-day ends exclusively");
var overnight = allDay with { Start = new(2026, 9, 16, 23, 0, 0), End = new(2026, 9, 17, 1, 0, 0), AllDay = false };
Check(CalendarDates.OccursOn(overnight, new(2026, 9, 16)), "Overnight first day");
Check(CalendarDates.OccursOn(overnight, new(2026, 9, 17)), "Overnight second day");
Check(!CalendarDates.OccursOn(overnight with { End = new(2026, 9, 17) }, new(2026, 9, 17)), "Midnight end does not spill into next day");
Console.WriteLine($"{count} calendar checks passed.");

var storageRoot = Path.Combine(Path.GetTempPath(), "daylight-check-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(storageRoot);
var storage = new CalendarStorage(storageRoot);
var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
Microsoft.AspNetCore.DataProtection.IDataProtectionProvider NewProtection() =>
    Microsoft.AspNetCore.DataProtection.DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(storageRoot, "keys")),
        options => Microsoft.AspNetCore.DataProtection.DataProtectionBuilderExtensions.SetApplicationName(options, "GoogleCalendarPi"));
var sessionId = Guid.NewGuid().ToString("N");
var vault = new TokenVault(new CheckHttpFactory(), config, NewProtection(), storage);
vault.Add(sessionId, "test-access-only", "test-refresh-only", DateTimeOffset.UtcNow.AddHours(1));
var ciphertext = File.ReadAllText(Path.Combine(storageRoot, "sessions", sessionId + ".protected"));
Check(!ciphertext.Contains("test-access-only") && !ciphertext.Contains("test-refresh-only"), "Stored tokens are encrypted");
var restartedVault = new TokenVault(new CheckHttpFactory(), config, NewProtection(), storage);
Check(await restartedVault.GetAsync(sessionId) == "test-access-only", "Connection survives a new vault and key provider");
var otherRejected = false;
try { await restartedVault.GetAsync(Guid.NewGuid().ToString("N")); } catch (InvalidOperationException) { otherRejected = true; }
Check(otherRejected, "Unknown sessions cannot read another connection");
restartedVault.Remove(sessionId);
var removedRejected = false;
try { await vault.GetAsync(sessionId); } catch (InvalidOperationException) { removedRejected = true; }
Check(removedRejected, "Disconnect removes persisted connection");
Console.WriteLine($"{count} total checks passed.");

sealed class CheckHttpFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => throw new Exception("Fresh token must not require Google access.");
}
