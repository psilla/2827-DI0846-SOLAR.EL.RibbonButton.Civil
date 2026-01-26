using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Process;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Autocad.Metrics;

namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_12_MainRemoveFieldLabels
    {
        public ProcessResult MainRemoveFieldLabels(
            string projectCode
        )
        {
            // Obtenemos variables
            Document doc = cls_00_DocumentInfo.GetActiveDocument();
            Database db = cls_00_DocumentInfo.GetDatabaseFromDocument(doc);
            Editor ed = cls_00_DocumentInfo.GetEditor(doc);

            // Bloquear el documento
            using (DocumentLock docLock = doc.LockDocument())
            {
                // Abrimos transaccion
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // try
                    try
                    {
                        // Llamamos al Main
                        int? totalLabelsCreated =
                            cls_12_ProcessRemoveFieldLabels.ProcessRemoveFieldLabels(ed, db, tr);
                        // Validamos
                        if (totalLabelsCreated == null)
                        {
                            // Abortamos
                            tr.Abort();
                            // Finalizamos
                            return new ProcessResult
                            {
                                TotalFilesProcessed = 1,
                                ParametersAnalyzed = 0
                            };
                        }

                        // Cerramos transaccion
                        tr.Commit();

                        // Enviar Metrics
                        SendMetrics(1, totalLabelsCreated.Value, projectCode);

                        // return
                        return new ProcessResult
                        {
                            TotalFilesProcessed = 1,
                            ParametersAnalyzed = totalLabelsCreated.Value
                        };

                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        // Mensaje
                        MessageBox.Show(
                            $"\n❌ Error: {ex.Message}\n{ex.StackTrace}",
                            "Error General"
                        );
                    }
                }
            }
            // Por defecto
            return new ProcessResult
            {
                TotalFilesProcessed = 1,
                ParametersAnalyzed = 0
            };
        }

        private void SendMetrics(int totalFiles, int labelsCreated, string projectCode)
        {
            string accionId = "69771b54a266bb4378c55fdc";
            string processLabelsCreation = "69771b54a266bb4378c55fda";
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
