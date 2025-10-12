FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MeetingApp.Api/MeetingApp.Api.csproj", "MeetingApp.Api/"]
COPY ["MeetingApp.Business/MeetingApp.Business.csproj", "MeetingApp.Business/"]
COPY ["MeetingApp.DataAccess/MeetingApp.DataAccess.csproj", "MeetingApp.DataAccess/"]
COPY ["MeetingApp.Models/MeetingApp.Models.csproj", "MeetingApp.Models/"]

RUN dotnet restore "MeetingApp.Api/MeetingApp.Api.csproj"

COPY . .
WORKDIR "/src/MeetingApp.Api"
RUN dotnet build "MeetingApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MeetingApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY MeetingApp.Api/EmailTemplates ./EmailTemplates

ENTRYPOINT ["dotnet", "MeetingApp.Api.dll"]