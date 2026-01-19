using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using TYPSA.SharedLib.Autocad.GetEntities;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetRequiredEntInv
    {
        public static bool GetRequiredEntInv(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out SelectionSet analyzePoly,
            out PromptSelectionResult psrContInv,
            out PromptSelectionResult psrInvLab,
            out PromptSelectionResult psrInvBlock,
            out PromptSelectionResult psrInvCab
        )
        {
            // Valores por defecto
            analyzePoly = null;
            psrContInv = null;
            psrInvLab = null;
            psrInvBlock = null;
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

            List<string> defaultLayersInvBlock =
                new List<string> {solarSet.BlockRefInvLayer};
            // BLOCKREF INVERSORES
            psrInvBlock = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefInvTag, "INSERT", defaultLayersInvBlock
            );
            // Validamos
            if (psrInvBlock == null) return false;

            List<string> defaultLayersInvCab =
                new List<string> {solarSet.CableInvToCtLayer};
            // CABLES INVERSOR - CT
            psrInvCab = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.CableInvToCtTag, "LWPOLYLINE", defaultLayersInvCab
            );
            // Validamos
            if (psrInvCab == null) return false;

            // return
            return true;
        }
    }
}
