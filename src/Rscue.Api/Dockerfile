FROM microsoft/aspnetcore:latest
ENTRYPOINT ["dotnet", "Rscue.Api.dll"]
ARG source=.
WORKDIR /app
EXPOSE 5000
COPY $source .
