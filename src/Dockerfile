FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

ARG VERSION=1.0.0

# Copy csproj and restore as distinct layers
COPY *.sln */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ./${file%.*}/ && mv $file ./${file%.*}/; done
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN echo $VERSION
# Publish
RUN dotnet publish Billy/Billy.csproj -c Release -o /app/out -p:Version=$VERSION

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 5000
ENTRYPOINT [ "dotnet", "Billy.dll", "--urls", "http://0.0.0.0:5000"]