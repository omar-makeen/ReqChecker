using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ReqChecker.App.Controls;

/// <summary>
/// A line chart control for displaying pass rate trends over time.
/// </summary>
public partial class LineChart : UserControl
{
    private const double PointRadius = 5;
    private const double LineThickness = 3;
    private const double ChartPadding = 40;
    
    private readonly List<Ellipse> _pointEllipses = new();
    private readonly List<Line> _gridLines = new();
    private readonly List<TextBlock> _axisLabels = new();
    private Path? _linePath;

    public static readonly DependencyProperty DataPointsProperty =
        DependencyProperty.Register(
            nameof(DataPoints),
            typeof(IEnumerable<ChartDataPoint>),
            typeof(LineChart),
            new PropertyMetadata(null, OnDataPointsChanged));

    public static readonly DependencyProperty MinYProperty =
        DependencyProperty.Register(
            nameof(MinY),
            typeof(double),
            typeof(LineChart),
            new PropertyMetadata(0.0, OnDataPointsChanged));

    public static readonly DependencyProperty MaxYProperty =
        DependencyProperty.Register(
            nameof(MaxY),
            typeof(double),
            typeof(LineChart),
            new PropertyMetadata(100.0, OnDataPointsChanged));

    public static readonly DependencyProperty PointColorProperty =
        DependencyProperty.Register(
            nameof(PointColor),
            typeof(Brush),
            typeof(LineChart),
            new PropertyMetadata(Brushes.Blue, OnDataPointsChanged));

    public static readonly DependencyProperty LineColorProperty =
        DependencyProperty.Register(
            nameof(LineColor),
            typeof(Brush),
            typeof(LineChart),
            new PropertyMetadata(Brushes.Blue, OnDataPointsChanged));

    public IEnumerable<ChartDataPoint>? DataPoints
    {
        get => (IEnumerable<ChartDataPoint>?)GetValue(DataPointsProperty);
        set => SetValue(DataPointsProperty, value);
    }

    public double MinY
    {
        get => (double)GetValue(MinYProperty);
        set => SetValue(MinYProperty, value);
    }

    public double MaxY
    {
        get => (double)GetValue(MaxYProperty);
        set => SetValue(MaxYProperty, value);
    }

    public Brush PointColor
    {
        get => (Brush)GetValue(PointColorProperty);
        set => SetValue(PointColorProperty, value);
    }

    public Brush LineColor
    {
        get => (Brush)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    public LineChart()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateChart();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateChart();
    }

    private static void OnDataPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LineChart chart && chart.IsLoaded)
        {
            chart.UpdateChart();
        }
    }

    private void UpdateChart()
    {
        if (FindName("ChartCanvas") is not Canvas canvas)
            return;

        // Clear existing elements
        canvas.Children.Clear();
        _pointEllipses.Clear();
        _gridLines.Clear();
        _axisLabels.Clear();

        var dataPointsList = DataPoints?.ToList() ?? new List<ChartDataPoint>();
        if (dataPointsList.Count == 0)
            return;

        var width = ActualWidth - ChartPadding * 2;
        var height = ActualHeight - ChartPadding * 2;
        var minY = MinY;
        var maxY = MaxY;
        var rangeY = maxY - minY;

        // Calculate chart area
        var chartLeft = ChartPadding;
        var chartRight = ActualWidth - ChartPadding;
        var chartBottom = ActualHeight - ChartPadding;

        // Draw grid lines (horizontal)
        var gridLineCount = 5;
        for (int i = 0; i <= gridLineCount; i++)
        {
            var yValue = minY + (rangeY * i / gridLineCount);
            var yPos = chartBottom - (height * (yValue - minY) / rangeY);

            var gridLine = new Line
            {
                X1 = chartLeft,
                Y1 = yPos,
                X2 = chartRight,
                Y2 = yPos,
                Stroke = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                StrokeThickness = 1
            };
            canvas.Children.Add(gridLine);
            _gridLines.Add(gridLine);

            // Add Y-axis label
            var label = new TextBlock
            {
                Text = $"{yValue:F0}%",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Canvas.SetLeft(label, chartLeft - 5);
            Canvas.SetTop(label, yPos - 6);
            canvas.Children.Add(label);
            _axisLabels.Add(label);
        }

        // Draw line connecting data points
        if (dataPointsList.Count > 1)
        {
            var points = new PointCollection();
            foreach (var dataPoint in dataPointsList)
            {
                var x = chartLeft + (width * (dataPoint.Index / (double)(dataPointsList.Count - 1)));
                var y = chartBottom - (height * (dataPoint.Value - minY) / rangeY);
                points.Add(new Point(x, y));
            }

            var lineGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = points[0],
                IsClosed = false
            };
            figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
            lineGeometry.Figures.Add(figure);

            _linePath = new Path
            {
                Data = lineGeometry,
                Stroke = LineColor,
                StrokeThickness = LineThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            canvas.Children.Add(_linePath);
        }

        // Draw data points
        foreach (var dataPoint in dataPointsList)
        {
            var x = chartLeft + (width * dataPoint.Index / (double)(dataPointsList.Count - 1));
            var y = chartBottom - (height * (dataPoint.Value - minY) / rangeY);

            var ellipse = new Ellipse
            {
                Width = PointRadius * 2,
                Height = PointRadius * 2,
                Fill = PointColor,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2
            };
            Canvas.SetLeft(ellipse, x - PointRadius);
            Canvas.SetTop(ellipse, y - PointRadius);
            canvas.Children.Add(ellipse);
            _pointEllipses.Add(ellipse);

            // Add X-axis label (date or index)
            var label = new TextBlock
            {
                Text = FormatXAxisLabel(dataPoint, dataPointsList.Count),
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, y + PointRadius + 5);
            canvas.Children.Add(label);
            _axisLabels.Add(label);
        }
    }

    private string FormatXAxisLabel(ChartDataPoint dataPoint, int totalPoints)
    {
        // If we have 5 or fewer points, show dates
        // Otherwise, show indices to avoid crowding
        if (totalPoints <= 5)
        {
            return dataPoint.DateTime.ToString("MMM dd");
        }
        else
        {
            return $"#{dataPoint.Index + 1}";
        }
    }

    /// <summary>
    /// Data point for the line chart.
    /// </summary>
    public class ChartDataPoint
    {
        public int Index { get; set; }
        public DateTime DateTime { get; set; }
        public double Value { get; set; }
    }
}
