using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using ReqChecker.Core.Enums;

namespace ReqChecker.App.Controls;

/// <summary>
/// A badge control that displays test status with glow effects and icon.
/// </summary>
public partial class TestStatusBadge : Border
{
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(TestStatus),
            typeof(TestStatusBadge),
            new PropertyMetadata(TestStatus.Skipped, OnStatusChanged));

    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(
            nameof(ShowIcon),
            typeof(bool),
            typeof(TestStatusBadge),
            new PropertyMetadata(true, OnShowIconChanged));

    public TestStatus Status
    {
        get => (TestStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public TestStatusBadge()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateStatusDisplay();
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TestStatusBadge badge)
        {
            badge.UpdateStatusDisplay();
        }
    }

    private static void OnShowIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TestStatusBadge badge)
        {
            badge.UpdateIconVisibility();
        }
    }

    private void UpdateStatusDisplay()
    {
        UpdateIconVisibility();
        UpdateGlowEffect();
    }

    private void UpdateIconVisibility()
    {
        var iconTextBlock = FindName("StatusIcon") as TextBlock;
        if (iconTextBlock == null)
            return;

        if (!ShowIcon)
        {
            iconTextBlock.Visibility = Visibility.Collapsed;
            return;
        }

        iconTextBlock.Visibility = Visibility.Visible;
        iconTextBlock.Text = Status switch
        {
            TestStatus.Pass => "\uE73E", // Checkmark
            TestStatus.Fail => "\uE711", // X mark
            TestStatus.Skipped => "\uE72A", // Forward arrow
            _ => "\uE9CE" // Circle
        };
    }

    private void UpdateGlowEffect()
    {
        var glowEffect = FindName("GlowEffect") as DropShadowEffect;
        if (glowEffect == null)
            return;

        glowEffect.Color = Status switch
        {
            TestStatus.Pass => Color.FromRgb(16, 185, 129),   // Green
            TestStatus.Fail => Color.FromRgb(239, 68, 68),    // Red
            TestStatus.Skipped => Color.FromRgb(245, 158, 11),  // Amber
            _ => Color.FromRgb(107, 114, 128)                   // Gray
        };
    }

    /// <summary>
    /// Triggers a pulse animation on the badge.
    /// </summary>
    public void TriggerPulse()
    {
        var scaleTransform = RenderTransform as ScaleTransform;
        if (scaleTransform == null)
        {
            scaleTransform = new ScaleTransform(1, 1);
            RenderTransform = scaleTransform;
            RenderTransformOrigin = new Point(0.5, 0.5);
        }

        var pulseStoryboard = new Storyboard();

        var scaleXAnim = new DoubleAnimation
        {
            From = 1.0,
            To = 1.15,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleXAnim, this);
        Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("RenderTransform.ScaleX"));

        var scaleYAnim = new DoubleAnimation
        {
            From = 1.0,
            To = 1.15,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleYAnim, this);
        Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("RenderTransform.ScaleY"));

        pulseStoryboard.Children.Add(scaleXAnim);
        pulseStoryboard.Children.Add(scaleYAnim);

        // Also pulse the glow
        var glowEffect = FindName("GlowEffect") as DropShadowEffect;
        if (glowEffect != null)
        {
            var glowAnim = new DoubleAnimation
            {
                From = 8,
                To = 20,
                Duration = TimeSpan.FromMilliseconds(150),
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(glowAnim, glowEffect);
            Storyboard.SetTargetProperty(glowAnim, new PropertyPath(DropShadowEffect.BlurRadiusProperty));
            pulseStoryboard.Children.Add(glowAnim);
        }

        pulseStoryboard.Begin();
    }
}
