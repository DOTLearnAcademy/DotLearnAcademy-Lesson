FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DotLearn.Lesson/DotLearn.Lesson.csproj", "DotLearn.Lesson/"]
RUN dotnet restore "DotLearn.Lesson/DotLearn.Lesson.csproj"
COPY . .
WORKDIR "/src/DotLearn.Lesson"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotLearn.Lesson.dll"]
