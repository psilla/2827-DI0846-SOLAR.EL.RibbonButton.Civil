using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetEntMeasureCablesN2
    {
        public static bool GetEntMeasureCablesN2(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out SelectionSet analyzePoly,
            out PromptSelectionResult psrContInv,
            out PromptSelectionResult psrInvLab,
            out PromptSelectionResult psrInvBlock,
            out PromptSelectionResult psrCtBlock,
            out PromptSelectionResult psrInvCab
        )
        {
            // Valores por defecto
            analyzePoly = null;
            psrContInv = null;
            psrInvLab = null;
            psrInvBlock = null;
            psrCtBlock = null;
            psrInvCab = null;

            // Seleccionamos Entidades
            List<string> defaultLayersInvCont =
                new List<string> {solarSet.PolyInvLayer};
            // CONTORNOS INVERSORES
            psrContInv = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.PolyInvTag, "LWPOLYLINE", defaultLayersInvCont
            );
            // Validamos
            if (psrContInv == null) return false;
            // Seleccionamos en funcion del bool (para este caso definimos TRUE)
            analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUser(
                ed, true, psrContInv, solarSet.PolyInvTag
            );
            // Validamos
            if (analyzePoly == null) return false;

            List<string> defaultLayersInvLab =
                new List<string> { solarSet.LabelInvLayer };
            // LABELS INVERSORES
            psrInvLab = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.LabelInvTag, "MTEXT", defaultLayersInvLab
            );
            // Validamos
            if (psrInvLab == null) return false;

            List<string> defaultLayersInvBlock = docLayers
                .Where(l => l.IndexOf(
                    solarSet.BlockRefInvLayer,
                    System.StringComparison.OrdinalIgnoreCase
                ) >= 0).ToList();
            // BLOCKREF INVERSORES
            psrInvBlock = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefInvTag, "INSERT", defaultLayersInvBlock
            );
            // Validamos
            if (psrInvBlock == null) return false;

            List<string> defaultLayersCtBlock =
                new List<string> { solarSet.BlockRefCtLayer };
            // BLOCKREF CT
            psrCtBlock = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefCtTag, "INSERT", defaultLayersCtBlock
            );
            // Validamos
            if (psrCtBlock == null) return false;

            List<string> defaultLayersInvCab =
                new List<string> {solarSet.CableInvToCtLayer};
            // CABLES INVERSOR - CT
            psrInvCab = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.CableN2Tag, "LWPOLYLINE", defaultLayersInvCab
            );
            // Validamos
            if (psrInvCab == null) return false;

            // return
            return true;
        }
    }
}
