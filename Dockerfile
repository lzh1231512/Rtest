FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /dotnet
EXPOSE 8080
COPY ./Release .
RUN chmod -R 777 /dotnet
ENTRYPOINT ["dotnet", "DL91Web8.dll"]