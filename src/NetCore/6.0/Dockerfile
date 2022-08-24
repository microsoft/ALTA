FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
WORKDIR /source/repos/ALTA/src/NetCore/Microsoft.ALTA
COPY ["/Microsoft.ALTA/Microsoft.ALTA.csproj", "Microsoft.ALTA/"]
RUN dotnet restore "Microsoft.ALTA/Microsoft.ALTA.csproj"
COPY . .
RUN dotnet build "Microsoft.ALTA.csproj" -c Release -o /app
RUN dotnet publish "Microsoft.ALTA.csproj" -c Release -o /app
COPY ["/Microsoft.ALTA.SampleTest/Microsoft.ALTA.SampleTest/bin/Debug/net6.0/*.dll", "/app"]
WORKDIR /source/repos/ALTA/src/NetCore/Microsoft.ALTA/Microsoft.ALTA



FROM mcr.microsoft.com/dotnet/aspnet:6.0-cbl-mariner2.0 as runtime

# copy in the built application
WORKDIR /app

COPY --from=builder /app .
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Microsoft.ALTA.dll"]