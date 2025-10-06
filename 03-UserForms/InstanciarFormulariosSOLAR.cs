using System.Collections.Generic;
using System.Windows.Forms;

namespace SOLAR.EL.RibbonButton.Revit.UserForms
{
    internal class InstanciarFormulariosSOLAR
    {
        public static Dictionary<string, string>
            TextBoxFormOut_Solar(string mensaje)
        {
            using (TextBoxForm_Solar ventana = new TextBoxForm_Solar(mensaje))
            {
                return ventana.ShowDialog() == DialogResult.OK ? ventana.salida : null;
            }
        }




    }
}
