using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;
using TYPSA.SharedLib.Excel;
using SOLAR.EL.RibbonButton.Autocad.Process;

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

            // Obtenemos las etiquetas
            List<DBObject> mTextOjects =
                cls_00_MTextObjectsByLayer.get_MTextObjectsByLayer_FromDicc(doc, true);
            // Validamos
            if (mTextOjects == null)
            {
                // Finalizamos
                return new ProcessResult
                {
                    TotalFilesProcessed = 1,
                    ParametersAnalyzed = 0
                };
            }

            List<List<string>> dataToExcel;
            // Abrimos transaccion
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                // Obtenemos info
                dataToExcel = cls_13_ProcessExportLabelsToExcel.
                    ProcessExportLabelsToExcel(mTextOjects);
                // Cerramos transaccion
                tr.Abort();
            }

            // Validamos
            if (dataToExcel.Count == 0)
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

            // Por defecto
            return new ProcessResult
            {
                TotalFilesProcessed = 1,
                ParametersAnalyzed = dataToExcel.Count
            };
        }



    }
}
