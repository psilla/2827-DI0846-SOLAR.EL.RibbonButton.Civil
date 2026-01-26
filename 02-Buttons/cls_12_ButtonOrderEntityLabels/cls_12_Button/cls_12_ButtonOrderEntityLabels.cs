using Autodesk.AutoCAD.Runtime;
using TYPSA.SharedLib.Autocad.Buttons;
using SOLAR.EL.RibbonButton.Autocad.Main;
using TYPSA.SharedLib.Autocad.Main;
using System;
using System.Windows.Forms;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_ButtonOrderEntityLabels
    {
        [CommandMethod("OrderStringLabels")]
        public static void ButtonOrderEntityLabels()
        {
            DateTime startTime = DateTime.Now;

            // Obtener codigo de proyecto
            string projectCode = cls_00_GetUserData.GetProjectCodeFromDialog();
            // Validamos
            if (projectCode == null) return;

            // Procesar los archivos seleccionados
            cls_12_MainOrderEntityLabels mainProcess = new cls_12_MainOrderEntityLabels();
            // Obtener el resultado del proceso
            ProcessResult processResult = mainProcess.MainOrderEntityLabels(projectCode);

            // Mostrar el resumen de los resultados al finalizar
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            // Mensaje
            MessageBox.Show(
                "OrderEntityLabels process has completed successfully." +
                "\nDuration: " + duration.ToString(@"hh\:mm\:ss") +
                "\nStarted at: " + startTime.ToString("HH:mm:ss") +
                "\nEnded at: " + endTime.ToString("HH:mm:ss") +
                $"\n\n{processResult.ParametersAnalyzed} labels have been ordered in total." +
                $"\n{processResult.TotalFilesProcessed} files were processed successfully.",
                "Extraction Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        













































    }
}
