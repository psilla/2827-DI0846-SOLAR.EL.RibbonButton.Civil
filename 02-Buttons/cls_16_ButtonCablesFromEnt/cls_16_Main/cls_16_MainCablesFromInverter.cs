using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Autocad.Metrics;
using SOLAR.EL.RibbonButton.Autocad.Process;

namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_16_MainCablesFromInverter
    {
        public ProcessResult MainCablesFromInverter(
            string projectCode,
            string excelPath,
            string projectUnits
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

                        // Obtenemos info
                        int? totalLabelsCreated = cls_16_ProcessCablesFromInv.
                            ProcessCablesFromInverter(ed, db, tr, btr, excelPath, projectUnits);
                        // Validamos
                        if (totalLabelsCreated != null)
                        {
                            // Abortamos transaccion
                            tr.Abort();

                            // Enviar Metrics
                            SendMetrics(1, totalLabelsCreated.Value, projectCode);

                            // return
                            return new ProcessResult
                            {
                                TotalFilesProcessed = 1,
                                ParametersAnalyzed = totalLabelsCreated.Value
                            };
                        }

                        // Cerramos transaccion
                        tr.Commit();
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

        private void SendMetrics(int totalFiles, int labelsExported, string projectCode)
        {
            string accionId = "696e04dea266bb4378c54d3c";
            string processLabelsCreation = "696e04dea266bb4378c54d38";
            string emailUser = Environment.UserName;

            var executed_process = new[]
            {
            new { proceso = processLabelsCreation, recuento = labelsExported }
            };

            var additionalData = new
            {
                ScriptName = "DE2827 - Energía: Cable Length Measurements",
                FilesProcessed = totalFiles,
                ProjectCode = projectCode,
                ExecutionStatus = 1,
                Version = "V.00.01"
            };

            cls_00_MetricsSender.SendMetricsAsync(emailUser, accionId, executed_process, additionalData);
        }

    }
}
