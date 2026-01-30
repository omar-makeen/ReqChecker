using Microsoft.Extensions.DependencyInjection;
using ReqChecker.Core.Interfaces;
using ReqChecker.Infrastructure.Tests;
using ReqChecker.Infrastructure.Execution;
using ReqChecker.App.ViewModels;
using System.Reflection;

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

    public App()
    {
        // Configure dependency injection
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

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

        // Register test runner
        services.AddSingleton<ITestRunner>(sp =>
        {
            var tests = sp.GetServices<ITest>();
            return new SequentialTestRunner(tests);
        });

        // Register services
        services.AddSingleton<Services.NavigationService>(sp =>
            new Services.NavigationService(sp));
        services.AddSingleton<Services.DialogService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<TestListViewModel>();
        services.AddTransient<RunProgressViewModel>();
        services.AddTransient<ResultsViewModel>();

        // Build service provider
        Services = services.BuildServiceProvider();
    }
}
