using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_ProcessOrderEntityLabels
    {
        public static int? ProcessOrderEntityLabels(
            Editor ed,
            Database db,
            Transaction tr,
            SolarSettings solarSet,
            AutocadSettings autoSettings
        )
        {
            // try
            try
            {
                // Obtenemos el listado de capas del documento
                List<string> docLayers = cls_00_GetLayerNamesFromDoc.GetLayerNamesFromDoc(db);

                string psrStringLabLayer = null;
                string defaultLayerStringLab = solarSet.LabelStringLayer;
                // Obtenemos las etiquetas
                PromptSelectionResult psrStringLab = cls_00_GetEntityByLayer.GetTextAndMTextByLayer(
                    docLayers, ed, solarSet.LabelStringTag, out psrStringLabLayer, defaultLayerStringLab
                );
                // Validamos
                if (psrStringLab == null) return null;

                // Obtenemos los Ids
                HashSet<ObjectId> psrStringLabIds = new HashSet<ObjectId>(psrStringLab.Value.GetObjectIds());

                // Validamos estructura de las etiquetas
                if (!cls_00_MTextObjectsByLayer.AllLabelsHaveSameFieldCount(
                    tr, psrStringLabIds, autoSettings, out int fieldCount, out List<string> referenceFields
                )) return null;

                // Construimos dict Campo - nuevo Indice
                Dictionary<string, string> fieldOrderDict = BuildFieldOrderDictionary(referenceFields);
                // Validamos
                if (fieldOrderDict == null) return null;

                // Form para reordenar
                Dictionary<string, int> fieldOrderDictAsInt = cls_00_InstaForm_TextBox.TextBoxFormOut_NextToLabel_Integer(
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

                Dictionary<ObjectId, string> failedLabels = new Dictionary<ObjectId, string>();
                // Iteramos
                foreach (ObjectId id in psrStringLabIds)
                {
                    // Aplicamos nuevo orden
                    bool success = ApplyFieldOrderToLabel(
                        tr, id, autoSettings, fieldOrderDictAsInt, out string error
                    );
                    // Validamos
                    if (!success)
                    {
                        // Almacenamos la invalida
                        failedLabels[id] = error ?? "Unknown error";
                        continue; 
                    }
                }

                // Validamos errores
                if (failedLabels.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Some labels could not be processed:\n");
                    foreach (var id in failedLabels)
                    {
                        sb.AppendLine($"- ObjectId: {id}");
                    }
                    // Mostramos con tu clase personalizada
                    ShowStringBuilder.ShowInfo(
                        $"Entities by Document Summary:", sb.ToString()
                    );
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
            Dictionary<string, int> fieldOrderDict,
            out string error
        )
        {
            error = null;

            // Obtenemos el texto
            DBObject dbObj = tr.GetObject(labelId, OpenMode.ForWrite);

            string value = null;
            bool isMText = false;
            bool isDBText = false;
            // Validamos tipo
            if (dbObj is MText mText)
            {
                value = mText.Contents;
                isMText = true;
            }
            else if (dbObj is DBText dbText)
            {
                value = dbText.TextString;
                isDBText = true;
            }
            else
            {
                error = "Unsupported object type (not MText or DBText)";
                return false;
            }

            // Extraemos campos
            List<string> originalFields = cls_00_MTextObjectsByLayer.SplitLabelValueByCondAndToken(
                autoSettings, value
            );
            // Validamos
            if (originalFields == null || originalFields.Count == 0)
            {
                error = "No fields found in label";
                return false;
            }

            int count = originalFields.Count;
            string[] reordered = new string[count];
            HashSet<int> usedIndexes = new HashSet<int>();
            // Reordenamos
            for (int i = 0; i < count; i++)
            {
                string fieldKey = cls_00_MTextObjectsByLayer.GetAlphabeticFieldKey(originalFields[i]);
                // Validamos
                if (string.IsNullOrEmpty(fieldKey))
                {
                    error = $"Invalid field format: '{originalFields[i]}'";
                    return false;
                }
                // Validamos
                if (!fieldOrderDict.ContainsKey(fieldKey))
                {
                    error = $"Field '{fieldKey}' not found in order definition";
                    return false;
                }

                // Obtenemos indice
                int newIndex = fieldOrderDict[fieldKey];

                // Validar rango
                if (newIndex < 0 || newIndex >= count)
                {
                    error = $"Index out of range for field '{fieldKey}' → {newIndex}";
                    return false;
                }

                // Validar duplicados
                if (!usedIndexes.Add(newIndex))
                {
                    error = $"Duplicate index detected: {newIndex}";
                    return false;
                }

                // Almacenamos
                reordered[newIndex] = originalFields[i];
            }

            // Validar que no haya nulls
            if (reordered.Any(x => x == null))
            {
                error = "Reordering failed: missing fields in final structure";
                return false;
            }

            // Reconstruimos texto
            char separator = cls_00_MTextObjectsByLayer.GetLabelSeparator(autoSettings, value);
            string newValue = string.Join(separator.ToString(), reordered);

            // Asignamos segun tipo
            if (isMText)
                ((MText)dbObj).Contents = newValue;
            else if (isDBText)
                ((DBText)dbObj).TextString = newValue;

            // return
            return true;
        }

        public static Dictionary<string, string> BuildFieldOrderDictionary(
            List<string> referenceFields
        )
        {
            Dictionary<string, string> fieldOrderDict = new Dictionary<string, string>();
            // Iteramos segun numero de campos
            for (int i = 0; i < referenceFields.Count; i++)
            {
                string fieldValue = referenceFields[i];
                // Extraemos la clave alfabética
                string fieldKey = cls_00_MTextObjectsByLayer.GetAlphabeticFieldKey(fieldValue);
                // Validamos
                if (string.IsNullOrEmpty(fieldKey))
                {
                    MessageBox.Show(
                        $"Invalid field format: '{fieldValue}'", "Invalid Label Format",
                        MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    return null;
                }
                // Validamos
                if (fieldOrderDict.ContainsKey(fieldKey))
                {
                    MessageBox.Show(
                        $"Duplicate field key found: '{fieldKey}'", "Invalid Label Format",
                        MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    return null;
                }

                // Convertimos a string
                fieldOrderDict.Add(fieldKey, i.ToString());
            }

            // return
            return fieldOrderDict;
        }

        //private static Dictionary<string, int> BuildFieldOrderDictionary(
        //    List<string> referenceFields
        //)
        //{
        //    Dictionary<string, int> fieldOrderDict = new Dictionary<string, int>();
        //    // Iteramos segun numero de campos
        //    for (int i = 0; i < referenceFields.Count; i++)
        //    {
        //        string fieldValue = referenceFields[i];
        //        // Extraemos la clave alfabética
        //        string fieldKey = cls_00_MTextObjectsByLayer.GetAlphabeticFieldKey(fieldValue);
        //        // Validamos
        //        if (string.IsNullOrEmpty(fieldKey))
        //        {
        //            // Mensaje
        //            MessageBox.Show(
        //                $"Invalid field format: '{fieldValue}'",
        //                "Invalid Label Format",
        //                MessageBoxButtons.OK,
        //                MessageBoxIcon.Error
        //            );
        //            // Finalizamos
        //            return null;
        //        }
        //        // Validamos duplicados
        //        if (fieldOrderDict.ContainsKey(fieldKey))
        //        {
        //            // Mensaje
        //            MessageBox.Show(
        //                $"Duplicate field key found: '{fieldKey}'",
        //                "Invalid Label Format",
        //                MessageBoxButtons.OK,
        //                MessageBoxIcon.Error
        //            );
        //            // Finalizamos
        //            return null;
        //        }
        //        // Almacenamos
        //        fieldOrderDict.Add(fieldKey, i);
        //    }
        //    // return
        //    return fieldOrderDict;
        //}











    }
}
