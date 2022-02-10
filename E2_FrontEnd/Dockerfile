#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["E2_FrontEnd/E2_FrontEnd.csproj", "E2_FrontEnd/"]
RUN dotnet restore "E2_FrontEnd/E2_FrontEnd.csproj"
COPY . .
WORKDIR "/src/E2_FrontEnd"
RUN dotnet build "E2_FrontEnd.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "E2_FrontEnd.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "E2_FrontEnd.dll"]