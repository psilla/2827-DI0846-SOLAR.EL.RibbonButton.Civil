using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Process;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Autocad.Metrics;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;
using TYPSA.SharedLib.Metrics;


namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_12_MainOrderEntityLabels
    {
        public ProcessResult MainOrderEntityLabels(
            string projectCode,
            SolarSettings solarSet,
            AutocadSettings autoSettings
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
                        int? totalLabelsCreated =  cls_12_ProcessOrderEntityLabels.ProcessOrderEntityLabels(
                            ed, db, tr, solarSet, autoSettings
                        );
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

                        // Enviar Metricas App
                        SendMetrics(1, totalLabelsCreated.Value, projectCode);
                        // Enviar Metricas Serapis
                        cls_00_SerapisMetrics.InitializeMetricsAsync(solarSet.GuidSerapisMetricsCreateEntLabels);

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
            string accionId = "69771b54a266bb4378c55fd6";
            string processLabelsCreation = "69771b54a266bb4378c55fd4";
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
