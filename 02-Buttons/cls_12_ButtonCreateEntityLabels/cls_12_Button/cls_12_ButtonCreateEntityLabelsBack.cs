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
    internal class cls_12_ButtonCreateEntityLabelsBack
    {
        [CommandMethod("CreateEntityLabelsBack")]
        public static void ButtonCreateEntityLabelsBack()
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

            cls_12_MainCreateEntityLabelsBack mainProcess = new cls_12_MainCreateEntityLabelsBack();
            // Obtener el resultado del proceso
            ProcessResult processResult = mainProcess.MainCreateEntityLabelsBack(
                selectedFiles.ToArray(), projectCode, solarSet, autoSettings
            );

            // -----------------------------
            // Mostrar resumen
            // -----------------------------

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            // Mensaje
            MessageBox.Show(
                "CreateEntityLabelsBack process has completed successfully." +
                "\nDuration: " + duration.ToString(@"hh\:mm\:ss") +
                "\nStarted at: " + startTime.ToString("HH:mm:ss") +
                "\nEnded at: " + endTime.ToString("HH:mm:ss") +
                $"\n\n{processResult.ParametersAnalyzed} labels have been created in total." +
                $"\n{processResult.TotalFilesProcessed} files were processed successfully.",
                "Extraction Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information
            );
        }
    }
}
