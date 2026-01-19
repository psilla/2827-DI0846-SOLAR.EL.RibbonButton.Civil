using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.GetEntityElevation;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetRequiredElevCt
    {
        public static bool GetRequiredElevCt(
            Transaction tr,
            SolarSettings solarSet,
            PromptSelectionResult psrCtLab,
            PromptSelectionResult psrCtBlock,
            PromptSelectionResult psrCtCab,
            out double elevCtLabel,
            out double elevCtBlock,
            out double elevCtCab
        )
        {
            // Valores por defecto
            elevCtLabel = 0.0;
            elevCtBlock = 0.0;
            elevCtCab = 0.0;

            // Validamos elevaciones
            // LABELS CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                    tr, psrCtLab, solarSet.LabelInvTag, out elevCtLabel
                )) return false;

            // BLOCKREF CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                    tr, psrCtBlock, solarSet.BlockRefInvTag, out elevCtBlock
                )) return false;

            // CABLES CT - ESTACION
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                    tr, psrCtCab, solarSet.CableInvToCtTag, out elevCtCab
                )) return false;

            // return
            return true;
        }
    }
}
