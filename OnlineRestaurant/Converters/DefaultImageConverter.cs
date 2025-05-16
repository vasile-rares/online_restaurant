using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace OnlineRestaurant.Converters
{
    public class DefaultImageConverter : IValueConverter
    {
        public string DefaultImagePath { get; set; } = "/Images/default.jpg";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string imagePath = null;
                
                if (value is ObservableCollection<string> images && images.Count > 0)
                {
                    // Returnează prima imagine din colecție
                    imagePath = images[0];
                    Debug.WriteLine($"Încercăm să încărcăm imaginea: {imagePath}");
                }
                else
                {
                    // Returnează imaginea implicită
                    imagePath = DefaultImagePath;
                    Debug.WriteLine($"Folosim imaginea implicită: {imagePath}");
                }

                // Eliminăm primul slash dacă există, pentru a trata calea relativ la rădăcina aplicației
                if (imagePath.StartsWith("/"))
                {
                    imagePath = imagePath.Substring(1);
                }

                // Verifică dacă fișierul există
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                Debug.WriteLine($"Calea completă a imaginii: {fullPath}");
                
                if (File.Exists(fullPath))
                {
                    Debug.WriteLine("Fișierul imaginii există!");
                    return new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                }
                else
                {
                    Debug.WriteLine($"ATENȚIE: Fișierul imaginii nu există la calea: {fullPath}");
                    // Încearcă să folosească imaginea implicită dacă prima a eșuat
                    string defaultFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                        DefaultImagePath.TrimStart('/'));
                    
                    if (File.Exists(defaultFullPath))
                    {
                        Debug.WriteLine($"Folosim imaginea implicită: {defaultFullPath}");
                        return new BitmapImage(new Uri(defaultFullPath, UriKind.Absolute));
                    }
                    else
                    {
                        Debug.WriteLine("Nici imaginea implicită nu există!");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la conversia imaginii: {ex.Message}");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nu este necesară conversia inversă
            throw new NotImplementedException();
        }
    }
} 