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
    internal class cls_12_MainCreateEntityLabels
    {
        public ProcessResult MainCreateEntityLabels(
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
                    try
                    {
                        // Obtener BlockTable
                        BlockTable bt = cls_00_DocumentInfo.GetBlockTableForRead(tr, db);
                        // Obtener BlockTableRecord
                        BlockTableRecord btr = cls_00_DocumentInfo.GetBlockTableRecordForWrite(tr, bt);

                        // Llamamos al Main
                        int? totalLabelsCreated = cls_12_ProcessCreateEntityLabels.ProcessCreateEntityLabels(
                            ed, db, tr, btr, solarSet, autoSettings
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
            string accionId = "68dcd05bb10174ef74121b55";
            string processLabelsCreation = "68dcd028b10174ef74121b34";
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
