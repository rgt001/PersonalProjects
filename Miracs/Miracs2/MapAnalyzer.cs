using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Miracs2
{
    public partial class MapAnalyzer : Form
    {
        public MapAnalyzer()
        {
            InitializeComponent();
        }

        private void MapAnalyzer_Load(object sender, EventArgs e)
        {
            DoCoisas();
        }

        private void DoCoisas()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Bitmap sourceImage = (Bitmap)Bitmap.FromFile(@"F:\Recognize\Source.png");
            Bitmap target = (Bitmap)Bitmap.FromFile(@"F:\Recognize\Target.png");

            byte[][] sourceBytes = GetBytes(sourceImage);
            byte[][] targetBytes = GetBytes(target);

            int result = 0;
            int current = 0;
            for (int i = 0; i < sourceBytes.Length; i++)
            {
                result = SearchBytes(sourceBytes[i], targetBytes[current]);

                if (result == -1)
                {
                    current = 0;
                }
                else
                {
                    current++;

                    if (current == targetBytes.Length)
                        break;
                }
            }
            stopwatch.Stop();
            MessageBox.Show("Time:" + stopwatch.ElapsedMilliseconds);
        }

        private int SearchBytes(byte[] source, byte[] target)
        {
            var targetLen = target.Length;
            var sourceLen = source.Length - targetLen;
            int k;
            for (var i = 0; i + targetLen <= sourceLen; i++)
            {
                k = 0;
                for (; k < targetLen; k++)
                {
                    if (target[k] != source[i + k])
                        break;
                }
                if (k == targetLen) 
                    return i;
            }
            return -1;
        }

        public byte[][] GetBytes(Bitmap sourceImage)
        {
            Rectangle rect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                sourceImage.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                sourceImage.PixelFormat);
            
            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride);
            byte[][] rgbValues = new byte[sourceImage.Height][];
            for (int i = 0; i < sourceImage.Height; i++)
            {
                rgbValues[i] = new byte[bytes];
                Marshal.Copy(ptr, rgbValues[i], 0, bytes);
                ptr = IntPtr.Add(ptr, bytes);
            }

            return rgbValues;
        }
    }
}
