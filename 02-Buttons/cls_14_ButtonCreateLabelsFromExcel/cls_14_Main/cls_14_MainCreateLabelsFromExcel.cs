using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using SOLAR.EL.RibbonButton.Autocad.Process;

namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_14_MainCreateLabelsFromExcel
    {
        public ProcessResult MainCreateLabelsFromExcel(
            string projectCode
        )
        {
            // Obtenemos el dict de informacion
            Dictionary<string, List<(double X, double Y)>> dictFromExcel = 
                cls_14_TryGetCoordFromExcel.TryGetCoordFromExcel();
            // Validamos
            if (dictFromExcel == null)
            {
                // Finalizamos
                return new ProcessResult
                {
                    TotalFilesProcessed = 0,
                    ParametersAnalyzed = 0
                };
            }
                
            // Obtenemos variables de AutoCAD
            Document doc = cls_00_DocumentInfo.GetActiveDocument();
            Database db = cls_00_DocumentInfo.GetDatabaseFromDocument(doc);
            Editor ed = cls_00_DocumentInfo.GetEditor(doc);

            // Bloquear documento
            using (DocumentLock docLock = doc.LockDocument())
            {
                // Abrimos transaccion
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // try
                    try
                    {
                        // Obtener BlockTable
                        BlockTable bt = cls_00_DocumentInfo.GetBlockTableForRead(tr, db);
                        // Obtener BlockTableRecord
                        BlockTableRecord btr = cls_00_DocumentInfo.GetBlockTableRecordForWrite(tr, bt);

                        // Creamos etiquetas
                        int totalLabelsCreated = cls_14_ProcessCreateLabelsFromExcel.
                            ProcessCreateLabelsFromExcel(dictFromExcel, tr, btr, db);

                        // Cerramos transaccion
                        tr.Commit();

                        // return
                        return new ProcessResult
                        {
                            TotalFilesProcessed = 1,
                            ParametersAnalyzed = totalLabelsCreated
                        };
                    }
                    // catch
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



    }
}
