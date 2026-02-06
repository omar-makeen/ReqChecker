using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ReqChecker.App.Controls;

/// <summary>
/// A premium progress ring control with gradient stroke and percentage display.
/// Supports both determinate (with Progress value) and indeterminate modes.
/// </summary>
public partial class ProgressRing : UserControl
{
    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(
            nameof(Progress),
            typeof(double),
            typeof(ProgressRing),
            new PropertyMetadata(0.0, OnProgressChanged));

    public static readonly DependencyProperty IsIndeterminateProperty =
        DependencyProperty.Register(
            nameof(IsIndeterminate),
            typeof(bool),
            typeof(ProgressRing),
            new PropertyMetadata(false, OnIsIndeterminateChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(ProgressRing),
            new PropertyMetadata("Complete"));

    public static readonly DependencyProperty ShowPercentageProperty =
        DependencyProperty.Register(
            nameof(ShowPercentage),
            typeof(bool),
            typeof(ProgressRing),
            new PropertyMetadata(true, OnShowPercentageChanged));

    public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register(
            nameof(IsRunning),
            typeof(bool),
            typeof(ProgressRing),
            new PropertyMetadata(false, OnIsRunningChanged));

    private Storyboard? _rotationStoryboard;
    private Path? _progressArc;
    private ArcSegment? _arcSegment;
    private PathFigure? _arcFigure;
    private Ellipse? _indeterminateRing;

    /// <summary>
    /// Gets or sets the progress value (0-100).
    /// </summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, Math.Clamp(value, 0, 100));
    }

    /// <summary>
    /// Gets or sets whether the progress ring is in indeterminate mode.
    /// </summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>
    /// Gets or sets the label text shown below the percentage.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show the percentage text.
    /// </summary>
    public bool ShowPercentage
    {
        get => (bool)GetValue(ShowPercentageProperty);
        set => SetValue(ShowPercentageProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the progress ring animation is running.
    /// </summary>
    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public ProgressRing()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Find elements
        _progressArc = FindName("ProgressArc") as Path;
        _arcSegment = FindName("ArcSegment") as ArcSegment;
        _arcFigure = FindName("ArcFigure") as PathFigure;
        _indeterminateRing = FindName("IndeterminateRing") as Ellipse;

        if (FindName("RotationStoryboard") is Storyboard storyboard)
        {
            _rotationStoryboard = storyboard;
        }

        UpdateArc();
        UpdateVisualState();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateArc();
    }

    private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressRing ring)
        {
            ring.UpdateArc();
        }
    }

    private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressRing ring)
        {
            ring.UpdateVisualState();
        }
    }

    private static void OnShowPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressRing ring)
        {
            ring.UpdateVisualState();
        }
    }

    private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressRing ring)
        {
            ring.UpdateAnimationState();
        }
    }

    private void UpdateArc()
    {
        if (_arcSegment == null || _arcFigure == null || _progressArc == null)
            return;

        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 0 || height <= 0)
            return;

        var strokeThickness = 8.0;
        var radius = (Math.Min(width, height) - strokeThickness) / 2;
        var center = new Point(width / 2, height / 2);

        // Handle edge case where progress is 0
        if (Progress <= 0)
        {
            _progressArc.Visibility = Visibility.Collapsed;
            return;
        }

        _progressArc.Visibility = Visibility.Visible;

        // Calculate arc based on progress
        double progressAngle;
        double startAngle;

        // Handle special case for 100% - render full ellipse instead of degenerate arc
        if (Progress >= 100)
        {
            // For a full circle, use a slightly less than 360 degree arc to avoid degenerate geometry
            progressAngle = 359.99;
            startAngle = -90.0;
        }
        else
        {
            progressAngle = (Progress / 100.0) * 360.0;
            startAngle = -90.0;
        }

        // Convert angles to radians
        var startRad = startAngle * Math.PI / 180.0;
        var endRad = (startAngle + progressAngle) * Math.PI / 180.0;

        // Calculate start and end points
        var startPoint = new Point(
            center.X + radius * Math.Cos(startRad),
            center.Y + radius * Math.Sin(startRad));

        var endPoint = new Point(
            center.X + radius * Math.Cos(endRad),
            center.Y + radius * Math.Sin(endRad));

        // Update arc
        _arcFigure.StartPoint = startPoint;
        _arcSegment.Point = endPoint;
        _arcSegment.Size = new Size(radius, radius);
        _arcSegment.IsLargeArc = progressAngle > 180;
    }

    private void UpdateVisualState()
    {
        if (_indeterminateRing != null)
        {
            _indeterminateRing.Visibility = IsIndeterminate ? Visibility.Visible : Visibility.Collapsed;
        }

        if (_progressArc != null)
        {
            _progressArc.Visibility = IsIndeterminate ? Visibility.Collapsed : (Progress > 0 ? Visibility.Visible : Visibility.Collapsed);
        }

        var percentageText = FindName("PercentageText") as TextBlock;
        if (percentageText != null)
        {
            percentageText.Visibility = ShowPercentage && !IsIndeterminate ? Visibility.Visible : Visibility.Collapsed;
        }

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (_rotationStoryboard == null)
            return;

        if (IsRunning && IsIndeterminate)
        {
            _rotationStoryboard.Begin();
        }
        else
        {
            _rotationStoryboard.Stop();
        }
    }

    /// <summary>
    /// Triggers a pulse animation on the progress ring.
    /// </summary>
    public void TriggerPulse()
    {
        var pulseStoryboard = new Storyboard();

        var scaleXAnim = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(scaleXAnim, this);
        Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("RenderTransform.ScaleX"));

        var scaleYAnim = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(scaleYAnim, this);
        Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("RenderTransform.ScaleY"));

        pulseStoryboard.Children.Add(scaleXAnim);
        pulseStoryboard.Children.Add(scaleYAnim);

        // Ensure we have a ScaleTransform
        if (RenderTransform is not ScaleTransform)
        {
            RenderTransform = new ScaleTransform(1, 1);
            RenderTransformOrigin = new Point(0.5, 0.5);
        }

        pulseStoryboard.Begin();
    }
}
