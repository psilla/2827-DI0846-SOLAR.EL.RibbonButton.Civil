using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Civil.GetEntityCoordinates;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_ProcessTrack
    {
        public static int ProcessTrack(
            BlockReference tracker,
            Transaction tr,
            BlockTableRecord btr,
            string stringLayer,
            Dictionary<string, string> propPreDict,
            string contGenProp, int contGenIndex,
            string contInvProp, int invIndex,
            string trackProp, int trackIndex,
            string stringProp, ref int stringIndex,
            bool isHorizontal,
            string labelsLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool dolarSelBool
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
                // Definimos la etiqueta
                cls_12_DrawStringLabel.DrawStringLabel(
                    str, tracker, tr, btr, propPreDict,
                    contGenProp, contGenIndex, contInvProp, invIndex,
                    trackProp, trackIndex, stringProp, stringIndex,
                    isHorizontal, labelsLayer, chosenStyle,
                    chosenJustification, infoRegiones, dolarSelBool
                );
                // Actualizamos contador
                stringIndex++;

                // Aumentamos contador de etiquetas creadas
                labelsCreated++;
            }
            // return
            return labelsCreated;
        }



    }
}
