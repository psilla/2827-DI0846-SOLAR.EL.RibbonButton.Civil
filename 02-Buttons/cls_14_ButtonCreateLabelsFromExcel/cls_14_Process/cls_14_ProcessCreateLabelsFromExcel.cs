
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using TYPSA.SharedLib.Autocad.DrawEntities;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_14_ProcessCreateLabelsFromExcel
    {
        public static int ProcessCreateLabelsFromExcel(
            Dictionary<string, List<(double X, double Y)>> dictFromExcel,
            Transaction tr,
            BlockTableRecord btr,
            Database db
        )
        {
            // Obtenemos settings
            SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();

            // Creamos las capa si no existe
            cls_00_CreateLayerIfNotExists.CreateLayerIfNotExists(solarSet.LabelStringLayer, db);

            // Obtenemos Text Styles
            List<string> availableTextStyles =
                cls_00_DocumentInfo.GetAllTextStylesFromDrawing(db);

            // Validamos Config
            if (!cls_14_CreateLabelsFromExcelConfig.CreateLabelsFromExcelConfig(
                solarSet, availableTextStyles,
                out bool isHorizontal, out string selectedTextStyle, out AttachmentPoint selectedTextJust
            )) return 0;

            int totalLabelsCreated = 0;
            // Recorremos el diccionario y creamos textos
            foreach (var kvp in dictFromExcel)
            {
                // Obtenemos coordenadas
                string tagValue = kvp.Key;
                List<(double X, double Y)> coords = kvp.Value;
                // Iteramos
                foreach (var (X, Y) in coords)
                {
                    // Obtenemos pto de insercion
                    Point3d basePoint = new Point3d(X, Y, 0);
                    // Creamos etiqueta
                    cls_00_DrawMtext.DrawMTextOnPoint(
                        basePoint, tagValue, tr, btr,
                        isHorizontal, 1, 7, solarSet.LabelStringLayer,
                        selectedTextStyle, selectedTextJust
                    );
                    // Actualizamos contador
                    totalLabelsCreated++;
                }
            }
            // return
            return totalLabelsCreated;
        }

    }
}
