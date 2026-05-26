using System.Collections.Generic;
using System.Windows.Forms;
using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Process;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.ExcelAutocad;

namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_14_MainCreateLabelsFromExcel
    {
        private static Dictionary<string, List<(double X, double Y)>> TryGetCoordFromExcel()
        {
            // try
            try
            {
                // Seleccionar el directorio del Excel
                string excelDirectory = cls_00_SelectExcelDirectory.SelectExcelDirectory();
                // Validamos
                if (string.IsNullOrEmpty(excelDirectory)) return null;

                // Seleccionar el archivo de Excel
                string excelPath = cls_00_SelectExcelFile.SelectExcelFile(excelDirectory);
                // Validamos
                if (string.IsNullOrEmpty(excelPath)) return null;

                // Obtenemos el dict de informacion
                Dictionary<string, List<(double X, double Y)>> dictFromExcel =
                    cls_00_ReadCoordFromExcel.ReadCoordinatesFromExcel(excelPath);
                // Validamos
                if (dictFromExcel == null || dictFromExcel.Count == 0)
                {
                    // Mensaje
                    MessageBox.Show("❌ No coordinates were found in the Excel file.", "Error");
                    // Finalizamos
                    return null;
                }

                // return
                return dictFromExcel;
            }
            // catch
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error getting coordinates from Excel:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Excel Process Exception",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return null;
            }
        }

        public ProcessResult MainCreateLabelsFromExcel(
            string projectCode
        )
        {
            // Obtenemos el dict de informacion
            Dictionary<string, List<(double X, double Y)>> dictFromExcel = TryGetCoordFromExcel();
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
