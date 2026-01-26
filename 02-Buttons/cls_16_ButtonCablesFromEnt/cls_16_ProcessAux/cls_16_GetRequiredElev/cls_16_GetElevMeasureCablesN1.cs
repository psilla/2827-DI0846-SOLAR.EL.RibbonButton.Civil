using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.GetEntityElevation;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetElevMeasureCablesN1
    {
        public static bool GetElevMeasureCablesN1(
            Transaction tr,
            SolarSettings solarSet,
            PromptSelectionResult psrString,
            PromptSelectionResult psrStringLab,
            PromptSelectionResult psrStringCab,
            out double elevString,
            out double elevStringLabel,
            out double elevStringCab
        )
        {
            // Valores por defecto
            elevString = 0;
            elevStringLabel = 0;
            elevStringCab = 0;

            // Validamos elevaciones
            // CONTORNOS STRINGS
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrString, solarSet.PolyStringTag, out elevString
            )) return false;

            // LABELS STRINGS
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrStringLab, solarSet.LabelStringTag, out elevStringLabel
            )) return false;

            // CABLES STRING - INVERSOR
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrStringCab, solarSet.CableStringToInvTag, out elevStringCab
            )) return false;

            // return
            return true;
        }
    }
}
