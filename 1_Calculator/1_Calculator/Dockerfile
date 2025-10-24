FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["1_Calculator.csproj", "."］
RUN dotnet restore "./1_Calculator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./1_Calculator.csproj" -C %BUILD_CONFIGURATION% —o /app/build
# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=ReLease
RUN dotnet publish "./1_Calculator.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p: UseAppHost=false
# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме Спо умолчанию, когда конфигурация отладки не используется)
FROM base AS finall
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "1_Calculator.dll"]