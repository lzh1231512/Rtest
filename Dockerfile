FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /dotnet
EXPOSE 8080
COPY ./Release .
ENTRYPOINT ["dotnet", "DL91Web8.dll"]
