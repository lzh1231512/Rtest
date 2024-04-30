FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /dotnet
EXPOSE 5005
COPY ./Release .
ENTRYPOINT ["dotnet", "DL91Web.dll"]
