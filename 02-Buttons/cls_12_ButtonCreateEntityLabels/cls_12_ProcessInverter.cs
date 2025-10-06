using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Civil.GetEntityCoordinates;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_ProcessInverter
    {
        public static int ProcessInverter(
            Region invRegion,
            Transaction tr,
            BlockTableRecord btr,
            HashSet<ObjectId> trackersIds,
            string trackTag,
            string stringLayer,
            Dictionary<string, string> propPreDict,
            string contGenProp, int contGenIndex,
            string contInvProp, int invIndex,
            string trackProp, ref int trackIndex,
            string stringProp,
            bool isHorizontal,
            string labelsLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool dolarSelBool
        )
        {
            // Contador de etiquetas creadas dentro de este inversor
            int labelsCreatedTotal = 0;

            // Obtenemos los trackers de la región
            List<Entity> trackerEntities = cls_12_GetTrackInRegInv.GetTrackInRegInv(
                tr, invRegion, trackersIds, trackTag, infoRegiones
            );
            // Validamos
            if (trackerEntities == null || trackerEntities.Count == 0) return 0;

            // Ordenamos lista de Trackers por centroide
            trackerEntities = cls_00_GetEntityCentroid.OrderByColumns(
                trackerEntities,
                e => cls_00_GetEntityCentroid.GetEntityCentroid(e)
            );

            // Contador de strings global dentro del inversor
            int stringIndex = 1;
            // Iteramos trackers
            foreach (BlockReference tracker in trackerEntities)
            {
                // Procesamos el Tracker
                int labelsCreated = cls_12_ProcessTrack.ProcessTrack(
                    tracker, tr, btr, stringLayer, propPreDict,
                    contGenProp, contGenIndex,
                    contInvProp, invIndex,
                    trackProp, trackIndex,
                    stringProp, ref stringIndex,
                    isHorizontal, labelsLayer,
                    chosenStyle, chosenJustification, infoRegiones,
                    dolarSelBool
                );
                // Actualizamos contador
                trackIndex++;

                // Sumamos al total
                labelsCreatedTotal += labelsCreated;
            }
            // return
            return labelsCreatedTotal;
        }






    }
}
