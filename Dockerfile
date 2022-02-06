FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
RUN sed -i 's/deb.debian.org/mirrors.ustc.edu.cn/g' /etc/apt/sources.list \
    && apt-get update \
    && apt-get install -y ffmpeg \
    && apt-get clean && apt-get autoclean && apt-get autoremove \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /dotnet
EXPOSE 5005
COPY ./Release .
ENTRYPOINT ["dotnet", "DL91Web.dll"]
