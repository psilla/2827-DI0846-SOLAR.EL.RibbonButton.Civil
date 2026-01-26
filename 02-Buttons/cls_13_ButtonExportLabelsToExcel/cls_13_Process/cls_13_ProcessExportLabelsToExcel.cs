using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_13_ProcessExportLabelsToExcel
    {
        public static List<List<string>> ProcessExportLabelsToExcel(
            Editor ed,
            Database db,
            Transaction tr
        )
        {
            // Obtenemos settings
            SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();
            AutocadSettings autoSettings = AutocadSettings.GetDefaultSettings();

            // Obtenemos el listado de capas del documento
            List<string> docLayers = cls_00_GetLayerNamesFromDoc.GetLayerNamesFromDoc(db);

            List<string> defaultLayersStringLab =
                new List<string> { solarSet.LabelStringLayer };
            // Obtenemos las etiquetas
            PromptSelectionResult psrStringLab = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.LabelStringTag, "MTEXT", defaultLayersStringLab
            );
            // Validamos
            if (psrStringLab == null) return null;

            // Obtenemos los Ids
            HashSet<ObjectId> psrStringLabIds = new HashSet<ObjectId>(psrStringLab.Value.GetObjectIds());

            // Validamos estructura de las etiquetas
            if (!cls_00_MTextObjectsByLayer.AllLabelsHaveSameFieldCount(
                tr, psrStringLabIds, autoSettings,
                out int fieldCount, out List<string> referenceFields
            )) return null;

            List<List<string>> dataToExcel = new List<List<string>>();
            // Iteramos
            foreach (ObjectId id in psrStringLabIds)
            {
                // Obtenemos el texto
                DBObject dbObj = tr.GetObject(id, OpenMode.ForRead);
                MText mText = dbObj as MText;
                // Validamos
                if (mText == null) continue;

                // Obtenemos valor del texto
                string value = mText.Contents;
                // Extraemos campos
                List<string> fieldValues =
                    cls_00_MTextObjectsByLayer.SplitLabelValueByCond(autoSettings, value);
                // Validamos
                if (fieldValues == null || fieldValues.Count == 0) continue;

                // Extraemos coordenadas
                double x = mText.Location.X;
                double y = mText.Location.Y;

                // Nueva lista con el orden deseado
                List<string> row = new List<string>
                {
                    x.ToString("F3"),
                    y.ToString("F3"),
                    value
                };
                // Agregar los campos del split
                row.AddRange(fieldValues);
                // Agregar a la lista final
                dataToExcel.Add(row);
            }
            // return
            return dataToExcel;
        }


    }
}
