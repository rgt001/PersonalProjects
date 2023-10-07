namespace Miracs2
{
    partial class Principal
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.inputText = new System.Windows.Forms.TextBox();
            this.btnnormal = new System.Windows.Forms.Button();
            this.btncentro = new System.Windows.Forms.Button();
            this.btndiferente = new System.Windows.Forms.Button();
            this.btndois = new System.Windows.Forms.Button();
            this.btnnenhum = new System.Windows.Forms.Button();
            this.btnMagnifier = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // inputText
            // 
            this.inputText.Location = new System.Drawing.Point(12, 12);
            this.inputText.Multiline = true;
            this.inputText.Name = "inputText";
            this.inputText.Size = new System.Drawing.Size(264, 64);
            this.inputText.TabIndex = 2;
            // 
            // btnnormal
            // 
            this.btnnormal.Enabled = false;
            this.btnnormal.Location = new System.Drawing.Point(13, 82);
            this.btnnormal.Name = "btnnormal";
            this.btnnormal.Size = new System.Drawing.Size(69, 31);
            this.btnnormal.TabIndex = 3;
            this.btnnormal.Text = "Normal";
            this.btnnormal.UseVisualStyleBackColor = true;
            this.btnnormal.Click += new System.EventHandler(this.btnnormal_Click);
            // 
            // btncentro
            // 
            this.btncentro.Location = new System.Drawing.Point(109, 82);
            this.btncentro.Name = "btncentro";
            this.btncentro.Size = new System.Drawing.Size(70, 31);
            this.btncentro.TabIndex = 4;
            this.btncentro.Text = "Centro";
            this.btncentro.UseVisualStyleBackColor = true;
            this.btncentro.Click += new System.EventHandler(this.btncentro_Click);
            // 
            // btndiferente
            // 
            this.btndiferente.Enabled = false;
            this.btndiferente.Location = new System.Drawing.Point(206, 82);
            this.btndiferente.Name = "btndiferente";
            this.btndiferente.Size = new System.Drawing.Size(70, 31);
            this.btndiferente.TabIndex = 5;
            this.btndiferente.Text = "Diferente";
            this.btndiferente.UseVisualStyleBackColor = true;
            this.btndiferente.Click += new System.EventHandler(this.btndiferente_Click);
            // 
            // btndois
            // 
            this.btndois.Enabled = false;
            this.btndois.Location = new System.Drawing.Point(13, 136);
            this.btndois.Name = "btndois";
            this.btndois.Size = new System.Drawing.Size(69, 31);
            this.btndois.TabIndex = 6;
            this.btndois.Text = "Duplicate";
            this.btndois.UseVisualStyleBackColor = true;
            this.btndois.Click += new System.EventHandler(this.btndois_Click);
            // 
            // btnnenhum
            // 
            this.btnnenhum.Enabled = false;
            this.btnnenhum.Location = new System.Drawing.Point(109, 136);
            this.btnnenhum.Name = "btnnenhum";
            this.btnnenhum.Size = new System.Drawing.Size(70, 31);
            this.btnnenhum.TabIndex = 7;
            this.btnnenhum.Text = "Nenhum";
            this.btnnenhum.UseVisualStyleBackColor = true;
            this.btnnenhum.Click += new System.EventHandler(this.btnnenhum_Click);
            // 
            // btnMagnifier
            // 
            this.btnMagnifier.Location = new System.Drawing.Point(206, 136);
            this.btnMagnifier.Name = "btnMagnifier";
            this.btnMagnifier.Size = new System.Drawing.Size(70, 31);
            this.btnMagnifier.TabIndex = 9;
            this.btnMagnifier.Text = "Magnifier";
            this.btnMagnifier.UseVisualStyleBackColor = true;
            this.btnMagnifier.Click += new System.EventHandler(this.btnMagnifier_Click);
            // 
            // Principal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 177);
            this.Controls.Add(this.btnMagnifier);
            this.Controls.Add(this.btnnenhum);
            this.Controls.Add(this.btndois);
            this.Controls.Add(this.btndiferente);
            this.Controls.Add(this.btncentro);
            this.Controls.Add(this.btnnormal);
            this.Controls.Add(this.inputText);
            this.Name = "Principal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mira";
            this.Load += new System.EventHandler(this.Principal_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox inputText;
        private System.Windows.Forms.Button btnnormal;
        private System.Windows.Forms.Button btncentro;
        private System.Windows.Forms.Button btndiferente;
        private System.Windows.Forms.Button btndois;
        private System.Windows.Forms.Button btnnenhum;
        private System.Windows.Forms.Button btnMagnifier;
    }
}