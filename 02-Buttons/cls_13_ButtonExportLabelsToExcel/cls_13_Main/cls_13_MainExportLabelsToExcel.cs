using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Process;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Autocad.Metrics;
using TYPSA.SharedLib.Excel;

namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_13_MainExportLabelsToExcel
    {
        public ProcessResult MainExportLabelsToExcel(
            string projectCode
        )
        {
            // Obtenemos variables
            Document doc = cls_00_DocumentInfo.GetActiveDocument();

            // Obtenemos variables
            Database db = doc.Database;
            Editor ed = cls_00_DocumentInfo.GetEditor(doc);

            List<List<string>> dataToExcel;
            // Abrimos transaccion
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                // Obtenemos info
                dataToExcel = cls_13_ProcessExportLabelsToExcel.
                    ProcessExportLabelsToExcel(ed, db, tr);
                // Cerramos transaccion
                tr.Abort();
            }
            // Validamos
            if (dataToExcel == null)
            {
                // Mensaje
                MessageBox.Show(
                    "No valid labels found to export.", "Export Labels",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
                // Finalizamos
                return new ProcessResult
                {
                    TotalFilesProcessed = 1,
                    ParametersAnalyzed = 0
                };
            }

            // Exportamos a Excel
            cls_00_ExportLabelsToExcel_OpenXml.
                ExportLabelsToExcel_OpenXml(dataToExcel);

            // Enviar Metrics
            SendMetrics(1, dataToExcel.Count, projectCode);

            // Por defecto
            return new ProcessResult
            {
                TotalFilesProcessed = 1,
                ParametersAnalyzed = dataToExcel.Count
            };
        }

        private void SendMetrics(int totalFiles, int labelsCreated, string projectCode)
        {
            string accionId = "68dcd0a3b10174ef74121b7b";
            string processLabelsCreation = "68dcd0a3b10174ef74121b79";
            string emailUser = Environment.UserName;

            var executed_process = new[]
            {
            new { proceso = processLabelsCreation, recuento = labelsCreated }
            };

            var additionalData = new
            {
                ScriptName = "DE2827 - Energía: Authomatic Labels",
                FilesProcessed = totalFiles,
                ProjectCode = projectCode,
                ExecutionStatus = 1,
                Version = "V.00.01"
            };

            cls_00_MetricsSender.SendMetricsAsync(emailUser, accionId, executed_process, additionalData);
        }



    }
}
