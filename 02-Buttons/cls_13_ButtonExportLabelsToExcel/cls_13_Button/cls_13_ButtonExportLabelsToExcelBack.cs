using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using SOLAR.EL.RibbonButton.Autocad.Main;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.Buttons;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_13_ButtonExportLabelsToExcelBack
    {
        [CommandMethod("ExportLabelsToExcelBack")]
        public static void ButtonExportLabelsToExcelBack()
        {
            // -----------------------------
            // Obtener Datos Usuario
            // -----------------------------

            bool userData = cls_00_GetUserData.GetUserData(
                out string projectCode,
                out List<string> selectedFiles,
                out string selectedFolderPath,
                out DateTime startTime
            );
            // Validamos
            if (!userData) return;

            // -----------------------------
            // Obtener settings
            // -----------------------------

            SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();
            AutocadSettings autoSettings = AutocadSettings.GetDefaultSettings();

            // -----------------------------
            // Llamada al main
            // -----------------------------

            cls_13_MainExportLabelsToExcelBack mainProcess = new cls_13_MainExportLabelsToExcelBack();
            // Obtener el resultado del proceso
            ProcessResult processResult = mainProcess.MainExportLabelsToExcelBack(
                selectedFiles.ToArray(), projectCode, solarSet, autoSettings
            );

            // -----------------------------
            // Mostrar resumen
            // -----------------------------

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            // Mensaje
            MessageBox.Show(
                "ExportLabelsToExcelBack process has completed successfully." +
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
