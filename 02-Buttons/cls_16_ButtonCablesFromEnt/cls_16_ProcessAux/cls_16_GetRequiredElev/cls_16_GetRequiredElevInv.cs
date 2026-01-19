using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.GetEntityElevation;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetRequiredElevInv
    {
        public static bool GetRequiredElevInv(
            Transaction tr,
            SolarSettings solarSet,
            PromptSelectionResult psrContInv,
            PromptSelectionResult psrInvLab,
            PromptSelectionResult psrInvBlock,
            PromptSelectionResult psrInvCab,
            out double elevInvCont,
            out double elevInvLabel,
            out double elevInvBlock,
            out double elevInvCab
        )
        {
            // Valores por defecto
            elevInvCont = 0.0;
            elevInvLabel = 0.0;
            elevInvBlock = 0.0;
            elevInvCab = 0.0;

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

            // CABLES INVERSOR - CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                    tr, psrInvCab, solarSet.CableInvToCtTag, out elevInvCab
                )) return false;

            // return
            return true;
        }
    }
}
