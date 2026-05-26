using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetEntityCoordinates;

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
            Dictionary<string, string> labelFieldsDict,
            Dictionary<Region, string> invInCtLayerByInvRegion,
            int ctStartIndex,
            int trackStartIndex,
            bool isHorizontal,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegion,
            bool MPPtSelBool,
            string charSepSel,
            List<(int invNumber, Region invRegion)> invRegionsOrdered,
            bool inverterOutsideCt,
            ref HashSet<ObjectId> createdLabelIds
        )
        {
            int totalLabelsCT = 0;

            infoRegion.AppendLine($"═══════════════════════════════════");
            infoRegion.AppendLine($" ContGen Region Handle: {regionCt.Handle}");
            infoRegion.AppendLine($"═══════════════════════════════════");

            // -----------------------------
            // Detectar trackers dentro del CT
            // -----------------------------

            List<Entity> blockRefTrackAsEntListInCt = cls_00_GetEntityListByRegion.GetEntityListByRegionByPoint(
                tr, regionCt, psrBlockRefTrackIds
            );

            // -----------------------------
            // Obtener info
            // -----------------------------
          
            int blockRefTrackInCtCount = blockRefTrackAsEntListInCt?.Count ?? 0;
            int regionInvInCtCount = invRegionsOrdered.Count;

            // Mostramos 
            infoRegion.AppendLine($"\t\t• Total Inverters: {regionInvInCtCount}");
            infoRegion.AppendLine($"\t\t• Total Trackers: {blockRefTrackInCtCount}");

            // -----------------------------
            // Ordenar trackers por CT
            // -----------------------------

            blockRefTrackAsEntListInCt = cls_00_GetEntityCentroid.OrderByColumns(
                blockRefTrackAsEntListInCt, e => cls_00_GetEntityCentroid.GetEntityCentroid(e)
            );

            // -----------------------------
            // Contador global de trackers por CT
            // -----------------------------

            int trackIndex = trackStartIndex;

            // -----------------------------
            // Iteramos trackers
            // -----------------------------

            foreach (BlockReference blockreftrack in blockRefTrackAsEntListInCt)
            {
                // -----------------------------
                // Contar Strings por Tracker
                // -----------------------------

                int stringIndex = 1;

                // -----------------------------
                // Obtener layer Inversor en CT
                // -----------------------------

                string invInCtLayer = "";
                // Dentro del CT
                if (!inverterOutsideCt)
                {
                    // Obtener punto tracker
                    Point3d trackPoint = blockreftrack.Position;
                    // Buscar region inversor
                    foreach (var invTuple in invRegionsOrdered)
                    {
                        // Obtener region
                        Region invRegion = invTuple.invRegion;
                        // Validamos
                        if (
                            invRegion == null || invInCtLayerByInvRegion == null ||
                            !invInCtLayerByInvRegion.ContainsKey(invRegion)
                        ) continue;
                       
                        // Validar punto dentro region
                        if (cls_00_GetElemByRegionByBrep.PointByRegionByBrep(trackPoint, invRegion)
                        )
                        {
                            // Obtener layer
                            invInCtLayer = invInCtLayerByInvRegion[invRegion];
                            break;
                        }
                    }
                }

                // Procesamos Tracker
                int? labelsCreated = cls_12_ProcessTrack.ProcessTrack(
                    tr, btr, solarSet, blockreftrack, labelFieldsDict, invInCtLayer, ctStartIndex, 0,
                    trackIndex, ref stringIndex, isHorizontal, chosenStyle, chosenJustification,
                    infoRegion, MPPtSelBool, "", blockRefTrackInCtCount, charSepSel, inverterOutsideCt,
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
            infoRegion.AppendLine($"\t\t• Total Strings: {totalLabelsCT}");
            // return
            return totalLabelsCT;
        }







    }
}
