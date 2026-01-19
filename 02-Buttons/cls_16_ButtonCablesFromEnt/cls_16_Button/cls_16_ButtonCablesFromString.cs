using System;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.Main;
using SOLAR.EL.RibbonButton.Autocad.Main;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_16_ButtonCablesFromString
    {
        public static void ButtonCablesFromString(
            string projectCode,
            string excelPath,
            string projectUnits
        )
        {
            DateTime startTime = DateTime.Now;

            // Procesar los archivos seleccionados
            cls_16_MainCablesFromString mainProcess = new cls_16_MainCablesFromString();
            // Obtener el resultado del proceso
            ProcessResult processResult =
                mainProcess.MainCablesFromString(projectCode, excelPath, projectUnits);

            // Mostrar el resumen de los resultados al finalizar
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            // Mensaje
            MessageBox.Show("CablesFromString process has completed successfully." +
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
