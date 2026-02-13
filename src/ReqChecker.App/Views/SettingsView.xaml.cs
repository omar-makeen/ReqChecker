using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Settings page view.
/// </summary>
public partial class SettingsView : Page
{
    private readonly ThemeService _themeService;
    private bool _animationsApplied;

    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Get ThemeService for reduced-motion check
        _themeService = App.Services.GetRequiredService<ThemeService>();
        
        // Subscribe to theme changes to update border indicators
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Initial state
        UpdateThemeBorders(viewModel.CurrentTheme);
        
        // Defer entrance animations until the visual tree is fully loaded
        Loaded += OnViewLoaded;
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
        // Only apply animations once
        if (_animationsApplied) return;
        _animationsApplied = true;
        
        // Apply entrance animations (respecting reduced-motion setting)
        ApplyEntranceAnimations();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.CurrentTheme))
        {
            var viewModel = (SettingsViewModel)sender!;
            UpdateThemeBorders(viewModel.CurrentTheme);
        }
    }

    private void UpdateThemeBorders(AppTheme currentTheme)
    {
        // Update Dark theme border
        DarkThemeBorder.BorderBrush = currentTheme == AppTheme.Dark
            ? (Brush)Application.Current.Resources["AccentPrimary"]
            : Brushes.Transparent;
        
        // Update Light theme border
        LightThemeBorder.BorderBrush = currentTheme == AppTheme.Light
            ? (Brush)Application.Current.Resources["AccentPrimary"]
            : Brushes.Transparent;
    }

    /// <summary>
    /// Applies entrance animations to cards, respecting reduced-motion preferences.
    /// </summary>
    private void ApplyEntranceAnimations()
    {
        // Skip animations if reduced motion is enabled
        if (_themeService.IsReducedMotionEnabled)
        {
            return;
        }

        // Find all cards with AnimatedSettingsCard style and apply animations
        var cards = FindVisualChildren<Border>(this)
            .Where(b => b.Style == (Style)FindResource("AnimatedSettingsCard"))
            .ToList();

        var delay = 0;
        foreach (var card in cards)
        {
            ApplyCardAnimation(card, delay);
            delay += 50; // Stagger by 50ms
        }
    }

    /// <summary>
    /// Applies fade-in and slide-up animation to a single card.
    /// </summary>
    private void ApplyCardAnimation(Border card, int delayMs)
    {
        // Initialize for animation
        card.Opacity = 0;
        card.RenderTransform = new TranslateTransform { Y = 20 };

        var storyboard = new Storyboard();

        // Fade animation
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(fadeAnimation, card);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(nameof(Border.Opacity)));
        storyboard.Children.Add(fadeAnimation);

        // Slide animation
        var slideAnimation = new DoubleAnimation
        {
            From = 20,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(slideAnimation, card);
        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
        storyboard.Children.Add(slideAnimation);

        storyboard.Begin();
    }

    /// <summary>
    /// Finds all visual children of a specific type.
    /// </summary>
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) yield break;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
            {
                yield return typedChild;
            }

            foreach (var grandChild in FindVisualChildren<T>(child))
            {
                yield return grandChild;
            }
        }
    }
}
