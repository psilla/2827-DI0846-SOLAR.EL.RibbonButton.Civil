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
            BlockReference blockreftrack,
            Dictionary<string, string> labelFieldsDict,
            string invInCtLayer,
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
            bool inverterOutsideCt,
            ref HashSet<ObjectId> createdLabelIds
        )
        {
            int labelsCreated = 0;

            // Obtener strings de este blockreftrack
            List<Polyline> polyStrListInTrack = cls_00_GetNestedPolysInBlockRef.
                GetNestedPolysInBlockRef(blockreftrack, tr, solarSet.PolyStringLayer);
            // Validamos
            if (polyStrListInTrack == null || polyStrListInTrack.Count == 0) return null;

            // Iteramos
            foreach (Polyline polyStr in polyStrListInTrack)
            {
                // Procesamos String
                ObjectId labelId = cls_12_ProcessString.ProcessString(
                    tr, btr, solarSet, polyStr, blockreftrack, labelFieldsDict, invInCtLayer, ctStartIndex, invIndex, 
                    trackIndex, stringIndex, isHorizontal, chosenStyle, chosenJustification,
                    infoRegiones, MPPtSelBool, numCenTranAsString, totalTrackersCT, charSepSel, inverterOutsideCt
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
