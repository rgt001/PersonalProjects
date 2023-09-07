using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miracs2
{
    class Class1
    {
        private void DrawImage(Graphics g)
        {
            Image image = this.Enabled ? ImageEnabled : ((ImageDisabled != null) ? ImageDisabled : ImageEnabled);
            ImageAttributes imageAttr = null;
            if (null == image)
                return;
            if (m_monochrom)
            {
                imageAttr = new ImageAttributes();
                // transform the monochrom image                // white -> BackColor                // black -> ForeColor                ColorMap[] colorMap = new ColorMap[2];
                colorMap[0] = new ColorMap();
                colorMap[0].OldColor = Color.White;
                colorMap[0].NewColor = this.BackColor;
                colorMap[1] = new ColorMap();
                colorMap[1].OldColor = Color.Black;
                colorMap[1].NewColor = this.ForeColor;
                imageAttr.SetRemapTable(colorMap);
            }
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            if ((!Enabled) && (null == ImageDisabled))
            {
                using (Bitmap bitmapMono = new Bitmap(image, ClientRectangle.Size))
                {
                    if (imageAttr != null)
                    {
                        using (Graphics gMono = Graphics.FromImage(bitmapMono))
                        {
                            gMono.DrawImage(image, new Point[3] { new Point(0, 0), new Point(image.Width - 1, 0), new Point(0, image.Height - 1) }, rect, GraphicsUnit.Pixel, imageAttr);
                        }
                    }
                    ControlPaint.DrawImageDisabled(g, bitmapMono, 0, 0, this.BackColor);
                }
            }
            else
            {
                // Three points provided are upper-left, upper-right and                 // lower-left of the destination parallelogram.                 Point[] pts = new Point[3];
                pts[0].X = (Enabled && m_mouseOver && m_mouseCapture) ? 1 : 0;
                pts[0].Y = (Enabled && m_mouseOver && m_mouseCapture) ? 1 : 0;
                pts[1].X = pts[0].X + ClientRectangle.Width;
                pts[1].Y = pts[0].Y;
                pts[2].X = pts[0].X;
                pts[2].Y = pts[1].Y + ClientRectangle.Height;
                if (imageAttr == null)
                    g.DrawImage(image, pts, rect, GraphicsUnit.Pixel);
                else g.DrawImage(image, pts, rect, GraphicsUnit.Pixel, imageAttr);
            }
        }
    }
}
