using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using TYPSA.SharedLib.Autocad.DrawEntities;
using System;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_DrawStringLabel
    {
        public static ObjectId DrawStringLabel(
            Polyline str,
            BlockReference tracker,
            Transaction tr,
            BlockTableRecord btr,
            Dictionary<string, string> propPreDict,
            string contGenProp,
            int ctStartIndex,
            string contInvProp,
            int invIndex,
            string trackProp,
            int trackIndex,
            string stringProp,
            int stringIndex,
            bool isHorizontal,
            string labelsTrackLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool MPPtSelBool,
            string numCenTranAsString,
            string numInvAsString,
            int totalTrackersCT,
            string charSepSel,
            bool shouldDrawLabels = true
        )
        {
            ObjectId labelId = ObjectId.Null;

            // Calcular número de dígitos necesarios según el total de trackers
            int trackDigits = Math.Max(2, totalTrackersCT.ToString().Length);
            // Formatear índice del tracker con ceros a la izquierda
            string trackIndexStr = trackIndex.ToString($"D{trackDigits}");

            // Traemos valores por defecto de las etiquetas de inversores
            string regionPart = string.IsNullOrWhiteSpace(numCenTranAsString)
                ? ctStartIndex.ToString("D2")
                : numCenTranAsString.PadLeft(2, '0'); 
            string inverterPart = string.IsNullOrWhiteSpace(numInvAsString)
                ? invIndex.ToString("D2")
                : numInvAsString.PadLeft(2, '0');

            // Sustituimos por 'X' si no se deben mostrar números reales
            string displayInvPart = shouldDrawLabels ? inverterPart : new string('X', inverterPart.Length);
            string displayStrPart = shouldDrawLabels ? stringIndex.ToString("D2") : new string('X', 2);

            // Construimos la parte del inversor
            string invPart = $"{propPreDict[contInvProp]}{displayInvPart}";
            invPart += MPPtSelBool
                ? $"{charSepSel}${charSepSel}" 
                : $"{charSepSel}";

            // Construimos la etiqueta final (siempre incluye Tracker)
            string tagText =
                $"{propPreDict[contGenProp]}{regionPart}{charSepSel}" +
                $"{invPart}" +
                $"{propPreDict[trackProp]}{trackIndexStr}{charSepSel}" +
                $"{propPreDict[stringProp]}{displayStrPart} +/-";

            // try
            try
            {
                // Dibujamos siempre (solo cambia el contenido)
                Point3d basePoint = isHorizontal
                    ? cls_12_GetBottomEdgeMidPoint.GetBottomEdgeLeftPoint(str, tracker)
                    : cls_12_GetBottomEdgeMidPoint.GetBottomEdgeMidPoint(str, tracker);

                labelId = cls_00_DrawEntities.DrawMTextOnPoint(
                    basePoint, tagText, tr, btr,
                    isHorizontal, 1, 7, labelsTrackLayer,
                    chosenStyle, chosenJustification
                );
            }
            catch (System.Exception) { }

            // Mostrar info
            infoRegiones.AppendLine(
                $"\t\t\t- String {stringIndex:D2} en Tracker {trackIndex} ({tracker.Handle})" +
                (shouldDrawLabels ? "" : " [Etiqueta placeholder con X]")
            );

            // return
            return labelId;

            //// Definir el separador del inversor según configuración
            //string invPart = $"{propPreDict[contInvProp]}{inverterPart}";
            //invPart += MPPtSelBool ? $"{charSepSel}${charSepSel}" : $"{charSepSel}";

            //// Construimos la etiqueta final (siempre incluye Tracker)
            //string tagText =
            //    $"{propPreDict[contGenProp]}{regionPart}{charSepSel}" +
            //    $"{invPart}" +
            //    $"{propPreDict[trackProp]}{trackIndexStr}{charSepSel}" +
            //    $"{propPreDict[stringProp]}{displayStrPart} +/-";

            //// try
            //try
            //{
            //    if (shouldDrawLabels)
            //    {   // Calcular punto en funcion de la orientacion
            //        Point3d basePoint = isHorizontal
            //            ? cls_12_GetBottomEdgeMidPoint.GetBottomEdgeLeftPoint(str, tracker)
            //            : cls_12_GetBottomEdgeMidPoint.GetBottomEdgeMidPoint(str, tracker);
            //        // Dibujar etiqueta en coordenadas globales
            //        cls_00_DrawEntities.DrawMTextOnPoint(
            //            basePoint, tagText, tr, btr,
            //            isHorizontal, 1, 7, labelsTrackLayer,
            //            chosenStyle, chosenJustification
            //        );
            //    }
            //}
            //// catch
            //catch (System.Exception ex) { }
            //// Mostrar info
            //infoRegiones.AppendLine($"\t\t\t- String {stringIndex:D2} en Tracker {trackIndex} ({tracker.Handle})");
        }

        public static ObjectId DrawStringLabel(
            Polyline str,
            BlockReference tracker,
            Transaction tr,
            BlockTableRecord btr,
            Dictionary<string, string> propPreDict,
            string contGenProp,
            int ctStartIndex,
            string contInvProp,
            int invIndex,
            string trackProp,
            int trackIndex,
            string stringProp,
            int stringIndex,
            bool isHorizontal,
            string labelsTrackLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool MPPtSelBool,
            string numCenTranAsString,
            int totalTrackersCT,
            string charSepSel
        )
        {
            ObjectId labelId = ObjectId.Null;

            // Número de dígitos para track
            int trackDigits = Math.Max(2, totalTrackersCT.ToString().Length);
            string trackIndexStr = trackIndex.ToString($"D{trackDigits}");

            // Número de dígitos para inversor y string
            int invDigits = Math.Max(2, invIndex.ToString().Length);
            int strDigits = Math.Max(2, stringIndex.ToString().Length);

            string regionPart = string.IsNullOrWhiteSpace(numCenTranAsString)
                ? ctStartIndex.ToString("D2")
                : numCenTranAsString.PadLeft(2, '0');

            // Siempre reemplazamos por X según la longitud
            string displayInvPart = new string('X', invDigits);
            string displayStrPart = new string('X', strDigits);

            string invPart = $"{propPreDict[contInvProp]}{displayInvPart}";
            invPart += MPPtSelBool
                ? $"{charSepSel}${charSepSel}"
                : $"{charSepSel}";

            // Construimos la etiqueta final (siempre incluye Tracker)
            string tagText =
                $"{propPreDict[contGenProp]}{regionPart}{charSepSel}" +
                $"{invPart}" +
                $"{propPreDict[trackProp]}{trackIndexStr}{charSepSel}" +
                $"{propPreDict[stringProp]}{displayStrPart} +/-";

            // try
            try
            {
                // Definimos pto insercion de la etiqueta
                Point3d basePoint = isHorizontal
                    ? cls_12_GetBottomEdgeMidPoint.GetBottomEdgeLeftPoint(str, tracker)
                    : cls_12_GetBottomEdgeMidPoint.GetBottomEdgeMidPoint(str, tracker);
                // Dibujamos etiqueta
                labelId = cls_00_DrawEntities.DrawMTextOnPoint(
                    basePoint, tagText, tr, btr,
                    isHorizontal, 1, 7, labelsTrackLayer,
                    chosenStyle, chosenJustification
                );
            }
            // catch
            catch (System.Exception) { }
            // Mostramos
            infoRegiones.AppendLine(
                $"\t\t\t- String {stringIndex:D2} en Tracker {trackIndex} ({tracker.Handle}) [Etiqueta con X]"
            );
            // return
            return labelId;
        }





    }
}
