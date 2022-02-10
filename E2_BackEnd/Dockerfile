#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["E2_BackEnd/E2_BackEnd.csproj", "E2_BackEnd/"]
RUN dotnet restore "E2_BackEnd/E2_BackEnd.csproj"
COPY . .
WORKDIR "/src/E2_BackEnd"
RUN dotnet build "E2_BackEnd.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "E2_BackEnd.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "E2_BackEnd.dll"]