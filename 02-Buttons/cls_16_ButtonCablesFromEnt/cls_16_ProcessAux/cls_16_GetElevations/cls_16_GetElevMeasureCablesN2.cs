using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.GetEntityElevation;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetElevMeasureCablesN2
    {
        public static bool GetElevMeasureCablesN2(
            Transaction tr,
            SolarSettings solarSet,
            PromptSelectionResult psrPolyInv,
            PromptSelectionResult psrLabelInv,
            PromptSelectionResult psrBlockRefInv,
            PromptSelectionResult psrBlockRefCt,
            PromptSelectionResult psrPolyN2Cable,
            out double elevPolyInv,
            out double elevLabelInv,
            out double elevBlockRefInv,
            out double elevBlockRefCt,
            out double elevPolyN2Cable
        )
        {
            // Valores por defecto
            elevPolyInv = 0;
            elevLabelInv = 0;
            elevBlockRefInv = 0;
            elevBlockRefCt = 0;
            elevPolyN2Cable = 0;

            // Validamos elevaciones
            // CONTORNOS INVERSORES
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrPolyInv, solarSet.PolyInvTag, out elevPolyInv
            )) return false;

            // LABELS INVERSORES
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrLabelInv, solarSet.LabelInvTag, out elevLabelInv
            )) return false;

            // BLOCKREF INVERSORES
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrBlockRefInv, solarSet.BlockRefInvTag, out elevBlockRefInv
            )) return false;

            // BLOCKREF CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrBlockRefCt, solarSet.BlockRefInvTag, out elevBlockRefCt
            )) return false;

            // CABLES INVERSOR - CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrPolyN2Cable, solarSet.CableN2Tag, out elevPolyN2Cable
            )) return false;

            // return
            return true;
        }
    }
}
