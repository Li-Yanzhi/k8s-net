FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY ./k8s-net/k8s-net.csproj .
RUN dotnet restore k8s-net.csproj

COPY ./k8s-net .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "k8s-net.dll"]