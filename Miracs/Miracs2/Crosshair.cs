using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows;

namespace Miracs2
{
    public partial class Crosshair : Form//, IMessageFilter
    {
        public int nheight = 0;
        public int nwidth = 0;
        private CrosshairType CurrentType;

        public Crosshair()
        {
            InitializeComponent();
            this.TransparencyKey = Color.Turquoise;
            this.BackColor = Color.Turquoise;
            this.TopMost = true;
            //DisableMouse();
        }

        public void ChangeType(CrosshairType type)
        {
            int currentWidth = this.Size.Width;
            int currentHeight = this.Size.Height;

            switch (type)
            {
                case CrosshairType.normal:
                    CurrentType = type;
                    this.Location = new Point(955, 486);
                    this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2 + 3,
                              (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2 - 46);
                    break;
                case CrosshairType.centro:
                    CurrentType = type;
                    this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2,
                          (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2);
                    break;
                case CrosshairType.diferente:
                    CurrentType = type;
                    this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2 + nwidth,
                          (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2 - nheight);
                    break;
                case CrosshairType.HellNotLoose:
                    CurrentType = type;
                    this.Location = new Point((((Screen.PrimaryScreen.WorkingArea.Width) / 2) - 3), ((Screen.PrimaryScreen.WorkingArea.Height) / 2) + 20);
                    break;
                case CrosshairType.Cursor:
                    CurrentType = type;
                    this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2 + nwidth,
                          ((Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2 - nheight) + 23);
                    break;
                case CrosshairType.Decrease:
                    Size size = new Size(currentWidth - 1, currentHeight - 1);
                    this.MinimumSize = size;
                    this.MaximumSize = size;
                    ChangeType(CurrentType);
                    break;
                case CrosshairType.Increase:
                    Size sizee = new Size(currentWidth + 1, currentHeight + 1);
                    this.MaximumSize = sizee;
                    this.MinimumSize = sizee;
                    ChangeType(CurrentType);
                    break;
                default:
                    break;
            }

            GC.Collect();
        }

        //Rectangle OldRect = Rectangle.Empty;
        //private void EnableMouse()
        //{
        //    Cursor.Clip = OldRect;
        //    Cursor.Show();
        //    Application.RemoveMessageFilter(this);
        //}

        //public bool PreFilterMessage(ref Message m)
        //{
        //    if (m.Msg == 0x201 || m.Msg == 0x202 || m.Msg == 0x203) return true;
        //    if (m.Msg == 0x204 || m.Msg == 0x205 || m.Msg == 0x206) return true;
        //    return false;
        //}

        //private void DisableMouse()
        //{
        //    OldRect = Cursor.Clip;
        //    // Arbitrary location.
        //    Rectangle BoundRect = new Rectangle(50, 50, 1, 1);
        //    Cursor.Clip = BoundRect;
        //    //Cursor.Hide();
        //    Application.AddMessageFilter(this);
        //}
    }
}
