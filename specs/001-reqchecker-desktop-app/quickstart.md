# Quickstart: ReqChecker Development

**Branch**: `001-reqchecker-desktop-app` | **Date**: 2026-01-30

## Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| Windows | 10/11 | Development and target platform |
| .NET SDK | 8.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Visual Studio | 2022 17.8+ | Or VS Code with C# Dev Kit |
| Git | 2.x | Source control |

**Optional**:
- ReSharper or Rider for enhanced refactoring
- WiX Toolset v4 for installer building

## Initial Setup

### 1. Clone and Restore

```powershell
# Clone repository
git clone <repository-url> ReqChecker
cd ReqChecker

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build
```

### 2. Create Solution Structure

```powershell
# Create solution
dotnet new sln -n ReqChecker

# Create projects
dotnet new classlib -n ReqChecker.Core -o src/ReqChecker.Core -f net8.0
dotnet new classlib -n ReqChecker.Infrastructure -o src/ReqChecker.Infrastructure -f net8.0
dotnet new wpf -n ReqChecker.App -o src/ReqChecker.App -f net8.0-windows

# Create test projects
dotnet new xunit -n ReqChecker.Core.Tests -o tests/ReqChecker.Core.Tests -f net8.0
dotnet new xunit -n ReqChecker.Infrastructure.Tests -o tests/ReqChecker.Infrastructure.Tests -f net8.0
dotnet new xunit -n ReqChecker.App.Tests -o tests/ReqChecker.App.Tests -f net8.0-windows

# Add projects to solution
dotnet sln add src/ReqChecker.Core/ReqChecker.Core.csproj
dotnet sln add src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj
dotnet sln add src/ReqChecker.App/ReqChecker.App.csproj
dotnet sln add tests/ReqChecker.Core.Tests/ReqChecker.Core.Tests.csproj
dotnet sln add tests/ReqChecker.Infrastructure.Tests/ReqChecker.Infrastructure.Tests.csproj
dotnet sln add tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj

# Add project references
dotnet add src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj reference src/ReqChecker.Core/ReqChecker.Core.csproj
dotnet add src/ReqChecker.App/ReqChecker.App.csproj reference src/ReqChecker.Core/ReqChecker.Core.csproj
dotnet add src/ReqChecker.App/ReqChecker.App.csproj reference src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj

# Add test references
dotnet add tests/ReqChecker.Core.Tests/ReqChecker.Core.Tests.csproj reference src/ReqChecker.Core/ReqChecker.Core.csproj
dotnet add tests/ReqChecker.Infrastructure.Tests/ReqChecker.Infrastructure.Tests.csproj reference src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj
dotnet add tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj reference src/ReqChecker.App/ReqChecker.App.csproj
```

### 3. Install NuGet Packages

```powershell
# Core (no external dependencies - only system libraries)
# (intentionally empty)

# Infrastructure
dotnet add src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj package Serilog
dotnet add src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj package Serilog.Sinks.File
dotnet add src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj package FluentValidation
dotnet add src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj package FluentFTP
dotnet add src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj package CsvHelper

# App
dotnet add src/ReqChecker.App/ReqChecker.App.csproj package WPF-UI
dotnet add src/ReqChecker.App/ReqChecker.App.csproj package Microsoft.Extensions.Hosting
dotnet add src/ReqChecker.App/ReqChecker.App.csproj package Microsoft.Extensions.DependencyInjection
dotnet add src/ReqChecker.App/ReqChecker.App.csproj package CommunityToolkit.Mvvm

# Test packages
dotnet add tests/ReqChecker.Core.Tests/ReqChecker.Core.Tests.csproj package FluentAssertions
dotnet add tests/ReqChecker.Core.Tests/ReqChecker.Core.Tests.csproj package Moq
dotnet add tests/ReqChecker.Infrastructure.Tests/ReqChecker.Infrastructure.Tests.csproj package FluentAssertions
dotnet add tests/ReqChecker.Infrastructure.Tests/ReqChecker.Infrastructure.Tests.csproj package Moq
dotnet add tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj package FluentAssertions
dotnet add tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj package Moq
```

## Running the Application

```powershell
# Debug run
dotnet run --project src/ReqChecker.App/ReqChecker.App.csproj

# Or via Visual Studio
# Open ReqChecker.sln → Set ReqChecker.App as startup project → F5
```

## Running Tests

```powershell
# Run all tests
dotnet test

# Run specific project tests
dotnet test tests/ReqChecker.Core.Tests

# Run with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

## Building for Distribution

### Debug Build

```powershell
dotnet build -c Debug
```

### Release Build (Self-Contained)

```powershell
dotnet publish src/ReqChecker.App/ReqChecker.App.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o ./publish
```

### MSI Installer (Requires WiX v4)

```powershell
# Install WiX (once)
dotnet tool install --global wix

# Build installer
wix build -o ReqChecker.msi installer/ReqChecker.wxs
```

## Project Configuration

### Directory.Build.props (Solution Root)

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>12</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

### App.csproj Additions

```xml
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
  <ApplicationIcon>Resources\Icons\app.ico</ApplicationIcon>
  <Version>1.0.0</Version>
  <Company>YourCompany</Company>
  <Product>ReqChecker</Product>
</PropertyGroup>
```

## Development Workflow

### Adding a New Test Type

1. Create test class in `Infrastructure/Tests/`:
   ```csharp
   [TestType("MyNewTest")]
   public class MyNewTest : ITest
   {
       public async Task<TestResult> ExecuteAsync(
           TestDefinition definition,
           CancellationToken cancellationToken)
       {
           // Implementation
       }
   }
   ```

2. Test type is auto-discovered via `[TestType]` attribute at startup

3. Add parameter schema to `contracts/profile-schema.json`

4. Write unit tests in `Infrastructure.Tests/Tests/MyNewTestTests.cs`

### Adding a New View

1. Create ViewModel in `App/ViewModels/`:
   ```csharp
   public partial class MyViewModel : ObservableObject
   {
       [ObservableProperty]
       private string _title = "My View";
   }
   ```

2. Create View in `App/Views/`:
   ```xml
   <Page x:Class="ReqChecker.App.Views.MyView"
         xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml">
       <ui:Card>
           <TextBlock Text="{Binding Title}" />
       </ui:Card>
   </Page>
   ```

3. Register in DI and NavigationService

## Environment Variables

| Variable | Purpose | Default |
|----------|---------|---------|
| `REQCHECKER_LOG_LEVEL` | Minimum log level | `Information` |
| `REQCHECKER_DATA_DIR` | Override AppData location | `%LOCALAPPDATA%\ReqChecker` |

## Troubleshooting

### WPF-UI Not Rendering

Ensure `App.xaml` includes theme dictionaries:
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ui:ThemesDictionary Theme="Dark" />
            <ui:ControlsDictionary />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Tests Hang on CI

Ensure headless mode for WPF tests or skip UI tests in CI:
```powershell
dotnet test --filter "Category!=UI"
```

### Credential Manager Access Denied

Run Visual Studio / terminal as the same user who stored credentials.

## Useful Commands

```powershell
# Clean all build artifacts
dotnet clean && Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue

# Update all packages
dotnet outdated --upgrade

# Format code
dotnet format

# Analyze code
dotnet build /p:EnableNETAnalyzers=true /warnaserror
```

## Documentation Links

- [WPF-UI Documentation](https://wpfui.lepo.co/documentation/)
- [.NET 8 What's New](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [FluentValidation Docs](https://docs.fluentvalidation.net/)
- [Serilog Configuration](https://github.com/serilog/serilog/wiki/Configuration-Basics)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
