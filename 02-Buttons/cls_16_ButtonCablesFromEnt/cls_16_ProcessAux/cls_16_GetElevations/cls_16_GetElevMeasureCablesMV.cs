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
            PromptSelectionResult psrCtLab,
            PromptSelectionResult psrCtBlock,
            PromptSelectionResult psrEstBlock,
            PromptSelectionResult psrCtCab,
            out double elevCtLabel,
            out double elevCtBlock,
            out double elevEstBlock,
            out double elevCtCab
        )
        {
            // Valores por defecto
            elevCtLabel = 0;
            elevCtBlock = 0;
            elevEstBlock = 0;
            elevCtCab = 0;

            // Validamos elevaciones
            // LABELS CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrCtLab, solarSet.LabelInvTag, out elevCtLabel
            )) return false;

            // BLOCKREF CT
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrCtBlock, solarSet.BlockRefInvTag, out elevCtBlock
            )) return false;

            // BLOCKREF ESTACION
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrEstBlock, solarSet.BlockRefEstTag, out elevEstBlock
            )) return false;

            // CABLES CT - ESTACION
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrCtCab, solarSet.CableN2Tag, out elevCtCab
            )) return false;

            // return
            return true;
        }
    }
}
