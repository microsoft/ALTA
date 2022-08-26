FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder

# build the test files
COPY ["/Microsoft.ALTA.SampleTest/Microsoft.ALTA.SampleTest/", "Microsoft.ALTA.SampleTest/Microsoft.ALTA.SampleTest/"]
RUN dotnet restore "Microsoft.ALTA.SampleTest/Microsoft.ALTA.SampleTest/Microsoft.ALTA.SampleTest.csproj"
WORKDIR /Microsoft.ALTA
COPY Microsoft.ALTA /ALTA
WORKDIR /Microsoft.ALTA.SampleTest/Microsoft.ALTA.SampleTest
RUN dotnet build "Microsoft.ALTA.SampleTest.csproj" -c Release -o /app

#build ALTA
COPY ["/Microsoft.ALTA/Microsoft.ALTA/Microsoft.ALTA.csproj", "Microsoft.ALTA/Microsoft.ALTA/"]
RUN dotnet restore "/ALTA/Microsoft.ALTA/Microsoft.ALTA.csproj"
WORKDIR /ALTA/Microsoft.ALTA/Microsoft.ALTA
RUN dotnet build "/ALTA/Microsoft.ALTA/Microsoft.ALTA.csproj" -c Release -o /app
RUN dotnet publish "/ALTA/Microsoft.ALTA/Microsoft.ALTA.csproj" -c Release -o /app

# copy over to mariner
FROM mcr.microsoft.com/dotnet/aspnet:6.0-cbl-mariner2.0 as runtime

WORKDIR /app

COPY --from=builder /app .
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Microsoft.ALTA.dll"]
