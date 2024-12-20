using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LambdaVGames.Windows;
using Microsoft.Win32;

namespace LambdaVGames.Controls;

public partial class ImageControl : UserControl {
    public static DependencyProperty ImageIdProperty = DependencyProperty.Register(
        nameof(ImageId), typeof(int), typeof(ImageControl),
        new PropertyMetadata(-1, OnImageIdChanged));
    
    public static readonly RoutedEvent ImageIdChangedEvent = EventManager.RegisterRoutedEvent("ImageIdChanged", RoutingStrategy.Bubble,
        typeof(ImageIdChangedEventHandler), typeof(ImageControl));

    private static readonly BitmapImage FallbackImage = new(
        new Uri("pack://application:,,,/Resources/MissingImage.png", UriKind.Absolute));

    public int ImageId {
        get => (int)GetValue(ImageIdProperty);
        set => SetValue(ImageIdProperty, value);
    }

    private string Path {
        get => MainWindow.LVG_IMAGES_PATH + $"/image_{ImageId}.png";
    }
    
    public ImageControl() {
        InitializeComponent();
        
        // ReloadImage();
        Image.Source = FallbackImage;
    }

    private void LoadImage_Click(object sender, RoutedEventArgs e) {
        OpenFileDialog dialog = new() {
            Filter = "Image Files|*.png;"
        };

        if (dialog.ShowDialog() == true) {
            if (TryLoadImage(dialog.FileName)) {
                byte[] imgBytes = File.ReadAllBytes(dialog.FileName);
                
                File.WriteAllBytes(Path, imgBytes);
            }
            else {
                ReloadImage();
            }
        }
    }

    private void ReloadImage() {
        try {
            BitmapImage image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(Path, UriKind.Relative);
            image.EndInit();
            
            Image.Source = image;
        }
        catch {
            Image.Source = FallbackImage;
        }
    }
    
    private bool TryLoadImage(string path) {
        try {
            BitmapImage image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            
            Image.Source = image;
            
            return true;
        }
        catch {
            MessageBox.Show($"Failed to load image: {path}", "Unable to load image.", MessageBoxButton.OK, MessageBoxImage.Error);
            
            Image.Source = FallbackImage;
            return false;
        }
    }

    private static void OnImageIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ImageControl imageControl) {
            imageControl.ReloadImage();
        }
    }

    /// <summary>
    /// Cleans up the image of the specified path.
    /// </summary>
    /// <param name="id"></param>
    public static void CleanupImage(int id) {
        File.Delete(MainWindow.LVG_IMAGES_PATH + $"/image_{id}.png");
    }
}