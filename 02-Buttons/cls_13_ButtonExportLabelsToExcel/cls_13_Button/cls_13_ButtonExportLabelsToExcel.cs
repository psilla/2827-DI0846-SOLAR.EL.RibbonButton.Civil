using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using SOLAR.EL.RibbonButton.Autocad.Main;
using TYPSA.SharedLib.Autocad.Buttons;
using TYPSA.SharedLib.Autocad.Main;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_13_ButtonExportLabelsToExcel
    {
        [CommandMethod("ExportLabelsToExcel")]
        public static void ButtonExportLabelsToExcel()
        {
            DateTime startTime = DateTime.Now;

            // Obtener codigo de proyecto
            string projectCode = cls_00_GetUserData.GetProjectCodeFromDialog();
            // Validamos
            if (projectCode == null) return;

            // Procesar el Main
            cls_13_MainExportLabelsToExcel mainProcess =
                new cls_13_MainExportLabelsToExcel();
            // Obtener el resultado del proceso
            ProcessResult processResult = 
                mainProcess.MainExportLabelsToExcel(projectCode);

            // Mostrar el resumen de los resultados al finalizar
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            // Mensaje
            MessageBox.Show(
                "ExportLabelsToExcel process has completed successfully." +
                "\nDuration: " + duration.ToString(@"hh\:mm\:ss") +
                "\nStarted at: " + startTime.ToString("HH:mm:ss") +
                "\nEnded at: " + endTime.ToString("HH:mm:ss") +
                $"\n\n{processResult.ParametersAnalyzed} labels have been exported in total." +
                $"\n{processResult.TotalFilesProcessed} files were processed successfully.",
                "Extraction Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }



    }
}
