using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using SOLAR.EL.RibbonButton.Autocad.Main;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.Buttons;
using TYPSA.SharedLib.Autocad.Main;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_ButtonRemoveFieldLabels
    {
        [CommandMethod(RibbonCommands.RemoveFieldFromLabels)]
        public static void ButtonRemoveFieldLabels()
        {
            DateTime startTime = DateTime.Now;

            // -----------------------------
            // Obtener Datos Usuario
            // -----------------------------

            string projectCode = cls_00_GetUserData.GetProjectCodeFromDialog();
            // Validamos
            if (projectCode == null) return;

            // -----------------------------
            // Obtener settings
            // -----------------------------

            SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();

            // -----------------------------
            // Llamada al main
            // -----------------------------

            cls_12_MainRemoveFieldLabels mainProcess = new cls_12_MainRemoveFieldLabels();
            // Obtener el resultado del proceso
            ProcessResult processResult = mainProcess.MainRemoveFieldLabels(projectCode, solarSet);

            // -----------------------------
            // Mostrar resumen
            // -----------------------------

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            // Mensaje
            MessageBox.Show(
                "RemoveFieldLabels process has completed successfully." +
                "\nDuration: " + duration.ToString(@"hh\:mm\:ss") +
                "\nStarted at: " + startTime.ToString("HH:mm:ss") +
                "\nEnded at: " + endTime.ToString("HH:mm:ss") +
                $"\n\n{processResult.ParametersAnalyzed} labels have been modified in total." +
                $"\n{processResult.TotalFilesProcessed} files were processed successfully.",
                "Extraction Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information
            );
        }

        













































    }
}
