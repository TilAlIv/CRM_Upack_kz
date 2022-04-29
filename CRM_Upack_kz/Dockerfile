FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CRM_Upack_kz/CRM_Upack_kz.csproj", "CRM_Upack_kz/"]
RUN dotnet restore "CRM_Upack_kz/CRM_Upack_kz.csproj"
COPY . .
WORKDIR "/src/CRM_Upack_kz"
RUN dotnet build "CRM_Upack_kz.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CRM_Upack_kz.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CRM_Upack_kz.dll"]
