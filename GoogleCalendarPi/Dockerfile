FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY GoogleCalendarPi.csproj ./
RUN dotnet restore GoogleCalendarPi.csproj
COPY . .
RUN dotnet publish GoogleCalendarPi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV CalendarStorage__Path=/app/data
EXPOSE 8080
COPY --from=build /app/publish .
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app/data
USER $APP_UID
ENTRYPOINT ["dotnet", "GoogleCalendarPi.dll"]
