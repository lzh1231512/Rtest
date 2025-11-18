# 使用官方 Playwright 镜像，包含 .NET SDK 和浏览器
FROM mcr.microsoft.com/playwright/dotnet:latest

# 设置工作目录
WORKDIR /dotnet

# 暴露端口
EXPOSE 8080

# 复制已发布的文件（假设你在本地已执行 dotnet publish -c Release -o Release）
COPY ./Release .

# 安装 Playwright 依赖（关键步骤）
RUN pwsh /dotnet/playwright.ps1 install --with-deps

# 设置权限（可选）
RUN chmod -R 777 /dotnet

# 入口点：运行 ASP.NET Core 应用
ENTRYPOINT ["dotnet", "DL91Web8.dll"]