using System.Collections.Generic;
using System.Windows.Forms;
using TYPSA.SharedLib.Excel;

namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_14_TryGetCoordFromExcel
    {
        public static Dictionary<string, List<(double X, double Y)>> TryGetCoordFromExcel()
        {
            // Seleccionar el directorio del Excel
            string excelDirectory = cls_00_SelectExcelDirectory.SelectExcelDirectory();
            // Validamos
            if (string.IsNullOrEmpty(excelDirectory)) return null;
           
            // Seleccionar el archivo de Excel
            string excelPath = cls_00_SelectExcelFile.SelectExcelFile(excelDirectory);
            // Validamos
            if (string.IsNullOrEmpty(excelPath)) return null;

            // Obtenemos el dict de informacion
            Dictionary<string, List<(double X, double Y)>> dictFromExcel =
                cls_00_ReadCoordFromExcel.ReadCoordinatesFromExcel(excelPath);
            // Validamos
            if (dictFromExcel == null || dictFromExcel.Count == 0)
            {
                // Mensaje
                MessageBox.Show("❌ No coordinates were found in the Excel file.", "Error");
                // Finalizamos
                return null;
            }

            // return
            return dictFromExcel;
        }

    }
}
