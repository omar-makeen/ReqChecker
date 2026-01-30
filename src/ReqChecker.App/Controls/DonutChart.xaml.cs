using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ReqChecker.App.Controls;

/// <summary>
/// A donut chart control for displaying pass/fail/skip ratios with draw-in animation.
/// </summary>
public partial class DonutChart : UserControl
{
    private const double StrokeThickness = 12;
    private readonly List<Path> _segmentPaths = new();

    public static readonly DependencyProperty PassedProperty =
        DependencyProperty.Register(
            nameof(Passed),
            typeof(int),
            typeof(DonutChart),
            new PropertyMetadata(0, OnValuesChanged));

    public static readonly DependencyProperty FailedProperty =
        DependencyProperty.Register(
            nameof(Failed),
            typeof(int),
            typeof(DonutChart),
            new PropertyMetadata(0, OnValuesChanged));

    public static readonly DependencyProperty SkippedProperty =
        DependencyProperty.Register(
            nameof(Skipped),
            typeof(int),
            typeof(DonutChart),
            new PropertyMetadata(0, OnValuesChanged));

    public static readonly DependencyProperty TotalProperty =
        DependencyProperty.Register(
            nameof(Total),
            typeof(int),
            typeof(DonutChart),
            new PropertyMetadata(0));

    public static readonly DependencyProperty CenterLabelProperty =
        DependencyProperty.Register(
            nameof(CenterLabel),
            typeof(string),
            typeof(DonutChart),
            new PropertyMetadata("Total"));

    public static readonly DependencyProperty AnimateDurationProperty =
        DependencyProperty.Register(
            nameof(AnimateDuration),
            typeof(TimeSpan),
            typeof(DonutChart),
            new PropertyMetadata(TimeSpan.FromMilliseconds(800)));

    public int Passed
    {
        get => (int)GetValue(PassedProperty);
        set => SetValue(PassedProperty, value);
    }

    public int Failed
    {
        get => (int)GetValue(FailedProperty);
        set => SetValue(FailedProperty, value);
    }

    public int Skipped
    {
        get => (int)GetValue(SkippedProperty);
        set => SetValue(SkippedProperty, value);
    }

    public int Total
    {
        get => (int)GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }

    public string CenterLabel
    {
        get => (string)GetValue(CenterLabelProperty);
        set => SetValue(CenterLabelProperty, value);
    }

    public TimeSpan AnimateDuration
    {
        get => (TimeSpan)GetValue(AnimateDurationProperty);
        set => SetValue(AnimateDurationProperty, value);
    }

    public DonutChart()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateChart(animate: true);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateChart(animate: false);
    }

    private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DonutChart chart && chart.IsLoaded)
        {
            chart.Total = chart.Passed + chart.Failed + chart.Skipped;
            chart.UpdateChart(animate: true);
        }
    }

    private void UpdateChart(bool animate)
    {
        if (FindName("ChartCanvas") is not Canvas canvas)
            return;

        // Clear existing segments
        canvas.Children.Clear();
        _segmentPaths.Clear();

        var total = Passed + Failed + Skipped;
        if (total == 0)
        {
            Total = 0;
            return;
        }

        Total = total;

        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;
        var radius = Math.Min(centerX, centerY) - StrokeThickness / 2;

        var segments = new List<(int value, Color color)>
        {
            (Passed, Color.FromRgb(16, 185, 129)),   // Green
            (Failed, Color.FromRgb(239, 68, 68)),    // Red
            (Skipped, Color.FromRgb(245, 158, 11))   // Amber
        };

        var startAngle = -90.0; // Start from top

        foreach (var (value, color) in segments)
        {
            if (value <= 0)
                continue;

            var sweepAngle = (double)value / total * 360;
            var path = CreateArcPath(centerX, centerY, radius, startAngle, sweepAngle, color);
            canvas.Children.Add(path);
            _segmentPaths.Add(path);

            if (animate)
            {
                AnimateSegment(path, sweepAngle);
            }

            startAngle += sweepAngle;
        }
    }

    private Path CreateArcPath(double centerX, double centerY, double radius,
        double startAngle, double sweepAngle, Color color)
    {
        var path = new Path
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = StrokeThickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = color,
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.4
            }
        };

        UpdateArcGeometry(path, centerX, centerY, radius, startAngle, sweepAngle);

        return path;
    }

    private static void UpdateArcGeometry(Path path, double centerX, double centerY,
        double radius, double startAngle, double sweepAngle)
    {
        var startAngleRad = startAngle * Math.PI / 180;
        var endAngleRad = (startAngle + sweepAngle) * Math.PI / 180;

        var startX = centerX + radius * Math.Cos(startAngleRad);
        var startY = centerY + radius * Math.Sin(startAngleRad);
        var endX = centerX + radius * Math.Cos(endAngleRad);
        var endY = centerY + radius * Math.Sin(endAngleRad);

        var isLargeArc = sweepAngle > 180;

        var pathGeometry = new PathGeometry();
        var figure = new PathFigure
        {
            StartPoint = new Point(startX, startY),
            IsClosed = false
        };

        var arcSegment = new ArcSegment
        {
            Point = new Point(endX, endY),
            Size = new Size(radius, radius),
            IsLargeArc = isLargeArc,
            SweepDirection = SweepDirection.Clockwise
        };

        figure.Segments.Add(arcSegment);
        pathGeometry.Figures.Add(figure);
        path.Data = pathGeometry;
    }

    private void AnimateSegment(Path path, double targetSweepAngle)
    {
        // Get the current path geometry info
        if (path.Data is not PathGeometry geometry ||
            geometry.Figures.Count == 0 ||
            geometry.Figures[0].Segments.Count == 0)
            return;

        var figure = geometry.Figures[0];
        var startPoint = figure.StartPoint;

        // Calculate center and radius from start point
        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;
        var radius = Math.Min(centerX, centerY) - StrokeThickness / 2;

        // Calculate start angle
        var startAngle = Math.Atan2(startPoint.Y - centerY, startPoint.X - centerX) * 180 / Math.PI;

        // Animate using a timer
        var startTime = DateTime.Now;
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        timer.Tick += (s, e) =>
        {
            var elapsed = DateTime.Now - startTime;
            var progress = Math.Min(1.0, elapsed.TotalMilliseconds / AnimateDuration.TotalMilliseconds);

            // Apply easing (ease out)
            var easedProgress = 1 - Math.Pow(1 - progress, 3);
            var currentSweep = targetSweepAngle * easedProgress;

            UpdateArcGeometry(path, centerX, centerY, radius, startAngle, Math.Max(0.1, currentSweep));

            if (progress >= 1.0)
            {
                timer.Stop();
            }
        };

        timer.Start();
    }

    /// <summary>
    /// Triggers the draw-in animation.
    /// </summary>
    public void Animate()
    {
        UpdateChart(animate: true);
    }
}
