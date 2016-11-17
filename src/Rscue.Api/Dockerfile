FROM microsoft/aspnetcore:1.0.1
ENV ASPNETCORE_URLS="http://*:5000"
ENTRYPOINT ["dotnet", "Rscue.Api.dll"]
ARG source=.
WORKDIR /app
EXPOSE 5000
COPY $source .
