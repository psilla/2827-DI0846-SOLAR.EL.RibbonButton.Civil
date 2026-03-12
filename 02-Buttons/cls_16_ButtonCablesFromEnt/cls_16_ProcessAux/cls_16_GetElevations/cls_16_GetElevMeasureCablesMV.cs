using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.GetEntityElevation;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetElevMeasureCablesMV
    {
        public static bool GetElevMeasureCablesMV(
            Transaction tr,
            SolarSettings solarSet,
            PromptSelectionResult psrLabelCt,
            PromptSelectionResult psrBlockRefCt,
            PromptSelectionResult psrBlockRefEst,
            PromptSelectionResult psrPolyMvCable,
            out double elevLabelCt,
            out double elevBlockRefCt,
            out double elevBlockRefEst,
            out double elevPolyMvCable
        )
        {
            // Valores por defecto
            elevLabelCt = 0;
            elevBlockRefCt = 0;
            elevBlockRefEst = 0;
            elevPolyMvCable = 0;

            // Validamos elevaciones
            // LABELS CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrLabelCt, solarSet.LabelInvTag, out elevLabelCt
            )) return false;

            // BLOCKREF CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrBlockRefCt, solarSet.BlockRefInvTag, out elevBlockRefCt
            )) return false;

            // BLOCKREF ESTACION
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrBlockRefEst, solarSet.BlockRefEstTag, out elevBlockRefEst
            )) return false;

            // CABLES CT - ESTACION
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrPolyMvCable, solarSet.CableN2Tag, out elevPolyMvCable
            )) return false;

            // return
            return true;
        }
    }
}
