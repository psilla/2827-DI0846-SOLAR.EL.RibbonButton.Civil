using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetEntityCoordinates;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_ProcessCT
    {
        public static int ProcessCtByInvLabel(
            SolarSettings solarSet,
            Region regionCT,
            Transaction tr,
            BlockTableRecord btr,
            HashSet<ObjectId> trackersIds,
            Dictionary<string, string> propPreDict,
            int ctStartIndex,
            int trackStartIndex,
            bool isHorizontal,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool MPPtSelBool,
            string charSepSel,
            List<(int invNumber, Region invRegion)> invRegionsOrdered,
            ref HashSet<ObjectId> createdLabelIds
        )
        {
            int totalLabelsCT = 0;

            infoRegiones.AppendLine($"═══════════════════════════════════");
            infoRegiones.AppendLine($" ContGen Region Handle: {regionCT.Handle}");
            infoRegiones.AppendLine($"═══════════════════════════════════");

            // Detectar todos los trackers dentro del CT
            List<Entity> allTrackersInCT = cls_00_GetEntityListByRegion.
                GetEntityListByRegionByPoint(tr, regionCT, trackersIds);
            int totalTrackersCT = allTrackersInCT?.Count ?? 0;
            int totalInvertersCT = invRegionsOrdered.Count;

            // Mostramos 
            infoRegiones.AppendLine($"\t\t• Total Inverters: {totalInvertersCT}");
            infoRegiones.AppendLine($"\t\t• Total Trackers: {totalTrackersCT}");

            // Ordenar trackers por CT
            allTrackersInCT = cls_00_GetEntityCentroid.OrderByColumns(
                allTrackersInCT,
                e => cls_00_GetEntityCentroid.GetEntityCentroid(e)
            );

            // Contador global de trackers por CT
            int trackIndex = trackStartIndex;
            // Iteramos
            foreach (BlockReference tracker in allTrackersInCT)
            {
                // Contar Strings por Tracker
                int stringIndex = 1;
                // Procesamos Tracker
                int? labelsCreated = cls_12_ProcessTrack.ProcessTrack(
                    tr, btr, solarSet, tracker,
                    propPreDict, ctStartIndex, 0,
                    trackIndex, ref stringIndex,
                    isHorizontal, chosenStyle, chosenJustification,
                    infoRegiones, MPPtSelBool, "",
                    totalTrackersCT, charSepSel,
                    ref createdLabelIds
                );
                // Validamos
                if (labelsCreated != null)
                {
                    // Actualizamos contador
                    trackIndex++;
                    totalLabelsCT += labelsCreated.Value;
                }
            }
            // Mostramos 
            infoRegiones.AppendLine($"\t\t• Total Strings: {totalLabelsCT}");
            // return
            return totalLabelsCT;
        }







    }
}
