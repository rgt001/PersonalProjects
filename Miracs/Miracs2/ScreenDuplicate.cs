using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Miracs2
{
    public partial class ScreenDuplicate : Form
    {
        Bitmap ScreenBMP = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        System.Timers.Timer timer;

        public ScreenDuplicate()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        const int ScaleFactor = 1;
        const int endWidth = 1920;
        const int endHeight = 1080;
        const int startWidth = 1920;
        const int startHeight = 1080;
        Size size = new Size(1920, 1080);
        Point pointCross = new Point();

        int count = 0;

        Task worker;
        Task worker2;
        Task janitor;

        private void Magnifier_Load(object sender, EventArgs e)
        {
            //this.pictureBox1.Visible = false;
            TopMost = true;
            pointCross = new Point(1, 1);

            worker = Task.Factory.StartNew(() => WorkerGDI());
            worker2 = Task.Factory.StartNew(() => WorkerGDI());

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
            Bitmap ScreenBMP = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppPArgb);
            GraphicsVariable = Graphics.FromImage(ScreenBMP);
            GraphicsVariable.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            GraphicsVariable.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            GraphicsVariable.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;

            MethodInvoker pictureChange = delegate { this.pictureBox1.Image = ScreenBMP; };
            while (true)
            {
                try
                {
                    GraphicsVariable.CopyFromScreen(0, 0, 0, 0, size);
                    GraphicsVariable.DrawImage(ScreenBMP, 0, 0);
                    this.Invoke(pictureChange);
                    Interlocked.Increment(ref count);
                }
                catch (Exception){}
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
