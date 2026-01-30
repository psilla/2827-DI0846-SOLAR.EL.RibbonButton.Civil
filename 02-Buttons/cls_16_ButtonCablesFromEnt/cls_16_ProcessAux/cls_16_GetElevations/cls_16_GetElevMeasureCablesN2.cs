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
            PromptSelectionResult psrContInv,
            PromptSelectionResult psrInvLab,
            PromptSelectionResult psrInvBlock,
            PromptSelectionResult psrCtBlock,
            PromptSelectionResult psrInvCab,
            out double elevInvCont,
            out double elevInvLabel,
            out double elevInvBlock,
            out double elevCtBlock,
            out double elevInvCab
        )
        {
            // Valores por defecto
            elevInvCont = 0;
            elevInvLabel = 0;
            elevInvBlock = 0;
            elevCtBlock = 0;
            elevInvCab = 0;

            // Validamos elevaciones
            // CONTORNOS INVERSORES
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrContInv, solarSet.PolyInvTag, out elevInvCont
            )) return false;

            // LABELS INVERSORES
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrInvLab, solarSet.LabelInvTag, out elevInvLabel
            )) return false;

            // BLOCKREF INVERSORES
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrInvBlock, solarSet.BlockRefInvTag, out elevInvBlock
            )) return false;

            // BLOCKREF CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrCtBlock, solarSet.BlockRefInvTag, out elevCtBlock
            )) return false;

            // CABLES INVERSOR - CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrInvCab, solarSet.CableN2Tag, out elevInvCab
            )) return false;

            // return
            return true;
        }
    }
}
