FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["auth-system.csproj", "./"]
RUN dotnet restore "auth-system.csproj"

COPY . .
RUN dotnet build "auth-system.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "auth-system.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "auth-system.dll"]
