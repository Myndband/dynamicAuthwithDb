# Author: Shilratna Dharmarakshak Chawhan
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet dev-certs https --trust
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
#RUN apk --no-cache add curl
COPY --from=build /app/out .
#EXPOSE 5066 
ENTRYPOINT ["dotnet", "DotnetAuthAndFileHandling.dll"]
