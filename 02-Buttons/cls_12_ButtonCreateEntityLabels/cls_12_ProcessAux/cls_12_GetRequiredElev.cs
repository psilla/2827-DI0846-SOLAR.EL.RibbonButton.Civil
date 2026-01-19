using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.GetEntityElevation;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_GetRequiredElev
    {
        public static bool GetRequiredElevations(
            Transaction tr,
            SolarSettings solarSet,
            PromptSelectionResult psrContGen,
            PromptSelectionResult psrContInv,
            PromptSelectionResult psrTrackers,
            PromptSelectionResult psrInvLabels,
            out double elevCenTrans,
            out double elevInv,
            out double elevTrack,
            out double elevInvLabel
        )
        {
            // Valores por defecto
            elevCenTrans = 0.0;
            elevInv = 0.0;
            elevTrack = 0.0;
            elevInvLabel = 0.0;

            // Validamos elevaciones
            // CONTORNOS CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrContGen, solarSet.PolyCtTag, out elevCenTrans
            )) return false;

            // CONTORNOS INVERSORES
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrContInv, solarSet.PolyInvTag, out elevInv
            )) return false;

            // BLOCKREF TRACKERS
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrTrackers, solarSet.BlockRefTrackTag, out elevTrack
            )) return false;

            // LABELS INVERSORES
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrInvLabels, solarSet.LabelInvTag, out elevInvLabel
            )) return false;

            // return
            return true;
        }

    }
}
