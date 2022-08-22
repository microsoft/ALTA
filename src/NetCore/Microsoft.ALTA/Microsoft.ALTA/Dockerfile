FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
WORKDIR /src/NetCore/Microsoft.ALTA/Microsoft.ALTA
COPY ["Microsoft.ALTA.csproj"]
RUN dotnet restore "Microsoft.ALTA.csproj" --runtime linux-x64
COPY . .
RUN dotnet publish "Microsoft.ALTA.csproj" -c Release -o /app


FROM mcr.microsoft.com/dotnet/aspnet:6.0-cbl-mariner2.0 AS runtime

# copy in the built application
WORKDIR /app
COPY --from=builder /app .
USER root
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Microsoft.ALTA.dll"]
