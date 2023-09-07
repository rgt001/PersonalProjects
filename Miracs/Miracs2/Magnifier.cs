using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Miracs2
{
    public partial class Magnifier : Form
    {
        public Magnifier()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        const int ScaleFactor = 2;
        const int endWidth = 800 * ScaleFactor;
        const int endHeight = 600 * ScaleFactor;
        const int startWidth = endWidth / ScaleFactor;
        const int startHeight = endHeight / ScaleFactor;
        //const int endWidthDivided = (startWidth / 4);
        //const int endHeightDivided = (startHeight / 4);
        Size size = new Size(endWidth, endHeight);
        Point pointCross = new Point();

        int count = 0;

        Task worker;
        Task janitor;

        private void Magnifier_Load(object sender, EventArgs e)
        {
            //this.pictureBox1.Visible = false;
            TopMost = true;
            pointCross = new Point((Screen.PrimaryScreen.WorkingArea.Width) / 2, ((Screen.PrimaryScreen.WorkingArea.Height) / 2) + 23);
            pointCross.X = pointCross.X - (this.Width / 4);
            pointCross.Y = (pointCross.Y - (this.Height / 4)) - 23;

            worker = Task.Factory.StartNew(() => WorkerGDI());
            //janitor = Task.Factory.StartNew(() => Janitor());
            //Thread.Sleep(4);
            //var worker2 = Task.Factory.StartNew(() => WorkerGDI());

            Task.Factory.StartNew(() =>
            {
                int fps = 0;
                int currentcount;
                int currentcount2;
                MethodInvoker labelChange = delegate { lblFPS.Text = fps.ToString(); };
                do
                {
                    currentcount = count;
                    Thread.Sleep(1000);
                    currentcount2 = count;
                    fps = currentcount2 - currentcount;
                    this.lblFPS.Invoke(labelChange);
                } while (true);
            });
        }

        private void WorkerGDI()
        {
            Graphics GraphicsVariable;
            Stopwatch stopwatch = new Stopwatch();
            Bitmap ScreenBMP = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            ScreenBMP = new Bitmap(startWidth, startHeight, PixelFormat.Format32bppPArgb);
            GraphicsVariable = Graphics.FromImage(ScreenBMP);
            GraphicsVariable.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            GraphicsVariable.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            GraphicsVariable.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;

            #region FuturoTest
            //IntPtr desktopPtr = GetDC(IntPtr.Zero);
            //Graphics g = Graphics.FromHdc(desktopPtr);
            //Graphics OutputBMP = Graphics.FromHdc(desktopPtr);
            //desktopPtr = this.Handle;
            //OutputBMP.CopyFromScreen(pointCross.X, pointCross.Y, 0, 0, size);
            //OutputBMP.DrawImage(ScreenBMP, 0, 0, endWidth, endHeight);
            //OutputBMP.CopyFromScreen(pointCross.X, pointCross.Y, 0, 0, size);
            //OutputBMP.DrawImage(ScreenBMP, pointCross.X, pointCross.Y, pointCross.X + endWidth, pointCross.Y + endHeight);
            #endregion

            MethodInvoker pictureChange = delegate { this.pictureBox1.Image = ScreenBMP; };
            while (true)
            {
                GraphicsVariable.CopyFromScreen(pointCross.X, pointCross.Y, 0, 0, size);
                GraphicsVariable.DrawImage(ScreenBMP, 0, 0, endWidth, endHeight);
                this.Invoke(pictureChange);
                Interlocked.Increment(ref count);
            }
        }

        private void Janitor()
        {
            while (true)
            {
                GC.Collect();
                Thread.Sleep(60_000);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Magnifier_FormClosing(object sender, FormClosingEventArgs e)
        {
            //worker.Dispose();
            //janitor.Dispose();
        }

        //private void cbmFps_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    timer.Stop();
        //    switch (cbmFps.SelectedValue)
        //    {
        //        case "wtf":
        //            timer.Interval = 8;
        //            break;
        //        case "60":
        //            timer.Interval = 16;
        //            break;
        //        case "30":
        //            timer.Interval = 33;
        //            break;
        //        case "24":
        //            timer.Interval = 42;
        //            break;
        //        default:
        //            timer.Interval = 16;
        //            break;
        //    }
        //    timer.Start();
        //}
    }
}
