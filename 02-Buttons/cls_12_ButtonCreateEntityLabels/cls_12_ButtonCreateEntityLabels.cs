using Autodesk.AutoCAD.Runtime;
using TYPSA.SharedLib.Civil.Buttons;
using SOLAR.EL.RibbonButton.Civil.Main;
using TYPSA.SharedLib.Civil.Main;
using System;
using System.Windows.Forms;


namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_ButtonCreateEntityLabels
    {
        [CommandMethod("CreateEntityLabels")]
        public static void CreateEntityLabels()
        {
            DateTime startTime = DateTime.Now;

            // Obtener codigo de proyecto
            string projectCode = cls_00_GetUserData.GetProjectCodeFromDialog();
            // Validamos
            if (projectCode == null) return;

            // Obtenemos settings
            SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();

            // Procesar los archivos seleccionados
            cls_12_MainCreateEntityLabels mainProcess = new cls_12_MainCreateEntityLabels();
            // Obtener el resultado del proceso
            ProcessResult processResult = 
                mainProcess.MainCreateEntityLabels(solarSet, projectCode);

            // Mostrar el resumen de los resultados al finalizar
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            // Mensaje
            MessageBox.Show("CreateEntityLabels process has completed successfully." +
                            "\nDuration: " + duration.ToString(@"hh\:mm\:ss") +
                            "\nStarted at: " + startTime.ToString("HH:mm:ss") +
                            "\nEnded at: " + endTime.ToString("HH:mm:ss") +
                            $"\n\n{processResult.ParametersAnalyzed} labels have been created in total." +
                            $"\n{processResult.TotalFilesProcessed} files were processed successfully.",
                            "Extraction Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        













































    }
}
