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
            out PromptSelectionResult psrPolyStr,
            out PromptSelectionResult psrLabelStr,
            out PromptSelectionResult psrPolyN1Cable,
            out List<string> psrPolyStrLayers,
            out List<string> psrLabelStrLayers,
            out List<string> psrPolyN1CableLayers
        )
        {
            // Valores por defecto
            analyzePoly = null;
            psrPolyStr = null;
            psrLabelStr = null;
            psrPolyN1Cable = null;

            psrPolyStrLayers = null;
            psrLabelStrLayers = null;
            psrPolyN1CableLayers = null;

            List<string> defaultLayersPolyStr = new List<string> {solarSet.PolyStringLayer };
            List<string> defaultLayersLabelStr = new List<string> { solarSet.LabelStringLayer };
            List<string> defaultLayersPolyN1Cable = new List<string> { solarSet.CableStringToInvLayer };

            // POLY STR
            psrPolyStr = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.PolyStringTag, "LWPOLYLINE", out psrPolyStrLayers, defaultLayersPolyStr
            );
            // Validamos
            if (psrPolyStr == null) return false;
            // Seleccionamos en funcion del bool (para este caso definimos TRUE)
            analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUser(
                ed, true, psrPolyStr, solarSet.PolyStringTag
            );
            // Validamos
            if (analyzePoly == null) return false;

            // LABEL STR
            psrLabelStr = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.LabelStringTag, "MTEXT", out psrLabelStrLayers, defaultLayersLabelStr
            );
            // Validamos
            if (psrLabelStr == null) return false;

            // CABLE N1
            psrPolyN1Cable = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.CableN1Tag, "LWPOLYLINE", out psrPolyN1CableLayers, defaultLayersPolyN1Cable
            );
            // Validamos
            if (psrPolyN1Cable == null) return false;

            // return
            return true;
        }
    }
}
