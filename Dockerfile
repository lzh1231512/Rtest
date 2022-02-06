FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /dotnet
EXPOSE 5005
COPY ./Release .
ENTRYPOINT ["dotnet", "DL91Web.dll"]
