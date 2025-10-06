using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using System.Windows.Forms;
using TYPSA.SharedLib.Civil.Buttons;
using TYPSA.SharedLib.Civil.Main;
using SOLAR.EL.RibbonButton.Civil.Main;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_ButtonCreateEntityLabelsBack
    {
        [CommandMethod("CreateEntityLabelsBack")]
        public static void CreateEntityLabelsBack()
        {
            // Obtener datos de usuario
            bool userData = cls_00_GetUserData.GetUserData(
                out string projectCode,
                out List<string> selectedFiles,
                out string selectedFolderPath,
                out DateTime startTime
            );
            // Validamos
            if (!userData) return;

            // Obtenemos settings
            SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();

            // Procesar los archivos seleccionados
            cls_12_MainCreateEntityLabelsBack mainProcess =
                new cls_12_MainCreateEntityLabelsBack();

            // Obtener el resultado del proceso
            ProcessResult processResult =
                mainProcess.MainCreateEntityLabelsBack(selectedFiles.ToArray(), projectCode, solarSet);

            // Mostrar el resumen de los resultados al finalizar
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            // Mensaje
            MessageBox.Show("CreateEntityLabelsBack process has completed successfully." +
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
