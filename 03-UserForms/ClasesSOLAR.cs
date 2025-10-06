using System.Drawing;
using System.Windows.Forms;

namespace SOLAR.EL.RibbonButton.Revit.UserForms
{
    internal class ClasesSOLAR : Form
    {
        public static TextBox textBox_SOLAR(int fixedWidth, Label label)
        {
            TextBox textBox = new TextBox
            {
                BackColor = Color.FromArgb(245, 245, 245), // Gris más claro, estilo moderno
                BorderStyle = BorderStyle.FixedSingle,     // Borde fino
                Font = new Font("Segoe UI", 9),            // Fuente moderna y estándar
                Size = new Size(get__width_textbox_SOLAR(fixedWidth), 24), // Más compacto
                Location = get_location_textbox_SOLAR(label)
            };

            return textBox;
        }

        public static int get__width_textbox_SOLAR(int fixedWidth)
        {
            // Devolver el ancho fijo del TextBox
            return fixedWidth;
        }

        public static Point get_location_textbox_SOLAR(Label label)
        {
            // Alinear en X a la derecha del Label con un margen de 10px
            int x = label.Location.X + label.Width + 10;

            // Usar la misma posición Y del Label
            int y = label.Location.Y;

            return new Point(x, y);
        }



    }
}
