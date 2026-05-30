# Digital Signature Platform -- Complete Solution Scaffolder
$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Scaffolding Digital Signature Platform..." -ForegroundColor Cyan

# 1. Create solution
Write-Host "1. Creating solution..." -ForegroundColor Yellow
& dotnet new sln -n DigitalSignature -f | Out-Null

# 2. Create src projects
Write-Host "2. Creating src projects..." -ForegroundColor Yellow
& dotnet new classlib -n DigitalSignature.Domain -o src/DigitalSignature.Domain -f | Out-Null
& dotnet new classlib -n DigitalSignature.Application -o src/DigitalSignature.Application -f | Out-Null
& dotnet new classlib -n DigitalSignature.Infrastructure -o src/DigitalSignature.Infrastructure -f | Out-Null
& dotnet new webapi -n DigitalSignature.Api -o src/DigitalSignature.Api -f | Out-Null

# 3. Create test projects
Write-Host "3. Creating test projects..." -ForegroundColor Yellow
& dotnet new xunit -n DigitalSignature.Domain.UnitTests -o tests/DigitalSignature.Domain.UnitTests -f | Out-Null
& dotnet new xunit -n DigitalSignature.Application.UnitTests -o tests/DigitalSignature.Application.UnitTests -f | Out-Null
& dotnet new xunit -n DigitalSignature.Infrastructure.IntegrationTests -o tests/DigitalSignature.Infrastructure.IntegrationTests -f | Out-Null
& dotnet new xunit -n DigitalSignature.Api.IntegrationTests -o tests/DigitalSignature.Api.IntegrationTests -f | Out-Null
& dotnet new xunit -n DigitalSignature.ArchitectureTests -o tests/DigitalSignature.ArchitectureTests -f | Out-Null

# 4. Add projects to solution
Write-Host "4. Adding projects to solution..." -ForegroundColor Yellow
& dotnet sln DigitalSignature.sln add src/DigitalSignature.Domain/DigitalSignature.Domain.csproj | Out-Null
& dotnet sln DigitalSignature.sln add src/DigitalSignature.Application/DigitalSignature.Application.csproj | Out-Null
& dotnet sln DigitalSignature.sln add src/DigitalSignature.Infrastructure/DigitalSignature.Infrastructure.csproj | Out-Null
& dotnet sln DigitalSignature.sln add src/DigitalSignature.Api/DigitalSignature.Api.csproj | Out-Null
& dotnet sln DigitalSignature.sln add tests/DigitalSignature.Domain.UnitTests/DigitalSignature.Domain.UnitTests.csproj | Out-Null
& dotnet sln DigitalSignature.sln add tests/DigitalSignature.Application.UnitTests/DigitalSignature.Application.UnitTests.csproj | Out-Null
& dotnet sln DigitalSignature.sln add tests/DigitalSignature.Infrastructure.IntegrationTests/DigitalSignature.Infrastructure.IntegrationTests.csproj | Out-Null
& dotnet sln DigitalSignature.sln add tests/DigitalSignature.Api.IntegrationTests/DigitalSignature.Api.IntegrationTests.csproj | Out-Null
& dotnet sln DigitalSignature.sln add tests/DigitalSignature.ArchitectureTests/DigitalSignature.ArchitectureTests.csproj | Out-Null

# 5. Add project references
Write-Host "5. Setting up references..." -ForegroundColor Yellow
& dotnet add src/DigitalSignature.Application/DigitalSignature.Application.csproj reference src/DigitalSignature.Domain/DigitalSignature.Domain.csproj | Out-Null
& dotnet add src/DigitalSignature.Infrastructure/DigitalSignature.Infrastructure.csproj reference src/DigitalSignature.Domain/DigitalSignature.Domain.csproj | Out-Null
& dotnet add src/DigitalSignature.Infrastructure/DigitalSignature.Infrastructure.csproj reference src/DigitalSignature.Application/DigitalSignature.Application.csproj | Out-Null
& dotnet add src/DigitalSignature.Api/DigitalSignature.Api.csproj reference src/DigitalSignature.Domain/DigitalSignature.Domain.csproj | Out-Null
& dotnet add src/DigitalSignature.Api/DigitalSignature.Api.csproj reference src/DigitalSignature.Application/DigitalSignature.Application.csproj | Out-Null
& dotnet add src/DigitalSignature.Api/DigitalSignature.Api.csproj reference src/DigitalSignature.Infrastructure/DigitalSignature.Infrastructure.csproj | Out-Null

& dotnet add tests/DigitalSignature.Domain.UnitTests/DigitalSignature.Domain.UnitTests.csproj reference src/DigitalSignature.Domain/DigitalSignature.Domain.csproj | Out-Null
& dotnet add tests/DigitalSignature.Application.UnitTests/DigitalSignature.Application.UnitTests.csproj reference src/DigitalSignature.Application/DigitalSignature.Application.csproj | Out-Null
& dotnet add tests/DigitalSignature.Application.UnitTests/DigitalSignature.Application.UnitTests.csproj reference src/DigitalSignature.Domain/DigitalSignature.Domain.csproj | Out-Null
& dotnet add tests/DigitalSignature.Infrastructure.IntegrationTests/DigitalSignature.Infrastructure.IntegrationTests.csproj reference src/DigitalSignature.Infrastructure/DigitalSignature.Infrastructure.csproj | Out-Null
& dotnet add tests/DigitalSignature.Api.IntegrationTests/DigitalSignature.Api.IntegrationTests.csproj reference src/DigitalSignature.Api/DigitalSignature.Api.csproj | Out-Null
& dotnet add tests/DigitalSignature.ArchitectureTests/DigitalSignature.ArchitectureTests.csproj reference src/DigitalSignature.Domain/DigitalSignature.Domain.csproj | Out-Null
& dotnet add tests/DigitalSignature.ArchitectureTests/DigitalSignature.ArchitectureTests.csproj reference src/DigitalSignature.Application/DigitalSignature.Application.csproj | Out-Null
& dotnet add tests/DigitalSignature.ArchitectureTests/DigitalSignature.ArchitectureTests.csproj reference src/DigitalSignature.Infrastructure/DigitalSignature.Infrastructure.csproj | Out-Null
& dotnet add tests/DigitalSignature.ArchitectureTests/DigitalSignature.ArchitectureTests.csproj reference src/DigitalSignature.Api/DigitalSignature.Api.csproj | Out-Null

Write-Host "Done!" -ForegroundColor Green
