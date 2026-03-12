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
            Region regionCt,
            Transaction tr,
            BlockTableRecord btr,
            HashSet<ObjectId> psrBlockRefTrackIds,
            Dictionary<string, string> propPreDict,
            int ctStartIndex,
            int trackStartIndex,
            bool isHorizontal,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegion,
            bool MPPtSelBool,
            string charSepSel,
            List<(int invNumber, Region invRegion)> invRegionsOrdered,
            ref HashSet<ObjectId> createdLabelIds
        )
        {
            int totalLabelsCT = 0;

            infoRegion.AppendLine($"═══════════════════════════════════");
            infoRegion.AppendLine($" ContGen Region Handle: {regionCt.Handle}");
            infoRegion.AppendLine($"═══════════════════════════════════");

            // Detectar todos los trackers dentro del CT
            List<Entity> blockRefTrackAsEntListInCt = cls_00_GetEntityListByRegion.
                GetEntityListByRegionByPoint(tr, regionCt, psrBlockRefTrackIds);
            // Obtenemos info
            int blockRefTrackInCtCount = blockRefTrackAsEntListInCt?.Count ?? 0;
            int regionInvInCtCount = invRegionsOrdered.Count;

            // Mostramos 
            infoRegion.AppendLine($"\t\t• Total Inverters: {regionInvInCtCount}");
            infoRegion.AppendLine($"\t\t• Total Trackers: {blockRefTrackInCtCount}");

            // Ordenar trackers por CT
            blockRefTrackAsEntListInCt = cls_00_GetEntityCentroid.OrderByColumns(
                blockRefTrackAsEntListInCt, e => cls_00_GetEntityCentroid.GetEntityCentroid(e)
            );

            // Contador global de trackers por CT
            int trackIndex = trackStartIndex;
            // Iteramos
            foreach (BlockReference blockreftrack in blockRefTrackAsEntListInCt)
            {
                // Contar Strings por Tracker
                int stringIndex = 1;
                // Procesamos Tracker
                int? labelsCreated = cls_12_ProcessTrack.ProcessTrack(
                    tr, btr, solarSet, blockreftrack, propPreDict, ctStartIndex, 0,
                    trackIndex, ref stringIndex, isHorizontal, chosenStyle, chosenJustification,
                    infoRegion, MPPtSelBool, "", blockRefTrackInCtCount, charSepSel, ref createdLabelIds
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
            infoRegion.AppendLine($"\t\t• Total Strings: {totalLabelsCT}");
            // return
            return totalLabelsCT;
        }







    }
}
