using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_RemoveTrackFieldFromLabel
    {
        public static void RemoveTrackerFieldFromLabel(
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
            string fullText = mText.Contents ?? string.Empty;
            // Split por el espacio y cogemos primera parte
            int spaceIndex = fullText.IndexOf(' ');
            string cleanLabel = spaceIndex > -1
                ? fullText.Substring(0, spaceIndex)
                : fullText;

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





    }
}
