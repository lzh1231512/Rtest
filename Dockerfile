FROM mcr.microsoft.com/playwright/dotnet:latest AS build
WORKDIR /src
COPY . .

RUN dotnet restore
RUN dotnet build -c Release
RUN dotnet tool run playwright install --with-deps
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /dotnet
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DL91Web8.dll"]