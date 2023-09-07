using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Miracs2
{
    public partial class Principal : Form
    {
        List<Crosshair> Crosshairs = new List<Crosshair>();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public Principal()
        {
            InitializeComponent();

            int id = (int)CrosshairType.normal;     // The id of the hotkey.
            RegisterHotKey(this.Handle, id, (int)KeyModifier.Alt, Keys.A.GetHashCode());
            id = (int)CrosshairType.centro;
            RegisterHotKey(this.Handle, id, (int)KeyModifier.Alt, Keys.S.GetHashCode());       // Register Alt + S as global hotkey.
            id = (int)CrosshairType.dois;
            RegisterHotKey(this.Handle, id, (int)KeyModifier.Alt, Keys.D.GetHashCode());
            id = (int)CrosshairType.clear;
            RegisterHotKey(this.Handle, id, (int)KeyModifier.Alt, Keys.Q.GetHashCode());
            //id = (int)CrosshairTypes.diferente;
            //RegisterHotKey(this.Handle, id, (int)KeyModifier.Alt, Keys.W.GetHashCode());
            id = (int)CrosshairType.HellNotLoose;
            RegisterHotKey(this.Handle, id, (int)KeyModifier.Alt, Keys.G.GetHashCode());
            id = (int)CrosshairType.Decrease;
            RegisterHotKey(this.Handle, id, (int)KeyModifier.Alt, Keys.Down.GetHashCode());
            id = (int)CrosshairType.Increase;
            RegisterHotKey(this.Handle, id, (int)KeyModifier.Alt, Keys.Up.GetHashCode());
            id = (int)CrosshairType.Magnifier;
            RegisterHotKey(this.Handle, id, (int)KeyModifier.Alt, Keys.W.GetHashCode());
        }

        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        private void WriteOnTextBox(string text)
        {
            inputText.Text += text;
            inputText.Text += Environment.NewLine;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                //Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                //KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.

                switch ((CrosshairType)id)
                {
                    case CrosshairType.normal:
                        CloseAll();
                        btnnormal.PerformClick();
                        break;
                    case CrosshairType.centro:
                        CloseAll();
                        btncentro.PerformClick();
                        break;
                    case CrosshairType.dois:
                        CloseAll();
                        btndois.PerformClick();
                        break;
                    case CrosshairType.HellNotLoose:
                        CloseAll();
                        InstantiateNewCrosshair(CrosshairType.HellNotLoose);
                        break;
                    case CrosshairType.Decrease:
                        ModifyExistingCrosshair(CrosshairType.Decrease);
                        break;
                    case CrosshairType.Increase:
                        ModifyExistingCrosshair(CrosshairType.Increase);
                        break;
                    case CrosshairType.Magnifier:
                        OpenOrCloseMagnifier();
                        break;
                    case CrosshairType.clear:
                        CloseAll();
                        break;
                    default:
                        break;
                }
            }
        }

        private void OpenOrCloseMagnifier()
        {
            FormCollection fc = Application.OpenForms;

            foreach (Form frm in fc)
            {
                try
                {
                    if (frm.Name == "Magnifier")
                    {
                        frm.Close();
                        return;
                    }
                }
                catch (Exception)
                {
                    frm.Close();
                    return;
                }
                
            }

            Magnifier magnifier = new Magnifier();
            magnifier.Show();
            Point teste;
            if (Screen.AllScreens.Length > 1)
            {
                teste = Screen.AllScreens[1].WorkingArea.Location;
                teste.Y = teste.Y + (magnifier.Height / 2);

                if (teste.X < 0)
                {
                    teste.X = -magnifier.Width;
                }

                magnifier.Location = teste;
            }
        }

        private void OpenOrCloseScreenDuplicate()
        {
            FormCollection fc = Application.OpenForms;

            foreach (Form frm in fc)
            {
                try
                {
                    if (frm.Name == "ScreenDuplicate")
                    {
                        frm.Close();
                        return;
                    }
                }
                catch (Exception)
                {
                    frm.Close();
                    return;
                }

            }

            //this.Hide();
            ScreenDuplicate screenDuplicate = new ScreenDuplicate();
            screenDuplicate.Show();
        }

        private void btnnormal_Click(object sender, EventArgs e)
        {
            InstantiateNewCrosshair(CrosshairType.normal);
        }

        private void CloseAll()
        {
            foreach (var form in Crosshairs)
            {
                form.Close();
                form.Dispose();
            }

            Crosshairs.Clear();
        }

        private void btncentro_Click(object sender, EventArgs e)
        {
            InstantiateNewCrosshair(CrosshairType.centro);
        }

        private void ModifyExistingCrosshair(CrosshairType type)
        {
            foreach (var crosshair in Crosshairs)
            {
                crosshair.ChangeType(type);
            }
        }

        private void btndiferente_Click(object sender, EventArgs e)
        {
            #region Variaveis
            string height = "";
            string width = "";
            int pvirgula = 0;
            string texto = inputText.Lines.LastOrDefault();
            inputText.Text = texto;
            #endregion

            try
            {
                while (height != ";" && pvirgula <= 25)
                {
                    if (pvirgula == 0)
                    {
                        height = texto.Substring(0, 1);
                    }
                    else
                    {
                        height = texto.Substring(pvirgula, 1);
                    }
                    pvirgula++;
                }
                inputText.Text = "";//Limpar a text para poder inserir os textos.
                width = texto.Substring(0, pvirgula - 1);//pegando o numero antes do ponto e virgula
                height = texto.Substring(pvirgula, texto.Length - pvirgula);//pegando o numero antes do ponto e virgula
                WriteOnTextBox("Altura:" + height);
                WriteOnTextBox("Largura:" + width);

                InstantiateNewCrosshair(CrosshairType.diferente, int.Parse(width), int.Parse(height));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Test" + ex);
            }
        }

        private void PQP_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < Enum.GetNames(typeof(CrosshairType)).Length; i++)
            {
                UnregisterHotKey(this.Handle, i);
            }
        }

        private void btndois_Click(object sender, EventArgs e)
        {
            OpenOrCloseScreenDuplicate();
            //InstantiateNewCrosshair(CrosshairType.centro);
            //InstantiateNewCrosshair(CrosshairType.normal);
        }

        private void InstantiateNewCrosshair(CrosshairType type)
        {
            Crosshair crosshair = new Crosshair();
            crosshair.Show();
            crosshair.ChangeType(type);
            Crosshairs.Add(crosshair);
        }

        private void InstantiateNewCrosshair(CrosshairType type, int width, int height)
        {
            Crosshair crosshair = new Crosshair();
            crosshair.nheight = height;
            crosshair.nwidth = width;
            crosshair.ChangeType(type);
            crosshair.Show();
            Crosshairs.Add(crosshair);
        }

        private void btnMagnifier_Click(object sender, EventArgs e)
        {
            OpenOrCloseMagnifier();
        }

        private void Principal_Load(object sender, EventArgs e)
        {

        }

        private void btnnenhum_Click(object sender, EventArgs e)
        {
            MapAnalyzer mapAnalyzer = new MapAnalyzer();
            mapAnalyzer.Show();
        }
    }
}
