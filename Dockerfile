# 1. Pull the official Microsoft .NET 10 SDK image to compile your code
FROM ://microsoft.com AS build
WORKDIR /src

# 2. Copy your files into the container and build the binaries
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# 3. Pull the lightweight .NET 10 ASP.NET runtime image to execute it
FROM ://microsoft.com AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# 4. Bind to Render's required web server port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 5. Boot up your API (Configured matching your StoreApi.csproj file)
ENTRYPOINT ["dotnet", "StoreApi.dll"]
