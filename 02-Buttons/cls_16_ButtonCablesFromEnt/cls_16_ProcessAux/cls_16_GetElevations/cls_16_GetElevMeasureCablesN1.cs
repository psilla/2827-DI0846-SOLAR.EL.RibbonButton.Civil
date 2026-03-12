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
            PromptSelectionResult psrPolyStr,
            PromptSelectionResult psrLabelStr,
            PromptSelectionResult psrPolyN1Cable,
            out double elevPolyStr,
            out double elevLabelStr,
            out double elevPolyN1Cable
        )
        {
            // Valores por defecto
            elevPolyStr = 0;
            elevLabelStr = 0;
            elevPolyN1Cable = 0;

            // Validamos elevaciones
            // CONTORNOS STRINGS
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrPolyStr, solarSet.PolyStringTag, out elevPolyStr
            )) return false;

            // LABELS STRINGS
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrLabelStr, solarSet.LabelStringTag, out elevLabelStr
            )) return false;

            // CABLES STRING - INVERSOR
            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrPolyN1Cable, solarSet.CableN1Tag, out elevPolyN1Cable
            )) return false;

            // return
            return true;
        }
    }
}
