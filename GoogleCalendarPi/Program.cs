using System.Security.Claims;
using GoogleCalendarPi.Components;
using GoogleCalendarPi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
var storagePath = Path.GetFullPath(builder.Configuration["CalendarStorage:Path"] ??
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GoogleCalendarPi"));
Directory.CreateDirectory(storagePath);
builder.Services.AddSingleton(new CalendarStorage(storagePath));
builder.Services.AddDataProtection().SetApplicationName("GoogleCalendarPi")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(storagePath, "keys")));
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<TokenVault>();
builder.Services.AddScoped<CalendarService>();
var auth = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => { o.ExpireTimeSpan = TokenVault.SessionLifetime; o.SlidingExpiration = false; });
var google = builder.Configuration.GetSection("Google");
if (!string.IsNullOrWhiteSpace(google["ClientId"]) && !string.IsNullOrWhiteSpace(google["ClientSecret"]))
    auth.AddGoogle(o =>
    {
        o.ClientId = google["ClientId"]!;
        o.ClientSecret = google["ClientSecret"]!;
        o.AccessType = "offline";
        o.Scope.Add("https://www.googleapis.com/auth/calendar.calendarlist.readonly");
        o.Scope.Add("https://www.googleapis.com/auth/calendar.events");
        o.Events.OnRedirectToAuthorizationEndpoint = c => { c.Response.Redirect(c.RedirectUri + "&prompt=consent"); return Task.CompletedTask; };
        o.Events.OnCreatingTicket = c =>
        {
            var session = Guid.NewGuid().ToString("N");
            c.Identity!.AddClaim(new Claim("calendar-session", session));
            c.HttpContext.RequestServices.GetRequiredService<TokenVault>().Add(session, c.AccessToken!, c.RefreshToken,
                DateTimeOffset.UtcNow.Add(c.ExpiresIn ?? TimeSpan.FromHours(1)));
            return Task.CompletedTask;
        };
        o.Events.OnRemoteFailure = c => { c.HandleResponse(); c.Response.Redirect("/?authError=true"); return Task.CompletedTask; };
    });
builder.Services.AddAuthorization();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error", createScopeForErrors: true); app.UseHsts(); }
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapGet("/auth/google", async (HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(google["ClientId"]) || string.IsNullOrWhiteSpace(google["ClientSecret"]))
    { context.Response.Redirect("/?setup=true"); return; }
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/", IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.Add(TokenVault.SessionLifetime)
    });
});
app.MapPost("/auth/logout", async (HttpContext context, TokenVault vault) =>
{
    await context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>().ValidateRequestAsync(context);
    vault.Remove(context.User.FindFirstValue("calendar-session"));
    await context.SignOutAsync();
    context.Response.Redirect("/");
});
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
