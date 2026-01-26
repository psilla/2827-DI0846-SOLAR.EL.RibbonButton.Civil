using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using TYPSA.SharedLib.Autocad.GetEntities;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetEntMeasureCablesN1
    {
        public static bool GetEntMeasureCablesN1(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out SelectionSet analyzePoly,
            out PromptSelectionResult psrString,
            out PromptSelectionResult psrStringLab,
            out PromptSelectionResult psrStringCab
        )
        {
            // Valores por defecto
            analyzePoly = null;
            psrString = null;
            psrStringLab = null;
            psrStringCab = null;

            // Seleccionamos Entidades
            List<string> defaultLayersString =
                new List<string> {solarSet.PolyStringLayer };
            // CONTORNOS STRINGS
            psrString = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.PolyStringTag, "LWPOLYLINE", defaultLayersString
            );
            // Validamos
            if (psrString == null) return false;
            // Seleccionamos en funcion del bool (para este caso definimos TRUE)
            analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUser(
                ed, true, psrString, solarSet.PolyStringTag
            );
            // Validamos
            if (analyzePoly == null) return false;

            List<string> defaultLayersStringLab =
                new List<string> { solarSet.LabelStringLayer };
            // LABELS STRINGS
            psrStringLab = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.LabelStringTag, "MTEXT", defaultLayersStringLab
            );
            // Validamos
            if (psrStringLab == null) return false;

            List<string> defaultLayersInvCab =
                new List<string> {solarSet.CableStringToInvLayer};
            // CABLES STRING - INVERSOR
            psrStringCab = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.CableStringToInvTag, "LWPOLYLINE", defaultLayersInvCab
            );
            // Validamos
            if (psrStringCab == null) return false;

            // return
            return true;
        }
    }
}
