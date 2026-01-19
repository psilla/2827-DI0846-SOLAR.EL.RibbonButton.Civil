using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using System;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Metrics;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.UserForms;
using SOLAR.EL.RibbonButton.Autocad.Process;

namespace SOLAR.EL.RibbonButton.Autocad.Main
{
    internal class cls_12_MainCreateEntityLabelsBack
    {
        public ProcessResult MainCreateEntityLabelsBack(
            string[] selectedFiles, 
            string projectCode
        )
        {
            // Variables para recopilar métricas
            int totalSelectedFiles = selectedFiles.Length;
            int filesSelectedProcessed = 0;
            int percentage = 0;
            int totalLabelsCreatedGlobal = 0;

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

                                bool skipFile = false;
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

                                        // Llamamos al Main
                                        int? totalLabelsCreated = cls_12_ProcessCreateEntityLabels.ProcessCreateEntityLabels(
                                            ed, db, tr, btr
                                        );
                                        // Validamos
                                        if (totalLabelsCreated == null)
                                        {
                                            // Abortamos
                                            tr.Abort();
                                            // Obviamos
                                            skipFile = true;
                                        }
                                        else
                                        {
                                            // Obtenemos numero etiquetas
                                            int totalLabelsCreatedValue = totalLabelsCreated.Value;
                                            // Mensaje
                                            new AutoCloseMessageForm(
                                                $"A total of {totalLabelsCreatedValue} labels have been created."
                                            ).ShowDialog();
                                            // Actualizamos contador
                                            totalLabelsCreatedGlobal += totalLabelsCreatedValue;
                                            // Cerramos transaccion
                                            tr.Commit();
                                        }
                                    }
                                    // catch
                                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                                    {
                                        // Mensaje
                                        new AutoCloseMessageForm(
                                            $"Error while processing the document '{file}':\n" +
                                            $"{ex.Message}\nDetails:\n{ex.StackTrace}"
                                        ).ShowDialog();
                                        // Obviamos
                                        skipFile = true;
                                    }
                                }
                                // Validamos
                                if (skipFile)
                                {
                                    // Cerramos y descartamos documento
                                    openedDoc.CloseAndDiscard();
                                }
                                // En caso contrario
                                else
                                {
                                    // Guardamos y guardamos documento 
                                    string filePathName = System.IO.Path.GetFullPath(file);
                                    openedDoc.CloseAndSave(filePathName);
                                }

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

                    // Enviar Metrics
                    SendMetrics(filesSelectedProcessed, totalLabelsCreatedGlobal, projectCode);

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

        private void SendMetrics(int totalFiles, int labelsCreated, string projectCode)
        {
            string accionId = "68dcd05bb10174ef74121b55";
            string processIdOpeningDWGFile = "669669da1d83111125968025";
            string processLabelsCreation = "68dcd028b10174ef74121b34";
            string emailUser = Environment.UserName;

            var executed_process = new[]
            {
            new { proceso = processIdOpeningDWGFile, recuento = totalFiles },
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
