using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;

namespace ReqChecker.App.Controls;

/// <summary>
/// A summary card control for displaying stats with icon, value, and label.
/// </summary>
public partial class SummaryCard : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(SummaryCard),
            new PropertyMetadata("0"));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(SummaryCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(SymbolRegular),
            typeof(SummaryCard),
            new PropertyMetadata(SymbolRegular.Info24));

    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(
            nameof(ShowIcon),
            typeof(bool),
            typeof(SummaryCard),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AccentColorProperty =
        DependencyProperty.Register(
            nameof(AccentColor),
            typeof(Color),
            typeof(SummaryCard),
            new PropertyMetadata(Color.FromRgb(0, 217, 255), OnAccentColorChanged));

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(
            nameof(AccentBrush),
            typeof(Brush),
            typeof(SummaryCard),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 217, 255))));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public SymbolRegular Icon
    {
        get => (SymbolRegular)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public SummaryCard()
    {
        InitializeComponent();
    }

    private static void OnAccentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SummaryCard card && e.NewValue is Color color)
        {
            card.AccentBrush = new SolidColorBrush(color);
        }
    }

    /// <summary>
    /// Triggers a count-up animation for the value.
    /// </summary>
    public void AnimateValue(int targetValue, TimeSpan duration)
    {
        if (FindName("ValueText") is not System.Windows.Controls.TextBlock valueText)
            return;

        var startValue = 0;
        if (int.TryParse(Value, out var currentValue))
        {
            startValue = currentValue;
        }

        var animation = new Int32Animation
        {
            From = startValue,
            To = targetValue,
            Duration = duration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();

        // We'll use a timer-based approach since TextBlock.Text isn't animatable
        var startTime = DateTime.Now;
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60fps
        };

        timer.Tick += (s, e) =>
        {
            var elapsed = DateTime.Now - startTime;
            var progress = Math.Min(1.0, elapsed.TotalMilliseconds / duration.TotalMilliseconds);

            // Apply easing
            var easedProgress = 1 - Math.Pow(1 - progress, 2);
            var currentAnimValue = (int)(startValue + (targetValue - startValue) * easedProgress);

            Value = currentAnimValue.ToString();

            if (progress >= 1.0)
            {
                timer.Stop();
                Value = targetValue.ToString();
            }
        };

        timer.Start();
    }

    /// <summary>
    /// Triggers a pulse animation on the card.
    /// </summary>
    public void TriggerPulse()
    {
        if (FindName("CardBorder") is not Border cardBorder)
            return;

        var scaleTransform = cardBorder.RenderTransform as ScaleTransform;
        if (scaleTransform == null)
        {
            scaleTransform = new ScaleTransform(1, 1);
            cardBorder.RenderTransform = scaleTransform;
        }

        var pulseStoryboard = new Storyboard();

        var scaleXAnim = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleXAnim, cardBorder);
        Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("RenderTransform.ScaleX"));

        var scaleYAnim = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleYAnim, cardBorder);
        Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("RenderTransform.ScaleY"));

        pulseStoryboard.Children.Add(scaleXAnim);
        pulseStoryboard.Children.Add(scaleYAnim);

        pulseStoryboard.Begin();
    }
}
