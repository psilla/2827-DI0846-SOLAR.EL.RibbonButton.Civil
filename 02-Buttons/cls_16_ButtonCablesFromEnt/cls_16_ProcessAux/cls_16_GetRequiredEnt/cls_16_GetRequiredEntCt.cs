using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using TYPSA.SharedLib.Autocad.GetEntities;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetRequiredEntCt
    {
        public static bool GetRequiredEntCt(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out PromptSelectionResult psrCtLab,
            out PromptSelectionResult psrCtBlock,
            out PromptSelectionResult psrCtCab
        )
        {
            // Valores por defecto
            psrCtLab = null;
            psrCtBlock = null;
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

            List<string> defaultLayersCtCab =
                new List<string> {solarSet.CableCtToEstLayer};
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
