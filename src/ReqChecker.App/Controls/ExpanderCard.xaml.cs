using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ReqChecker.Core.Enums;

namespace ReqChecker.App.Controls;

/// <summary>
/// An expandable card control for displaying test results with collapsible details.
/// </summary>
public partial class ExpanderCard : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ExpanderCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(ExpanderCard),
            new PropertyMetadata(null));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(TestStatus),
            typeof(ExpanderCard),
            new PropertyMetadata(TestStatus.Skipped, OnStatusChanged));

    public static readonly DependencyProperty StatusBrushProperty =
        DependencyProperty.Register(
            nameof(StatusBrush),
            typeof(Brush),
            typeof(ExpanderCard),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(107, 114, 128))));

    public static readonly DependencyProperty SummaryProperty =
        DependencyProperty.Register(
            nameof(Summary),
            typeof(string),
            typeof(ExpanderCard),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TechnicalDetailsProperty =
        DependencyProperty.Register(
            nameof(TechnicalDetails),
            typeof(string),
            typeof(ExpanderCard),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(
            nameof(ErrorMessage),
            typeof(string),
            typeof(ExpanderCard),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TestDurationProperty =
        DependencyProperty.Register(
            nameof(TestDuration),
            typeof(TimeSpan),
            typeof(ExpanderCard),
            new PropertyMetadata(TimeSpan.Zero));

    public static readonly DependencyProperty AttemptCountProperty =
        DependencyProperty.Register(
            nameof(AttemptCount),
            typeof(int),
            typeof(ExpanderCard),
            new PropertyMetadata(1));

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(ExpanderCard),
            new PropertyMetadata(false, OnIsExpandedChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public TestStatus Status
    {
        get => (TestStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public Brush StatusBrush
    {
        get => (Brush)GetValue(StatusBrushProperty);
        set => SetValue(StatusBrushProperty, value);
    }

    public string? Summary
    {
        get => (string?)GetValue(SummaryProperty);
        set => SetValue(SummaryProperty, value);
    }

    public string? TechnicalDetails
    {
        get => (string?)GetValue(TechnicalDetailsProperty);
        set => SetValue(TechnicalDetailsProperty, value);
    }

    public string? ErrorMessage
    {
        get => (string?)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public TimeSpan TestDuration
    {
        get => (TimeSpan)GetValue(TestDurationProperty);
        set => SetValue(TestDurationProperty, value);
    }

    public int AttemptCount
    {
        get => (int)GetValue(AttemptCountProperty);
        set => SetValue(AttemptCountProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public ExpanderCard()
    {
        InitializeComponent();
        UpdateStatusBrush();
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ExpanderCard card)
        {
            card.UpdateStatusBrush();
        }
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ExpanderCard card)
        {
            card.AnimateExpansion((bool)e.NewValue);
        }
    }

    private void UpdateStatusBrush()
    {
        StatusBrush = Status switch
        {
            TestStatus.Pass => new SolidColorBrush(Color.FromRgb(16, 185, 129)),    // Green
            TestStatus.Fail => new SolidColorBrush(Color.FromRgb(239, 68, 68)),     // Red
            TestStatus.Skipped => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // Amber
            _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))                  // Gray
        };
    }

    private void HeaderButton_Click(object sender, RoutedEventArgs e)
    {
        IsExpanded = !IsExpanded;
    }

    private void AnimateExpansion(bool expand)
    {
        if (FindName("ContentBorder") is not Border contentBorder)
            return;

        if (FindName("ChevronRotation") is not RotateTransform chevronRotation)
            return;

        var storyboard = new Storyboard();

        // Chevron rotation
        var chevronAnim = new DoubleAnimation
        {
            To = expand ? 180 : 0,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(chevronAnim, chevronRotation);
        Storyboard.SetTargetProperty(chevronAnim, new PropertyPath(RotateTransform.AngleProperty));
        storyboard.Children.Add(chevronAnim);

        if (expand)
        {
            // Show content
            contentBorder.Visibility = Visibility.Visible;

            // Fade in
            var fadeAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeAnim, contentBorder);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(OpacityProperty));
            storyboard.Children.Add(fadeAnim);

            // Slide down
            var slideAnim = new DoubleAnimation
            {
                From = -10,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(slideAnim, contentBorder);
            Storyboard.SetTargetProperty(slideAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(slideAnim);
        }
        else
        {
            // Fade out
            var fadeAnim = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(fadeAnim, contentBorder);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(OpacityProperty));
            storyboard.Children.Add(fadeAnim);

            // Slide up
            var slideAnim = new DoubleAnimation
            {
                To = -10,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(slideAnim, contentBorder);
            Storyboard.SetTargetProperty(slideAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(slideAnim);

            // Hide after animation
            storyboard.Completed += (s, e) =>
            {
                if (!IsExpanded)
                {
                    contentBorder.Visibility = Visibility.Collapsed;
                }
            };
        }

        storyboard.Begin();
    }
}
