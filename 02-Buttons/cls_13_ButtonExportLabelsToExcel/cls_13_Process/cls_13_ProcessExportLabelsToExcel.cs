using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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
            Transaction tr,
            AutocadSettings autoSettings,
            string labelStringTag,
            List<string> defaultLayersStringLab
        )
        {
            // -------------------------------
            // Obtener capas del documento
            // -------------------------------

            List<string> docLayers = cls_00_GetLayerNamesFromDoc.GetLayerNamesFromDoc(db);

            // -------------------------------
            // Obtener etiquetas
            // -------------------------------

            List<string> psrStringLabLayers = null;
            // Obtenemos las etiquetas
            PromptSelectionResult psrStringLab = cls_00_GetEntityByLayer.GetTextAndMTextByLayers(
                docLayers, ed, labelStringTag, out psrStringLabLayers, defaultLayersStringLab
            );
            // Validamos
            if (psrStringLab == null) return null;

            // Obtenemos los Ids
            HashSet<ObjectId> psrStringLabIds = new HashSet<ObjectId>(psrStringLab.Value.GetObjectIds());

            // -------------------------------
            // Validar estructura etiquetas
            // -------------------------------

            if (!cls_00_MTextObjectsByLayer.AllLabelsHaveSameFieldCount(
                tr, psrStringLabIds, autoSettings, out int fieldCount, out List<string> referenceFields
            )) return null;

            // -------------------------------
            // Obtener datos
            // -------------------------------

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
                List<string> fieldValues =cls_00_MTextObjectsByLayer.SplitLabelValueByCond(
                    autoSettings, value
                );
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
