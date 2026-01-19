using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;
using TYPSA.SharedLib.Excel;
using TYPSA.SharedLib.UserForms;
using SOLAR.EL.RibbonButton.Autocad.Process;

namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_13_MainExportLabelsToExcelBack
    {
        public ProcessResult MainExportLabelsToExcelBack(
            string[] selectedFiles, 
            string projectCode
        )
        {
            // Variables para recopilar métricas
            int totalSelectedFiles = selectedFiles.Length;
            int filesSelectedProcessed = 0;
            int percentage = 0;
            int totalLabelsCreatedGlobal = 0;

            List<List<string>> dataToExcelGlobal = new List<List<string>>();
            // Crear el formulario de la barra de progreso
            using (ProgressBarControl progressBarForm = new ProgressBarControl())
            {
                // Mostramos barra de progreso
                progressBarForm.Show();

                // try
                try
                {
                    // Iterar sobre los archivos seleccionados
                    foreach (string file in selectedFiles)
                    {
                        // Obtener el nombre sin extensión
                        string fileName =
                            System.IO.Path.GetFileNameWithoutExtension(file);
                        // try
                        try
                        {
                            // Abrir el documento
                            using (Document openedDoc = Application.DocumentManager.Open(file, false))
                            {
                                // Validamos
                                if (openedDoc == null)
                                {
                                    // Mensaje
                                    new AutoCloseMessageForm(
                                        $"Error opening document:\n{file}"
                                    ).ShowDialog();
                                    // Actualizar la barra de progreso
                                    filesSelectedProcessed++;
                                    percentage = (int)((double)filesSelectedProcessed / totalSelectedFiles * 100);
                                    progressBarForm.ProgressValue = percentage;
                                    // Obviamos
                                    continue;
                                }

                                // Obtenemos variables
                                Database db = openedDoc.Database;
                                Editor ed = cls_00_DocumentInfo.GetEditor(openedDoc);

                                // Bloquear el documento
                                using (openedDoc.LockDocument())
                                // Iniciar transacción
                                using (Transaction tr = openedDoc.TransactionManager.StartTransaction())
                                {
                                    // try
                                    try
                                    {
                                        // Obtener BlockTable
                                        BlockTable bt = cls_00_DocumentInfo.GetBlockTableForRead(tr, db);
                                        // Obtener BlockTableRecord
                                        BlockTableRecord btr = cls_00_DocumentInfo.GetBlockTableRecordForWrite(tr, bt);

                                        // Obtenemos las etiquetas
                                        List<DBObject> mTextOjects = cls_00_MTextObjectsByLayer.
                                            get_MTextObjectsByLayer_FromDicc(openedDoc, true);
                                        // Validamos
                                        if (mTextOjects == null) continue;

                                        // Llamamos al Main
                                        List<List<string>> dataToExcelByFile = cls_13_ProcessExportLabelsToExcel
                                            .ProcessExportLabelsToExcel(mTextOjects);

                                        // Obtenemos numero etiquetas exportadas en archivo
                                        int totalLabelsCreated = dataToExcelByFile.Count;
                                        // Validamos
                                        if (totalLabelsCreated != 0)
                                        {
                                            // Mensaje
                                            new AutoCloseMessageForm(
                                                $"A total of {totalLabelsCreated} labels have been exported."
                                            ).ShowDialog();

                                            // Añadimos
                                            dataToExcelGlobal.AddRange(dataToExcelByFile);

                                            // Actualizamos contador
                                            totalLabelsCreatedGlobal += totalLabelsCreated;
                                        }

                                        // Cerramos transaccion
                                        tr.Abort();
                                    }
                                    // catch
                                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                                    {
                                        // Mensaje
                                        new AutoCloseMessageForm(
                                            $"Error while processing the document '{file}':\n" +
                                            $"{ex.Message}\nDetails:\n{ex.StackTrace}"
                                        ).ShowDialog();
                                    }
                                }
                                // Cerramos y descartamos documento
                                openedDoc.CloseAndDiscard();

                                // Actualizar la barra de progreso
                                filesSelectedProcessed++;
                                percentage = (int)((double)filesSelectedProcessed / totalSelectedFiles * 100);
                                progressBarForm.ProgressValue = percentage;
                            }
                        }
                        // catch
                        catch (Autodesk.AutoCAD.Runtime.Exception ex)
                        {
                            // Mensaje
                            MessageBox.Show(
                                $"EXCEPTION while opening document:" +
                                $"\n{ex.Message}\n{ex.StackTrace}"
                            );
                        }
                    }

                    // Cerramos
                    progressBarForm.Close();

                    // Validamos
                    if (dataToExcelGlobal.Count == 0)
                    {
                        // Mensaje
                        MessageBox.Show(
                            "No valid labels found to export.", "Export Labels",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning
                        );
                        // Finalizamos
                        return new ProcessResult
                        {
                            TotalFilesProcessed = filesSelectedProcessed,
                            ParametersAnalyzed = 0
                        };
                    }

                    // Exportamos a Excel
                    cls_00_ExportLabelsToExcel_OpenXml
                        .ExportLabelsToExcel_OpenXml(dataToExcelGlobal);

                    // return
                    return new ProcessResult
                    {
                        TotalFilesProcessed = filesSelectedProcessed,
                        ParametersAnalyzed = totalLabelsCreatedGlobal
                    };
                }
                // catch
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    // Mensaje
                    MessageBox.Show(
                        $"ERROR: {ex.GetType().Name}\n{ex.Message}\n{ex.StackTrace}",
                        "Error"
                    );
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
}
