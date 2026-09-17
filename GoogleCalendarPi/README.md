# Daylight / GoogleCalendarPi

A .NET 10 Blazor Interactive Server calendar with a light sage interface, month and week views, year/date navigation, a mini calendar, calendar filters, a selected-day agenda, and a popup event composer with a calendar-plus watermark. No Google credentials are needed to explore the clearly labeled, session-only demo.

## Run

Install the .NET 10 SDK, then run from this folder:

```powershell
dotnet restore
dotnet run
```

Open http://localhost:5242. The launch profile enables Development and local static assets. For production, run the published output or Docker image, rather than running the source project with `--no-launch-profile`.

## Connect Google Calendar

1. Create a Google Cloud project and enable **Google Calendar API**.
2. Configure the OAuth consent screen. While the app is in Testing, add your Google account as a test user.
3. Create an OAuth client of type **Web application**. Add the exact authorized redirect URI `http://localhost:5242/signin-google`. For local Docker, also add `http://localhost:8080/signin-google`. For a deployed site, use `https://your-host/signin-google`.
4. Store the credentials outside source control:

```powershell
dotnet user-secrets set "Google:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "Google:ClientSecret" "YOUR_CLIENT_SECRET"
dotnet run
```

5. Click **Connect Google Calendar** and grant calendar access.

For local testing, you can instead fill in `Google.ClientId` and `Google.ClientSecret` in `appsettings.Development.json`, then run `dotnet run`. This local configuration file is excluded from Git, Docker build context, and published output. User secrets and environment variables override these values if already set.

The app requests calendar-list read access and event read/write access. It lists all subscribed calendars, expands recurring events, follows Google pagination, and only offers writable calendars when creating events. OAuth tokens are encrypted with ASP.NET Core Data Protection and stored outside the project in the current user's local application data/GoogleCalendarPi directory. Authentication keys are persisted alongside them, so connections survive app restarts. Login cookies and stored sessions last 30 days; Google may revoke or expire a grant earlier. Existing memory-only connections require one more sign-in after this upgrade. Disconnect deletes the saved token session and clears the login cookie; revoke the app in your Google account to revoke Google's grant. This implementation is intended for a single application instance. Multiple replicas need shared protected token storage and Blazor session affinity.

Times display in the browser's time zone. All-day event end dates are exclusive; selecting the same start/end date creates a one-day event. Clock-change times that are ambiguous or nonexistent are rejected when creating timed events. Existing multi-day events appear on each affected day. The Refresh button fetches the current visible range; background polling is not enabled. Demo events are lost on reload and are never sent to Google.

Google OAuth apps in Testing can have short-lived refresh grants. Public deployments may need Google's consent verification. Use HTTPS in deployment. If TLS terminates at a reverse proxy, configure ASP.NET Core forwarded headers with explicitly trusted proxy addresses before authentication, so Google receives an HTTPS callback URI. WebSockets must be supported for Blazor. Do not expose the container directly over unencrypted HTTP on the public internet.

## Docker

The multi-stage Dockerfile builds with the .NET 10 SDK and runs as the non-root .NET container user on port 8080. Docker Desktop must be running in Linux-container mode.

```powershell
# Build, tag and load locally (no upload)
.\scripts\Publish-Docker.ps1 -Username YOUR_DOCKERHUB_USERNAME -Tag 1.0.0
docker run --rm -p 8080:8080 YOUR_DOCKERHUB_USERNAME/google-calendar-pi:1.0.0

# Authenticate interactively; do not put your password in the script
docker login

# Build and publish to Docker Hub
.\scripts\Publish-Docker.ps1 -Username YOUR_DOCKERHUB_USERNAME -Tag 1.0.0 -Push

# Publish both PC and 64-bit Raspberry Pi images using a Buildx builder
docker buildx create --name daylight-builder --driver docker-container --use
.\scripts\Publish-Docker.ps1 -Username YOUR_DOCKERHUB_USERNAME -Tag 1.0.0 -Platform 'linux/amd64,linux/arm64' -Push
```

The script publishes exactly the requested tag. Use `-Tag latest` separately if desired. Raspberry Pi requires a 64-bit OS. Multi-platform publishing requires a Buildx builder with the relevant platform support/emulation.

For Compose, set secrets in your shell or an untracked `.env` file, then run:

```powershell
$env:GOOGLE_CLIENT_ID = 'YOUR_CLIENT_ID'
$env:GOOGLE_CLIENT_SECRET = 'YOUR_CLIENT_SECRET'
docker compose up --build -d
```

For `docker run`, pass `Google__ClientId` and `Google__ClientSecret` as runtime environment variables. Never pass secrets as build arguments. `/health` is a lightweight health endpoint. No image is automatically pushed by Compose.

## Validation

```powershell
dotnet build -c Release
dotnet run --project tests/CalendarChecks/CalendarChecks.csproj
dotnet publish GoogleCalendarPi.csproj -c Release -o artifacts/publish
```

The dependency-free checks exercise Monday week boundaries, leap days, exclusive event ends, overnight events, and multi-day events. Google API round trips require your configured OAuth account. Docker builds require a running Docker engine.

## References

- [Blazor render modes](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0)
- [Google OAuth web-server flow](https://developers.google.com/identity/protocols/oauth2/web-server)
- [Google event listing](https://developers.google.com/workspace/calendar/api/v3/reference/events/list)
- [Google event creation](https://developers.google.com/workspace/calendar/api/v3/reference/events/insert)

The interface uses text/CSS symbols and does not redistribute third-party icon artwork.


Storage can be relocated with CalendarStorage__Path. Keep this directory private: it contains encrypted tokens and their protection keys. Compose persists /app/data in the calendar-data volume. With docker run, add -v calendar-data:/app/data to preserve connections when replacing the container. Do not delete the volume or keys unless you intend to reset saved connections.

