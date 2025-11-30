using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Emgu.CV.Structure;
using Emgu.CV;

namespace aoci_lab2
{
    public partial class MainWindow : Window
    {

        // --- Код повторяется из лабораторной работы #1 ---

        private Image<Bgr, byte> sourceImage;
        public MainWindow()
        {
            InitializeComponent();
        }

        public BitmapSource ToBitmapSource(Image<Bgr, byte> image)
        {
            var mat = image.Mat;

            return BitmapSource.Create(
                mat.Width,
                mat.Height,
                96d,
                96d,
                PixelFormats.Bgr24,
                null,
                mat.DataPointer,
                mat.Step * mat.Height,
                mat.Step);
        }
        public Image<Bgr, byte> ToEmguImage(BitmapSource source)
        {
            if (source == null) return null;

            FormatConvertedBitmap safeSource = new FormatConvertedBitmap();
            safeSource.BeginInit();
            safeSource.Source = source;
            safeSource.DestinationFormat = PixelFormats.Bgr24;
            safeSource.EndInit();

            Image<Bgr, byte> resultImage = new Image<Bgr, byte>(safeSource.PixelWidth, safeSource.PixelHeight);
            var mat = resultImage.Mat;

            safeSource.CopyPixels(
                new System.Windows.Int32Rect(0, 0, safeSource.PixelWidth, safeSource.PixelHeight), 
                mat.DataPointer, 
                mat.Step * mat.Height,
                mat.Step); 

            return resultImage;
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы изображений (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                sourceImage = new Image<Bgr, byte>(openFileDialog.FileName);

                MainImage.Source = ToBitmapSource(sourceImage);
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource currentWpfImage = MainImage.Source as BitmapSource;
            if (currentWpfImage == null)
            {
                MessageBox.Show("Отсутсвует изображение");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|Bitmap Image (*.bmp)|*.bmp";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    Image<Bgr, byte> imageToSave = ToEmguImage(currentWpfImage);
                    imageToSave.Save(saveFileDialog.FileName);

                    MessageBox.Show($"Изображение успешно сохранено в {saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Ошибка! Не могу сохранить файл. Подробности: {ex.Message}");
                }
            }
        }


        //Обрабатывает изменение значений слайдеров, работая в цветовом пространстве HSV.
        private void OnHSVFilterChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sourceImage == null) return;

            //Конвертация из BGR в HSV

            // HSV (Hue, Saturation, Value) разделяет цвет на:
            // H - Цветовой тон (какой это цвет: красный, зеленый, синий). Диапазон 0-179 в EmguCV.
            // S - Насыщенность (насколько цвет "чистый" или "бледный"). 0-255.
            // V - Значение/Яркость (насколько цвет темный или светлый). 0-255.

            Image<Hsv, byte> hsvImage = sourceImage.Convert<Hsv, byte>();

            double hsvBrightness = BrightnessHSVSlider.Value;

            for (int y = 0; y < hsvImage.Rows; y++)
            {
                for (int x = 0; x < hsvImage.Cols; x++)
                {
                    //Получение пикселя изображения в качестве обьекта класса HSV
                    Hsv pixel = hsvImage[y, x];

                    //Изменение яркости
                    double v = pixel.Value;
                    pixel.Value = (byte)Math.Max(0, Math.Min(255, v + hsvBrightness));

                    //Запись нового значения пикселя
                    hsvImage[y, x] = pixel;
                }
            }

            //ВАЖНО: Конвертируем обратно в BGR, чтобы WPF мог отобразить изображение.
            MainImage.Source = ToBitmapSource(hsvImage.Convert<Bgr, byte>());
        }
    }
}
