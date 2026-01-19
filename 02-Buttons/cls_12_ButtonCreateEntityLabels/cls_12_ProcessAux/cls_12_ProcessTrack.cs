using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.GetEntities;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_ProcessTrack
    {
        public static int? ProcessTrack(
            Transaction tr,
            BlockTableRecord btr,
            SolarSettings solarSet,
            BlockReference tracker,
            Dictionary<string, string> propPreDict,
            int ctStartIndex,
            int invIndex, 
            int trackIndex,
            ref int stringIndex,
            bool isHorizontal,
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
            List<Polyline> stringsInTrack = cls_00_GetNestedPolysInBlockRef.
                GetNestedPolysInBlockRef(tracker, tr, solarSet.PolyStringLayer);
            // Validamos
            if (stringsInTrack == null || stringsInTrack.Count == 0) return null;

            // Iteramos
            foreach (Polyline str in stringsInTrack)
            {
                // Procesamos String
                ObjectId labelId = cls_12_ProcessString.ProcessString(
                    tr, btr, solarSet, str, tracker, propPreDict,
                    ctStartIndex, invIndex, trackIndex, stringIndex,
                    isHorizontal, chosenStyle, chosenJustification,
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
