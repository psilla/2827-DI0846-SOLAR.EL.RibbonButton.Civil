using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using TYPSA.SharedLib.Civil.Buttons;
using TYPSA.SharedLib.Civil.GetDocument;
using TYPSA.SharedLib.Civil.ObjectsByTypeByLayer;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_13_ButtonExportLabelsToExcel
    {
        [CommandMethod("ExportLabelsToExcel")]
        public static void ExportLabelsToExcel()
        {
            // Obtener codigo de proyecto
            string projectCode = cls_00_GetUserData.GetProjectCodeFromDialog();
            // Validamos
            if (projectCode == null) return;

            // Obtenemos variables
            Document doc = cls_00_DocumentInfo.GetActiveDocument();

            // Obtenemos las etiquetas
            List<DBObject> mTextOjects = 
                cls_00_MTextObjectsByLayer.get_MTextObjectsByLayer_FromDicc(doc, true);
            // Validamos
            if (mTextOjects == null) return;

            // Obtenemos valores de las etiquetas
            List<string> mTextOjectsValues =
                cls_00_MTextObjectsByLayer.GetMTextValues(mTextOjects);
            // Validamos
            if (mTextOjectsValues == null) return;

            // Separamos valores por condicion
            List<List<string>> mTextOjectsValuesSplit =
                cls_00_MTextObjectsByLayer.SplitLabelValuesByCond(mTextOjectsValues);
            // Validamos
            if (mTextOjectsValuesSplit == null) return;

            // Definimos headers
            List<string> headers = new List<string>
            {
                "Centro", "Inversor", "MPPT ($)", "Tracker", "String"
            };

            // Exportar a Excel
            cls_00_MTextObjectsByLayer.ExportLabelsToExcel_EPPlus(
                mTextOjectsValuesSplit, headers
            );



        }
    }
}
