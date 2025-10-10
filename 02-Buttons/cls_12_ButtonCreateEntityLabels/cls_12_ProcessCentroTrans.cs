using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.GetEntityCoordinates;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_ProcessCentroTrans
    {
        public static int ProcessCentroTransByCentroid(
            Region regionCT,
            Transaction tr,
            BlockTableRecord btr,
            HashSet<ObjectId> contInvIds,
            Dictionary<Handle, Handle> dictPolyToRegionContInv,
            List<Region> validRegionContInv,
            HashSet<ObjectId> trackersIds,
            HashSet<ObjectId> invLabelIds,
            string trackTag,
            string contInvLabelTag,
            string stringLayer,
            Dictionary<string, string> propPreDict,
            string contGenProp, int ctStartIndex,
            string contInvProp,
            string trackProp,
            string stringProp,
            bool isHorizontal,
            string labelsTrackLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool MPPtSelBool,
            string charSepSel,
            List<(int invNumber, Region invRegion)> invRegionsOrdered,
            ref HashSet<ObjectId> createdLabelIds
        )
        {
            // Contador de etiquetas creadas en todo el Contorno General
            int labelsCreatedTotal = 0;

            // Obtenemos Inversores dentro del CT
            List<Entity> contInvEntities = cls_12_GetInvInRegGen.GetInvInRegGen(
                tr, regionCT, contInvIds, infoRegiones
            );
            // Validamos
            if (contInvEntities == null || contInvEntities.Count == 0) return 0;

            // Ordenamos Inversores por centroide
            contInvEntities.Sort((a, b) =>
                cls_00_GetEntityCentroid.CompareEntitiesByPosition(a, b, 10.0));

            // Obtenemos regiones de inversores
            List<Region> orderedInvRegions =
                cls_12_GetInvRegions.GetInvRegions(
                    contInvEntities, dictPolyToRegionContInv, validRegionContInv, infoRegiones
                );

            // Mostramos
            infoRegiones.AppendLine($"\t• Total Inverter Regions: {orderedInvRegions.Count}");

            // Mostramos
            infoRegiones.AppendLine($"\t- CT Region Handle: {regionCT.Handle}");
            // Obtenemos Trackers dentro del CT
            List<Entity> allTrackersInCT = cls_12_GetTrackInRegInv.
                GetEntInRegByPoint(tr, regionCT, trackersIds);
            // Mostramos
            infoRegiones.AppendLine($"\t\t• Total {trackTag}: {allTrackersInCT.Count}");
            // Contamos
            int totalTrackersCT = (allTrackersInCT != null) ? allTrackersInCT.Count : 0;

            // Contador de inversores dentro del Contorno General
            int invIndex = 1;
            // Contador de trackers dentro del Contorno General
            int trackIndex = 1;
            // Iteramos por las regiones de los Inversores
            foreach (Region invRegion in orderedInvRegions)
            {
                // Procesamos Inverter
                int labelsCreated = cls_12_ProcessInverter.ProcessInverter(
                    invRegion, tr, btr, trackersIds, invLabelIds, trackTag, contInvLabelTag, stringLayer,
                    propPreDict, contGenProp, ctStartIndex,
                    contInvProp, invIndex,
                    trackProp, ref trackIndex,
                    stringProp, isHorizontal, labelsTrackLayer,
                    chosenStyle, chosenJustification, infoRegiones,
                    MPPtSelBool, totalTrackersCT,
                    charSepSel, ref createdLabelIds, allTrackersInCT
                );
                // Actualizamos contador
                invIndex++;
                // Acumulamos etiquetas
                labelsCreatedTotal += labelsCreated;
            }
            // return
            return labelsCreatedTotal;
        }

        //public static int ProcessCentroTransByInvLabel(
        //    Region regionCT,
        //    Transaction tr,
        //    BlockTableRecord btr,
        //    HashSet<ObjectId> trackersIds,
        //    HashSet<ObjectId> invLabelIds,
        //    string trackTag,
        //    string contInvLabelTag,
        //    string stringLayer,
        //    Dictionary<string, string> propPreDict,
        //    string contGenProp, int ctStartIndex,
        //    string contInvProp,
        //    string trackProp,
        //    string stringProp,
        //    bool isHorizontal,
        //    string labelsTrackLayer,
        //    string chosenStyle,
        //    AttachmentPoint chosenJustification,
        //    StringBuilder infoRegiones,
        //    bool MPPtSelBool,
        //    string charSepSel,
        //    List<(int invNumber, Region invRegion)> invRegionsOrdered,
        //    ref HashSet<ObjectId> createdLabelIds
        //)
        //{
        //    // Contador de etiquetas creadas en el CT
        //    int labelsCreatedTotal = 0;

        //    // Mostramos
        //    infoRegiones.AppendLine($"\t- CT Region Handle: {regionCT.Handle}");

        //    // Obtenemos todos los Trackers dentro del CT
        //    List<Entity> allTrackersInCT = cls_12_GetTrackInRegInv.
        //        GetEntInRegByPoint(tr, regionCT, trackersIds);
        //    // Ordenamos lista de Trackers
        //    allTrackersInCT = cls_00_GetEntityCentroid.OrderByColumns(
        //        allTrackersInCT,
        //        e => cls_00_GetEntityCentroid.GetEntityCentroid(e)
        //    );

        //    // Contamos Inversores y Trackers
        //    int totalInvertersCT = invRegionsOrdered.Count;
        //    int totalTrackersCT = allTrackersInCT?.Count ?? 0;

        //    // Mostramos
        //    infoRegiones.AppendLine($"\t\t• Total {trackTag}: {allTrackersInCT.Count}");
        //    infoRegiones.AppendLine($"\t\t• Total Inverters: {totalInvertersCT}");

        //    // Contadores
        //    int invIndex = 1;
        //    int trackIndex = 1;
        //    // Iteramos por las regiones de los inversores en el orden correcto
        //    foreach (var (invNumber, invRegion) in invRegionsOrdered)
        //    {
        //        // Validamos
        //        if (invRegion == null) continue;
        //        // Procesamos el inversor
        //        int labelsCreated = cls_12_ProcessInverter.ProcessInverter(
        //            invRegion, tr, btr, trackersIds, invLabelIds,
        //            trackTag, contInvLabelTag, stringLayer, propPreDict,
        //            contGenProp, ctStartIndex, contInvProp, invIndex,
        //            trackProp, ref trackIndex, stringProp, isHorizontal,
        //            labelsTrackLayer, chosenStyle, chosenJustification,
        //            infoRegiones, MPPtSelBool,
        //            totalTrackersCT, charSepSel, ref createdLabelIds,
        //            allTrackersInCT
        //        );
        //        // Actualizamos contador
        //        invIndex++;
        //        // Acumulamos etiquetas
        //        labelsCreatedTotal += labelsCreated;
        //    }
        //    // return
        //    return labelsCreatedTotal;
        //}

        public static int ProcessCentroTransByInvLabel(
            Region regionCT,
            Transaction tr,
            BlockTableRecord btr,
            HashSet<ObjectId> trackersIds,
            string stringLayer,
            Dictionary<string, string> propPreDict,
            string contGenProp, 
            int ctStartIndex,
            int trackStartIndex,
            string contInvProp,
            string trackProp,
            string stringProp,
            bool isHorizontal,
            string labelsTrackLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool MPPtSelBool,
            string charSepSel,
            List<(int invNumber, Region invRegion)> invRegionsOrdered,
            ref HashSet<ObjectId> createdLabelIds
        )
        {
            int labelsCreatedTotal = 0;

            infoRegiones.AppendLine($"\t- CT Region Handle: {regionCT.Handle}");

            // 🧭 Detectar todos los trackers dentro del CT
            List<Entity> allTrackersInCT = cls_12_GetTrackInRegInv.GetEntInRegByPoint(tr, regionCT, trackersIds);
            int totalTrackersCT = allTrackersInCT?.Count ?? 0;
            int totalInvertersCT = invRegionsOrdered.Count;

            infoRegiones.AppendLine($"\t\t• Total Trackers: {totalTrackersCT}");
            infoRegiones.AppendLine($"\t\t• Total Inverters: {totalInvertersCT}");

            // Mostrar inversores detectados ANTES de procesar trackers
            if (invRegionsOrdered.Count > 0)
            {
                infoRegiones.AppendLine($"\t\t• Inverters in CT:");
                foreach (var (invNumber, invRegion) in invRegionsOrdered)
                {
                    if (invRegion == null) continue;
                    infoRegiones.AppendLine($"\t\t\t- Inverter {invNumber} (Handle: {invRegion.Handle})");
                }
            }

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
                int labelsCreated = cls_12_ProcessTrack.ProcessTrack(
                    tracker,
                    tr,
                    btr,
                    stringLayer,
                    propPreDict,
                    contGenProp,
                    ctStartIndex,
                    contInvProp,
                    0,                   
                    trackProp,
                    trackIndex,
                    stringProp,
                    ref stringIndex,
                    isHorizontal,
                    labelsTrackLayer,
                    chosenStyle,
                    chosenJustification,
                    infoRegiones,
                    MPPtSelBool,
                    "",                  
                    totalTrackersCT,
                    charSepSel,
                    ref createdLabelIds
                );
                // Actualizamos contador
                trackIndex++;
                labelsCreatedTotal += labelsCreated;
            }

            

            return labelsCreatedTotal;
        }







    }
}
