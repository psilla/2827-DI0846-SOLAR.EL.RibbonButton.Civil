using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;
using TYPSA.SharedLib.UserForms;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_ProcessOrderEntityLabels
    {
        public static int? ProcessOrderEntityLabels(
            Editor ed,
            Database db,
            Transaction tr
        )
        {
            // try
            try
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

                // Construimos dict Campo - nuevo Indice
                Dictionary<string, int> fieldOrderDict = BuildFieldOrderDictionary(referenceFields);
                // Validamos
                if (fieldOrderDict == null) return null;

                // Form para reordenar
                fieldOrderDict = InstanciarFormularios.TextBoxFormOut_NextToLabel_Integer(
                    "Enter the new order for each field:", fieldOrderDict
                );
                // Validamos
                if (fieldOrderDict == null)
                {
                    // Mensaje
                    MessageBox.Show(
                        "No valid number was provided. The process has been cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return null;
                }

                // Iteramos
                foreach (ObjectId id in psrStringLabIds)
                {
                    // Aplicamos nuevo orden
                    if (!ApplyFieldOrderToLabel(tr, id, autoSettings, fieldOrderDict)) return null;
                }
                // return
                return referenceFields.Count;
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show($"ERROR in ProcessOrderEntityLabels:\n{ex.Message}\n{ex.StackTrace}");
                // Finalizamos
                return null;
            }
        }

        private static bool ApplyFieldOrderToLabel(
            Transaction tr,
            ObjectId labelId,
            AutocadSettings autoSettings,
            Dictionary<string, int> fieldOrderDict
        )
        {
            // Obtenemos el texto
            DBObject dbObj = tr.GetObject(labelId, OpenMode.ForWrite);
            MText mText = dbObj as MText;
            // Validamos
            if (mText == null) return false;

            // Obtenemos valor del texto
            string value = mText.Contents;
            // Extraemos campos
            List<string> originalFields =
                cls_00_MTextObjectsByLayer.SplitLabelValueByCondAndToken(autoSettings, value);
            // Validamos
            if (originalFields == null || originalFields.Count == 0) return false;

            string[] reordered = new string[originalFields.Count];
            // Reordenamos
            for (int i = 0; i < originalFields.Count; i++)
            {
                string fieldKey = cls_00_MTextObjectsByLayer.GetAlphabeticFieldKey(originalFields[i]);
                // Validamos
                if (!fieldOrderDict.ContainsKey(fieldKey))
                {
                    // Mensaje
                    MessageBox.Show(
                        $"Field '{fieldKey}' not found in order definition.", "Invalid Order",
                        MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    // Finalizamos
                    return false;
                }
                // Almacenamos
                int newIndex = fieldOrderDict[fieldKey];
                reordered[newIndex] = originalFields[i];
            }

            // Reconstruimos el texto
            char separator = cls_00_MTextObjectsByLayer.GetLabelSeparator(autoSettings, value);
            // Asignamos
            mText.Contents = string.Join(separator.ToString(), reordered);

            // return
            return true;
        }

        private static Dictionary<string, int> BuildFieldOrderDictionary(
            List<string> referenceFields
        )
        {
            Dictionary<string, int> fieldOrderDict = new Dictionary<string, int>();
            // Iteramos segun numero de campos
            for (int i = 0; i < referenceFields.Count; i++)
            {
                string fieldValue = referenceFields[i];
                // Extraemos la clave alfabética
                string fieldKey = cls_00_MTextObjectsByLayer.GetAlphabeticFieldKey(fieldValue);
                // Validamos
                if (string.IsNullOrEmpty(fieldKey))
                {
                    // Mensaje
                    MessageBox.Show(
                        $"Invalid field format: '{fieldValue}'",
                        "Invalid Label Format",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    // Finalizamos
                    return null;
                }
                // Validamos duplicados
                if (fieldOrderDict.ContainsKey(fieldKey))
                {
                    // Mensaje
                    MessageBox.Show(
                        $"Duplicate field key found: '{fieldKey}'",
                        "Invalid Label Format",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    // Finalizamos
                    return null;
                }
                // Almacenamos
                fieldOrderDict.Add(fieldKey, i);
            }
            // return
            return fieldOrderDict;
        }











    }
}
