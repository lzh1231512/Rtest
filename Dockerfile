# 使用官方 Playwright 镜像，包含 SDK 和浏览器
FROM mcr.microsoft.com/playwright/dotnet:latest AS build

WORKDIR /src
COPY . .

# 恢复依赖并编译
RUN dotnet restore
RUN dotnet build -c Release

# 安装 Playwright CLI 并下载浏览器
RUN dotnet new tool-manifest \
 && dotnet tool install Microsoft.Playwright.CLI \
 && dotnet tool run playwright install --with-deps

# 发布应用
RUN dotnet publish -c Release -o /app/out

# 运行阶段，仍使用 Playwright 镜像（因为需要浏览器）
FROM mcr.microsoft.com/playwright/dotnet:latest
WORKDIR /dotnet
COPY --from=build /app/out .
RUN pwsh /dotnet/playwright.ps1 install 
EXPOSE 8080
ENTRYPOINT ["dotnet", "DL91Web8.dll"]