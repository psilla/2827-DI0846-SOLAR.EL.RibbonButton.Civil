using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_RemoveFieldFromLabel
    {
        public static void RemoveFieldFromLabel(
            Transaction tr,
            ObjectId lblId,
            string charSepSel,
            StringBuilder infoRegiones
        )
        {
            // Obtenemos el texto
            Entity ent = tr.GetObject(lblId, OpenMode.ForRead) as Entity;
            // Validamos
            if (ent == null || !(ent is MText mText)) return;

            // Obtenemos contenido
            string cleanLabel = GetCleanLabel(mText.Contents);

            // Obtenemos los campos
            string[] fields = cleanLabel.Split(new[] { charSepSel }, StringSplitOptions.None);

            // Eliminamos el campo del Tracker (penultimo)
            List<string> tempParts = fields.ToList();
            tempParts.RemoveAt(tempParts.Count - 2);

            // Juntamos todos los campos de nuevo
            string newText = string.Join(charSepSel, tempParts) + " +/-";

            // Actualizamos valor
            UpdateMTextContents(
                mText,
                newText,
                infoRegiones,
                "Tracker field removed",
                "Error removing tracker field"
            );
        }

        public static void UpdateMTextContents(
            MText mText,
            string newText,
            StringBuilder infoRegiones,
            string successMsg = "Label updated",
            string errorMsg = "Error updating label"
        )
        {
            // try
            try
            {
                // Actualizamos valor
                if (!mText.IsWriteEnabled) mText.UpgradeOpen();
                mText.Contents = newText;
                mText.RecordGraphicsModified(true);
                mText.DowngradeOpen();
                // Mostramos
                infoRegiones.AppendLine(
                    $"\t🧹 {successMsg}: {mText.Handle} → {newText}"
                );
            }
            // catch
            catch (Exception ex)
            {
                // Mostramos
                infoRegiones.AppendLine(
                    $"\t⚠ {errorMsg} {mText.Handle}: {ex.Message}"
                );
            }
        }

        public static string GetCleanLabel(string text)
        {
            // Validamos
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            // Split por el espacio
            int spaceIndex = text.IndexOf(' ');
            // return
            return spaceIndex > -1
                ? text.Substring(0, spaceIndex)
                : text;
        }

        public static string[] SplitLabelFields(string cleanLabel, string charSepSel)
        {
            // Validamos
            if (string.IsNullOrEmpty(cleanLabel))
                // Finalizamos
                return Array.Empty<string>();
            // return
            return cleanLabel.Split(new[] { charSepSel }, StringSplitOptions.None);
        }





    }
}
