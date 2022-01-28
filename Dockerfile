FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /dotnet
COPY ./Release .
ENTRYPOINT ["dotnet", "DL91.dll"]
