using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ReqChecker.Core.Interfaces;
using ReqChecker.Infrastructure.Tests;
using ReqChecker.Infrastructure.Execution;
using ReqChecker.Infrastructure.ProfileManagement;
using ReqChecker.Infrastructure.ProfileManagement.Migrations;
using ReqChecker.Infrastructure.Export;
using ReqChecker.Infrastructure.Logging;
using ReqChecker.Infrastructure.Security;
using ReqChecker.App.ViewModels;
using ReqChecker.App.Services;

namespace ReqChecker.App;

/// <summary>
/// Application entry point with DI container setup.
/// </summary>
public partial class App : System.Windows.Application
{
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    private static string AppDataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ReqChecker");

    public App()
    {
        // Configure logging first
        SerilogConfiguration.Configure(AppDataPath);

        // Configure dependency injection
        ConfigureServices();
    }

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure app state with logs path
        var appState = Services.GetRequiredService<IAppState>();
        appState.SetLogsPath(Path.Combine(AppDataPath, "Logs"));

        // Initialize theme service
        var themeService = Services.GetRequiredService<ThemeService>();

        // Register global exception handler
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        // Log the exception
        Serilog.Log.Error(e.Exception, "Unhandled dispatcher exception: {Message}", e.Exception.Message);

        // Show user-friendly error message
        ShowErrorDialog(
            "An unexpected error occurred",
            "The application encountered an error and needs to close. " +
            "Please check the logs folder for more details.",
            e.Exception);

        // Prevent default unhandled exception processing
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // Log the exception
        Serilog.Log.Error(e.ExceptionObject as Exception, "Unhandled domain exception: {Message}",
            e.ExceptionObject?.ToString() ?? "Unknown error");

        // Show user-friendly error message
        if (e.IsTerminating)
        {
            ShowErrorDialog(
                "Critical Error",
                "The application encountered a critical error and will close. " +
                "Please check the logs folder for more details.",
                e.ExceptionObject as Exception);
        }
    }

    private void ShowErrorDialog(string title, string message, Exception? exception)
    {
        var errorMessage = message;
        if (exception != null)
        {
            errorMessage += $"\n\nError: {exception.Message}";
        }

        System.Windows.MessageBox.Show(
            errorMessage,
            title,
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error);
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register app state (singleton for shared state across ViewModels)
        services.AddSingleton<IAppState, AppState>();

        // Register profile management services
        services.AddSingleton<IProfileLoader, JsonProfileLoader>();
        services.AddSingleton<IProfileValidator, FluentProfileValidator>();
        services.AddSingleton<IProfileMigrator, ProfileMigrationPipeline>();
        services.AddSingleton<V1ToV2Migration>();
        // Note: HmacIntegrityVerifier is optional (null for user-provided profiles)

        // Register test implementations via attribute discovery
        var testAssembly = Assembly.GetAssembly(typeof(PingTest));
        if (testAssembly != null)
        {
            var testTypes = testAssembly.GetTypes()
                .Where(t => typeof(ITest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var testType in testTypes)
            {
                services.AddSingleton(typeof(ITest), testType);
            }
        }

        // Register credential provider for secure credential storage
        services.AddSingleton<ICredentialProvider, WindowsCredentialProvider>();

        // Register test runner with credential provider
        services.AddSingleton<ITestRunner>(sp =>
        {
            var tests = sp.GetServices<ITest>();
            var credentialProvider = sp.GetRequiredService<ICredentialProvider>();
            return new SequentialTestRunner(tests, credentialProvider);
        });

        // Register exporters (both as interface for collection resolution and concrete for direct injection)
        services.AddSingleton<JsonExporter>();
        services.AddSingleton<CsvExporter>();
        services.AddSingleton<IExporter>(sp => sp.GetRequiredService<JsonExporter>());
        services.AddSingleton<IExporter>(sp => sp.GetRequiredService<CsvExporter>());

        // Register services
        services.AddSingleton<NavigationService>(sp =>
            new NavigationService(sp));
        services.AddSingleton<DialogService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IPreferencesService, PreferencesService>();
        services.AddSingleton<ThemeService>(sp =>
            new ThemeService(sp.GetRequiredService<IPreferencesService>()));

        // Register ViewModels
        services.AddTransient<MainViewModel>(sp =>
            new MainViewModel(sp.GetRequiredService<IPreferencesService>()));
        services.AddTransient<ProfileSelectorViewModel>();
        services.AddTransient<TestListViewModel>();
        services.AddTransient<RunProgressViewModel>();
        services.AddTransient<ResultsViewModel>();
        services.AddTransient<DiagnosticsViewModel>();

        // Build service provider
        Services = services.BuildServiceProvider();
    }
}
