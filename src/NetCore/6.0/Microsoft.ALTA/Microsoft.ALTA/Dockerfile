FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
WORKDIR /hardcodedapi/HardCodedTestApi
COPY ["/HardCodedTestApi/HardCodedTestApi.csproj", "HardCodedTestApi/"]
COPY ["/HardCodedTestApi.Tests/HardCodedTestApi.Tests.csproj", "HardCodedTestApi.Tests/"]
RUN dotnet restore "HardCodedTestApi/HardCodedTestApi.csproj" --runtime linux-x64
COPY . .
WORKDIR "/hardcodedapi/HardCodedTestApi/HardCodedTestApi"
RUN dotnet publish "HardCodedTestApi.csproj" -c Release -o /app


FROM mcr.microsoft.com/dotnet/aspnet:6.0-cbl-mariner2.0 AS runtime

# copy in the built application
WORKDIR /app
COPY --from=builder /app .
USER root
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "HardCodedTestApi.dll"]