using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using SOLAR.EL.RibbonButton.Autocad.Main;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.Buttons;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_13_ButtonExportLabelsToExcel
    {
        [CommandMethod(RibbonCommands.ExportLabelsToExcelDoc)]
        public static void ButtonExportLabelsToExcel()
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
            AutocadSettings autoSettings = AutocadSettings.GetDefaultSettings();

            // -----------------------------
            // Llamada al main
            // -----------------------------

            cls_13_MainExportLabelsToExcel mainProcess = new cls_13_MainExportLabelsToExcel();
            // Obtener el resultado del proceso
            ProcessResult processResult = mainProcess.MainExportLabelsToExcel(
                projectCode, solarSet, autoSettings
            );

            // -----------------------------
            // Mostrar resumen
            // -----------------------------

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
                MessageBoxButtons.OK, MessageBoxIcon.Information
            );
        }



    }
}
