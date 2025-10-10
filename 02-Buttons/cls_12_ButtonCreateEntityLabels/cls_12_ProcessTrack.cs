using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetEntityCoordinates;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_ProcessTrack
    {
        public static int ProcessTrack(
            BlockReference tracker,
            Transaction tr,
            BlockTableRecord btr,
            string stringLayer,
            Dictionary<string, string> propPreDict,
            string contGenProp, int ctStartIndex,
            string contInvProp, int invIndex,
            string trackProp, int trackIndex,
            string stringProp, ref int stringIndex,
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
            ref HashSet<ObjectId> createdLabelIds,
            bool shouldDrawLabels = true
        )
        {
            // Contador de etiquetas creadas en este tracker
            int labelsCreated = 0;

            // Obtenemos strings de este tracker
            List<Polyline> stringsInTrack =
                cls_12_GetStringsInTrack.GetStringsInTrack(tracker, tr, stringLayer);
            // Validamos
            if (stringsInTrack == null || stringsInTrack.Count == 0) return 0;

            // Ordenamos strings por centroide para que también salgan en orden
            stringsInTrack.Sort((a, b) =>
                cls_00_GetEntityCentroid.CompareEntitiesByPosition(a, b, 10.0));

            // Mostramos cada string con numeración continua
            foreach (Polyline str in stringsInTrack)
            {
                ObjectId labelId = cls_12_DrawStringLabel.DrawStringLabel(
                    str, tracker, tr, btr, propPreDict,
                    contGenProp, ctStartIndex, contInvProp, invIndex,
                    trackProp, trackIndex, stringProp, stringIndex,
                    isHorizontal, labelsTrackLayer, chosenStyle,
                    chosenJustification, infoRegiones, MPPtSelBool,
                    numCenTranAsString, numInvAsString,
                    totalTrackersCT, charSepSel, shouldDrawLabels
                );
                // Guardar ID si se generó
                if (labelId != ObjectId.Null)
                    createdLabelIds.Add(labelId);
                // Actualizamos contadores
                stringIndex++;
                if (shouldDrawLabels) labelsCreated++;
                //// Definimos la etiqueta
                //cls_12_DrawStringLabel.DrawStringLabel(
                //    str, tracker, tr, btr, propPreDict,
                //    contGenProp, ctStartIndex, contInvProp, invIndex,
                //    trackProp, trackIndex, stringProp, stringIndex,
                //    isHorizontal, labelsTrackLayer, chosenStyle,
                //    chosenJustification, infoRegiones, MPPtSelBool,
                //    numCenTranAsString, numInvAsString,
                //    totalTrackersCT, charSepSel
                //);
                //// Actualizamos contador
                //stringIndex++;
                //// Aumentamos contador de etiquetas creadas
                //labelsCreated++;
            }
            // return
            return labelsCreated;
        }

        public static int ProcessTrack(
            BlockReference tracker,
            Transaction tr,
            BlockTableRecord btr,
            string stringLayer,
            Dictionary<string, string> propPreDict,
            string contGenProp, int ctStartIndex,
            string contInvProp, int invIndex,
            string trackProp, int trackIndex,
            string stringProp, ref int stringIndex,
            bool isHorizontal,
            string labelsTrackLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool MPPtSelBool,
            string numCenTranAsString,
            int totalTrackersCT,
            string charSepSel,
            ref HashSet<ObjectId> createdLabelIds
        )
        {
            int labelsCreated = 0;

            // Obtener strings de este tracker
            List<Polyline> stringsInTrack =
                cls_12_GetStringsInTrack.GetStringsInTrack(tracker, tr, stringLayer);
            if (stringsInTrack == null || stringsInTrack.Count == 0) return 0;

            // Ordenar strings por posición
            stringsInTrack.Sort((a, b) =>
                cls_00_GetEntityCentroid.CompareEntitiesByPosition(a, b, 10.0));

            foreach (Polyline str in stringsInTrack)
            {
                // Obtenemos la etiqueta
                ObjectId labelId = cls_12_DrawStringLabel.DrawStringLabel(
                    str, tracker, tr, btr, propPreDict,
                    contGenProp, ctStartIndex, contInvProp, invIndex,
                    trackProp, trackIndex, stringProp, stringIndex,
                    isHorizontal, labelsTrackLayer, chosenStyle, chosenJustification,
                    infoRegiones, MPPtSelBool, numCenTranAsString,
                    totalTrackersCT, charSepSel
                );
                // Validamos
                if (labelId != ObjectId.Null)
                    createdLabelIds.Add(labelId);
                // Actualizamos contador
                stringIndex++;
                labelsCreated++;
            }
            // return
            return labelsCreated;
        }




    }
}
