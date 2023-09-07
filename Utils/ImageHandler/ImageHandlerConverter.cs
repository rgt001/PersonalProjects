using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Utils.ImageHandler
{
    public static class ImageHandlerConverter
    {
        public static Bitmap ResizeImage(string imagePath)
        {
            Image image = Image.FromFile(imagePath);

            int width = image.Width;
            int height = image.Height;

            if (width > 1200 || height > 1200)
            {
                if (height > width)
                {
                    double diff = 1200d / height;
                    height = 1200;
                    width = (int)(width * diff);
                }
                else
                {
                    double diff = 1200d / width;
                    width = 1200;
                    height = (int)(height * diff);
                }
            }

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, PixelFormat.Format16bppRgb565);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static byte[] ToByteArray(this Image img)
        {
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    img.Save(mStream, img.RawFormat);
                    return mStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void ToImageFile(this byte[] array, string path)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(array))
                {
                    string name = Path.GetRandomFileName();
                    string file = Path.Combine(path, name);
                    FileStream fileS = new FileStream(file, FileMode.Create, FileAccess.Write);
                    ms.WriteTo(fileS);
                    fileS.Close();
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] StringToByteArray(this string source)
        {
            return Encoding.ASCII.GetBytes(source);
        }

        public static string ByteArrayToString(this byte[] source)
        {
            return Encoding.ASCII.GetString(source);
        }
    }
}
