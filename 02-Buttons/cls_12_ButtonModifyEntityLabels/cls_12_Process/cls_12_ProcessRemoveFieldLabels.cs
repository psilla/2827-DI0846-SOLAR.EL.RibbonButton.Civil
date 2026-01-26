using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.UserForms;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_ProcessRemoveFieldLabels
    {
        public static int? ProcessRemoveFieldLabels(
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

                // Texto a eliminar
                string textToRemove = InstanciarFormularios.TextBoxFormOut(
                    "Enter the text to remove from all labels:", "+/-"
                );
                // Validamos
                if (string.IsNullOrWhiteSpace(textToRemove))
                {
                    // Mensaje
                    MessageBox.Show(
                        "No text was provided. The process has been cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return null;
                }

                int modifiedCount = 0;
                // Iteramos
                foreach (ObjectId id in psrStringLabIds)
                {
                    // Modificamos la etiqueta
                    if (!RemoveTextFromLabel(tr, id, textToRemove, out bool wasModified)) return null;
                    // Contamos
                    if (wasModified) modifiedCount++;
                }
                // return
                return modifiedCount;
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show($"ERROR in ProcessRemoveFieldLabels:\n{ex.Message}\n{ex.StackTrace}");
                // Finalizamos
                return null;
            }
        }

        private static bool RemoveTextFromLabel(
            Transaction tr,
            ObjectId labelId,
            string textToRemove,
            out bool wasModified
        )
        {
            wasModified = false;

            // Obtenemos el texto
            DBObject dbObj = tr.GetObject(labelId, OpenMode.ForWrite);
            MText mText = dbObj as MText;
            // Validamos
            if (mText == null) return false;

            string originalValue = mText.Contents;
            // Validamos
            if (string.IsNullOrWhiteSpace(originalValue))
                return true; // No error, simplemente no hay nada que hacer

            if (!originalValue.Contains(textToRemove))
                return true; // No se modifica, pero es valido

            // Definimos valor modificado
            string newValue = originalValue.Replace(textToRemove, string.Empty);

            // Limpieza básica (espacios duplicados)
            newValue = System.Text.RegularExpressions.Regex
                .Replace(newValue, @"\s{2,}", " ")
                .Trim();
            // Asignamos
            mText.Contents = newValue;
            wasModified = true;
            // return
            return true;
        }
















    }
}
