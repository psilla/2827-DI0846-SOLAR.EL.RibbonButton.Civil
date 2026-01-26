using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetEntMeasureCablesMV
    {
        public static bool GetEntMeasureCablesMV(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out PromptSelectionResult psrCtLab,
            out PromptSelectionResult psrCtBlock,
            out PromptSelectionResult psrEstBlock,
            out PromptSelectionResult psrCtCab
        )
        {
            // Valores por defecto
            psrCtLab = null;
            psrCtBlock = null;
            psrEstBlock = null;
            psrCtCab = null;

            // Seleccionamos Entidades
            List<string> defaultLayersCtLab =
                new List<string> { solarSet.LabelCtLayer};
            // LABELS CT
            psrCtLab = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.LabelCtTag, "MTEXT", defaultLayersCtLab
            );
            // Validamos
            if (psrCtLab == null) return false;

            List<string> defaultLayersCtBlock =
                new List<string> {solarSet.BlockRefCtLayer};
            // BLOCKREF CT
            psrCtBlock = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefCtTag, "INSERT", defaultLayersCtBlock
            );
            // Validamos
            if (psrCtBlock == null) return false;

            List<string> defaultLayersEstBlock =
                new List<string> { solarSet.BlockRefEstLayer };
            // BLOCKREF ESTACION
            psrEstBlock = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefEstTag, "INSERT", defaultLayersEstBlock
            );
            // Validamos
            if (psrEstBlock == null) return false;

            List<string> defaultLayersCtCab = docLayers
                .Where(l => l.IndexOf(
                    solarSet.CableCtToEstLayer,
                    System.StringComparison.OrdinalIgnoreCase
                ) >= 0).ToList();
            // CABLES CT - ESTACION
            psrCtCab = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.CableCtToEstTag, "LWPOLYLINE", defaultLayersCtCab
            );
            // Validamos
            if (psrCtCab == null) return false;

            // return
            return true;
        }
    }
}
