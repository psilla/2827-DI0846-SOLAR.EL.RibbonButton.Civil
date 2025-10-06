using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TYPSA.SharedLib.Civil.UserForms;
using SOLAR.EL.RibbonButton.Civil.Buttons;

namespace SOLAR.EL.RibbonButton.Revit.UserForms
{
    public class TextBoxForm_Solar : Form
    {
        private Label header;
        private Button btnNext;

        public Dictionary<string, string> salida = null;
        private Dictionary<string, TextBox> propertyTextBoxes = new Dictionary<string, TextBox>();

        public TextBoxForm_Solar(string mensajeSel)
        {
            this.Text = "Selection Form to choose label prefixes";
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.FormClosing += OnFormClosing;

            var screenSize = Screen.PrimaryScreen.WorkingArea;
            this.Width = screenSize.Width / 3;
            int spacing = 25;
            int uiWidth = this.ClientSize.Width;
            int uiHeight = this.ClientSize.Height;

            header = Clases.label_Header(mensajeSel, spacing);
            this.Controls.Add(header);

            int xPropiedad = 30;
            int xTextBox = xPropiedad + 500;   // separación fija entre propiedad y textbox
            int yOffset = header.Location.Y + header.Height + spacing;

            // Obtenemos settings
            SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();

            // Lista de 4 propiedades con valor por defecto
            List<(string propiedad, string valorDefecto)> props =
                new List<(string, string)>
            {
                (solarSet.ContGenProp, "P"),
                (solarSet.ContInvProp, "INV"),
                (solarSet.TrackProp, "TR"),
                (solarSet.StringProp, "S"),
            };

            foreach (var campo in props)
            {
                // Label de la propiedad
                Label labelPropiedad = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Text = campo.propiedad,
                    Location = new Point(xPropiedad, yOffset)
                };
                this.Controls.Add(labelPropiedad);

                // TextBox asociado
                TextBox textBox = ClasesSOLAR.textBox_SOLAR(150, labelPropiedad);
                textBox.Location = new Point(xTextBox, yOffset);
                textBox.Text = campo.valorDefecto;
                this.Controls.Add(textBox);

                // Guardamos referencia
                propertyTextBoxes[campo.propiedad] = textBox;

                yOffset += Math.Max(labelPropiedad.Height, textBox.Height) + spacing;
            }

            btnNext = Clases.button_Next(uiWidth, spacing, uiHeight);
            btnNext.Click += NextButtonPressed;
            btnNext.Location = new Point(this.ClientSize.Width - btnNext.Width - 30,
                                         this.ClientSize.Height - btnNext.Height - 30);
            this.Controls.Add(btnNext);

            int totalAlturaNecesaria = yOffset + btnNext.Height + spacing;
            this.Height = Math.Min(totalAlturaNecesaria, screenSize.Height - 100);

            this.AcceptButton = btnNext;
        }

        private void NextButtonPressed(object sender, EventArgs e)
        {
            salida = new Dictionary<string, string>();

            foreach (var pair in propertyTextBoxes)
            {
                string propertyName = pair.Key;
                string textBoxValue = pair.Value.Text.Trim();
                salida[propertyName] = textBoxValue;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (salida == null)
            {
                MessageBox.Show(
                    "The form has been closed without saving the data.",
                    "Warning"
                );
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TextBoxForm_Solar
            // 
            this.ClientSize = new System.Drawing.Size(292, 212);
            this.Name = "TextBoxForm_Solar";
            this.Load += new System.EventHandler(this.TextBoxForm_Solar_Load);
            this.ResumeLayout(false);

        }

        private void TextBoxForm_Solar_Load(object sender, EventArgs e)
        {

        }




    }
}
