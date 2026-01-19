using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.GetEntities;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_GetRequiredEntities
    {
        public static bool GetRequiredEntities(
            Editor ed,
            SolarSettings solarSet,
            List<string> docLayers,
            bool analyzeAllDoc,
            out SelectionSet analyzePoly,
            out PromptSelectionResult psrContGen,
            out PromptSelectionResult psrContInv,
            out PromptSelectionResult psrTrackers,
            out PromptSelectionResult psrInvLabels
        )
        {
            // Valores por defecto
            analyzePoly = null;
            psrContGen = null;
            psrContInv = null;
            psrTrackers = null;
            psrInvLabels = null;

            // Seleccionamos Entidades
            // CONTORNOS CT
            psrContGen = cls_00_GetEntityByLayer.GetEntityByLayer(
                docLayers, ed, solarSet.PolyCtTag, "LWPOLYLINE", solarSet.PolyCtLayer
            );
            // Validamos
            if (psrContGen == null) return false;
            // Seleccionamos en funcion del bool
            analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUser(
                ed, analyzeAllDoc, psrContGen, solarSet.PolyCtTag
            );
            // Validamos
            if (analyzePoly == null) return false;

            // CONTORNOS INVERSORES
            psrContInv = cls_00_GetEntityByLayer.GetEntityByLayer(
                docLayers, ed, solarSet.PolyInvTag, "LWPOLYLINE", solarSet.PolyInvLayer
            );
            // Validamos
            if (psrContInv == null) return false;

            // BLOCKREF TRACKERS
            psrTrackers = cls_00_GetEntityByLayer.GetEntityByLayer(
                docLayers, ed, solarSet.BlockRefTrackTag, "INSERT", solarSet.BlockRefTrackLayer
            );
            // Validamos
            if (psrTrackers == null) return false;

            // LABELS INVERSORES
            psrInvLabels = cls_00_GetEntityByLayer.GetEntityByLayer(
                docLayers, ed, solarSet.LabelInvTag, "MTEXT", solarSet.LabelInvLayer
            );
            // Validamos
            if (psrInvLabels == null) return false;

            // return
            return true; 
        }

    }
}
