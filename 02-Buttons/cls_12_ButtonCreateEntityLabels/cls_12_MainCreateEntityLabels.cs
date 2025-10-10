using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Windows.Forms;
using System;
using SOLAR.EL.RibbonButton.Autocad.Buttons;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Metrics;
using TYPSA.SharedLib.Autocad.Main;


namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_12_MainCreateEntityLabels
    {
        public ProcessResult MainCreateEntityLabels(
            SolarSettings solarSet,
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
                // Abrimos transacción
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // Obtener BlockTable
                        BlockTable bt = cls_00_DocumentInfo.GetBlockTableForRead(tr, db);

                        // Obtener BlockTableRecord
                        BlockTableRecord btr = cls_00_DocumentInfo.GetBlockTableRecordForWrite(tr, bt);

                        // Llamamos al Main
                        int totalLabelsCreated = cls_12_ProcessEntityLabels.ProcessEntityLabels(
                            ed, db, tr, btr,
                            solarSet.ContGenLayer, solarSet.ContInvLayer, solarSet.TrackLayer,
                            solarSet.StringLayer, solarSet.LabelsTrackLayer, solarSet.LabelsInvLayer,
                            solarSet.ContGenTag, solarSet.ContInvTag, solarSet.TrackTag, solarSet.ContInvLabelTag,
                            solarSet.ContGenProp, solarSet.ContInvProp, solarSet.TrackProp, solarSet.StringProp,
                            solarSet, solarSet.TipTrack, solarSet.TipEstFija
                        );

                        // Cerramos transacción
                        tr.Commit();

                        // Enviar Metrics
                        SendMetrics(1, totalLabelsCreated, projectCode);

                        // return
                        return new ProcessResult
                        {
                            TotalFilesProcessed = 1,
                            ParametersAnalyzed = totalLabelsCreated
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
                TotalFilesProcessed = 0,
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
