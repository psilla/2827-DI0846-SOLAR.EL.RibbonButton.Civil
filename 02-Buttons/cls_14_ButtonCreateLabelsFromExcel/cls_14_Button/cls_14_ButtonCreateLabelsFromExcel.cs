using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using SOLAR.EL.RibbonButton.Autocad.Main;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.Buttons;
using TYPSA.SharedLib.Autocad.Main;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    public class cls_14_ButtonCreateLabelsFromExcel
    {
        [CommandMethod(RibbonCommands.CreateLabelsFromExcel)]
        public static void ButtonCreateLabelsFromExcel()
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

            cls_14_MainCreateLabelsFromExcel mainProcess = new cls_14_MainCreateLabelsFromExcel();
            // Obtener el resultado del proceso
            ProcessResult processResult = mainProcess.MainCreateLabelsFromExcel(
                projectCode, solarSet
            );

            // -----------------------------
            // Mostrar resumen
            // -----------------------------

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            // Mensaje
            MessageBox.Show(
                "CreateLabelsFromExcel process has completed successfully." +
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
