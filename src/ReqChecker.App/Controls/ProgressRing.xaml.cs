using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ReqChecker.App.Controls;

/// <summary>
/// A progress ring control that displays a spinning animation when running
/// </summary>
public partial class ProgressRing : UserControl
{
    public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register(
            nameof(IsRunning),
            typeof(bool),
            typeof(ProgressRing),
            new PropertyMetadata(false, OnIsRunningChanged));

    public new static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(
            nameof(Foreground),
            typeof(Brush),
            typeof(ProgressRing),
            new PropertyMetadata(new SolidColorBrush(Colors.Blue)));

    private Storyboard? _rotationStoryboard;

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public new Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public ProgressRing()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Find the storyboard in resources
        if (FindName("RotationStoryboard") is Storyboard storyboard)
        {
            _rotationStoryboard = storyboard;
            UpdateAnimationState();
        }
    }

    private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressRing ring)
        {
            ring.UpdateAnimationState();
        }
    }

    private void UpdateAnimationState()
    {
        if (_rotationStoryboard == null)
            return;

        if (IsRunning)
        {
            _rotationStoryboard.Begin();
        }
        else
        {
            _rotationStoryboard.Stop();
        }
    }
}
