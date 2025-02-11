using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;


namespace Cursor_Customize_Real
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public string selectedFilePath = "";
        public string destinationCurPath = @"C:\cursor.cur";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog.Filter = "PNG files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true) 
            {
                // Save the selected file path to a variable
                selectedFilePath = openFileDialog.FileName;
                if(PNGCheck(selectedFilePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFilePath, UriKind.Absolute);
                    bitmap.EndInit();

                    Upload.Source = bitmap;
                    Error.Content = "";
                }
                else
                {
                    Error.Content = "Please ONLY upload a PNG or CUR file";
                }
        
            }
        }

        private void Button_Clicked(object sender, RoutedEventArgs e)
        {
            string destinationFilePath = @"C:\Windows\Cursors\aero_arrow.cur";
            if (File.Exists(destinationFilePath) && PNGCheck(selectedFilePath))
            {
                BitmapImage bitmapImage = new BitmapImage(new Uri(selectedFilePath));
                Bitmap bitmap = BitmapImage2Bitmap(bitmapImage);
                using (FileStream fs = new FileStream(destinationCurPath,FileMode.Create, FileAccess.Write))
                {
                    WriteIconDir(fs);
                    WriteIconDirEntry(fs, bitmap);
                    WriteBitmapData(fs, bitmap);
                }

                Process.Start("control.exe", "main.cpl,,1");

            }
        }

        public bool PNGCheck(string File)
        {
            if (System.IO.Path.GetExtension(File).ToLower() == ".png" || System.IO.Path.GetExtension(File).ToLower() == ".cur")
                return true;
            else
            {
                return false;
            }
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder(); // Use PngEncoder for better compatibility
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(outStream);

                Bitmap bitmap = new Bitmap(outStream);
                Bitmap bitmapWithAlpha = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(bitmapWithAlpha))
                {
                    g.DrawImage(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                }

                return bitmapWithAlpha; // Return the properly formatted bitmap
            }
        }

        static void WriteIconDir(FileStream fs)
        {
            fs.Write(BitConverter.GetBytes((short)0), 0, 2); // Reserved, must be 0
            fs.Write(BitConverter.GetBytes((short)2), 0, 2); // Type, 2 for cursor
            fs.Write(BitConverter.GetBytes((short)1), 0, 2); // Image count, 1 for single image
        }

        static void WriteIconDirEntry(FileStream fs, Bitmap bitmap)
        {
            byte width = (byte)(bitmap.Width == 256 ? 0 : bitmap.Width);
            byte height = (byte)(bitmap.Height == 256 ? 0 : bitmap.Height);
            byte colorCount = 0;
            byte reserved = 0;
            short xHotspot = (short)(bitmap.Width / 2);
            short yHotspot = (short)(bitmap.Height / 2);
            int sizeInBytes = (bitmap.Width * bitmap.Height * 4) + 40;
            int imageOffset = 22;

            fs.WriteByte(width);
            fs.WriteByte(height);
            fs.WriteByte(colorCount);
            fs.WriteByte(reserved);
            fs.Write(BitConverter.GetBytes(xHotspot), 0, 2);
            fs.Write(BitConverter.GetBytes(yHotspot), 0, 2);
            fs.Write(BitConverter.GetBytes(sizeInBytes), 0, 4);
            fs.Write(BitConverter.GetBytes(imageOffset), 0, 4);
        }
        static void WriteBitmapData(FileStream fs, Bitmap bitmap)
        {
            fs.Write(BitConverter.GetBytes(40), 0, 4); // Header size
            fs.Write(BitConverter.GetBytes(bitmap.Width), 0, 4); // Width
            fs.Write(BitConverter.GetBytes(bitmap.Height * 2), 0, 4) ; // Height 
            fs.Write(BitConverter.GetBytes((short)1), 0, 2); // Color planes, must be 1
            fs.Write(BitConverter.GetBytes((short)32), 0, 2); // Bit count (32bpp)
            fs.Write(BitConverter.GetBytes(0), 0, 4); // Compression, 0 for BI_RGB
            fs.Write(BitConverter.GetBytes(bitmap.Width * bitmap.Height * 4), 0, 4); // Image size, size of raw bitmap data
            fs.Write(BitConverter.GetBytes(0), 0, 4); // Horizontal resolution
            fs.Write(BitConverter.GetBytes(0), 0, 4); // Vertical resolution
            fs.Write(BitConverter.GetBytes(0), 0, 4); // Number of colors in the color palette
            fs.Write(BitConverter.GetBytes(0), 0, 4); // Important colors used

            // Write the pixel data
            System.Drawing.Color[,] pixelArray = GetPixelArray(bitmap);
            for (int y = pixelArray.GetLength(1) - 1; y >= 0; y--)
            {
                for (int x = 0; x < pixelArray.GetLength(0); x++)
                {
                    System.Drawing.Color pixel = pixelArray[x, y];
                    fs.WriteByte(pixel.B);
                    fs.WriteByte(pixel.G);
                    fs.WriteByte(pixel.R);
                    fs.WriteByte(pixel.A);


                }
            }


        }

        static System.Drawing.Color[,] GetPixelArray(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            System.Drawing.Color[,] pixelArray = new System.Drawing.Color[width, height];

            // Iterate through each pixel
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get the pixel color
                    pixelArray[x, y] = bitmap.GetPixel(x, y);
                }
            }

            return pixelArray;
        }

        
    }
}
