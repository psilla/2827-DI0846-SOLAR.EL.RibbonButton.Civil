using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.SelectEntities;

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
            out PromptSelectionResult psrPolyCt,
            out PromptSelectionResult psrPolyInv,
            out PromptSelectionResult psrBlockRefTrack,
            out PromptSelectionResult psrLabelInv,
            out string psrPolyCtLayer,
            out string psrPolyInvLayer,
            out string psrBlockRefTrackLayer,
            out string psrLabelInvLayer
        )
        {
            // Valores por defecto
            analyzePoly = null;
            psrPolyCt = null;
            psrPolyInv = null;
            psrBlockRefTrack = null;
            psrLabelInv = null;

            psrPolyCtLayer = null;
            psrPolyInvLayer = null;
            psrBlockRefTrackLayer = null;
            psrLabelInvLayer = null;

            // POLYS CT
            psrPolyCt = cls_00_GetEntityByLayer.GetEntityByLayer(
                docLayers, ed, solarSet.PolyCtTag, "LWPOLYLINE", out psrPolyCtLayer, solarSet.PolyCtLayer
            );
            // Validamos
            if (psrPolyCt == null) return false;
            // Seleccionamos en funcion del bool
            analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUser(
                ed, analyzeAllDoc, psrPolyCt, solarSet.PolyCtTag
            );
            // Validamos
            if (analyzePoly == null) return false;

            // POLYS INV
            psrPolyInv = cls_00_GetEntityByLayer.GetEntityByLayer(
                docLayers, ed, solarSet.PolyInvTag, "LWPOLYLINE", out psrPolyInvLayer, solarSet.PolyInvLayer
            );
            // Validamos
            if (psrPolyInv == null) return false;

            // BLOCKREF TRACK
            psrBlockRefTrack = cls_00_GetEntityByLayer.GetEntityByLayer(
                docLayers, ed, solarSet.BlockRefTrackTag, "INSERT", out psrBlockRefTrackLayer, solarSet.BlockRefTrackLayer
            );
            // Validamos
            if (psrBlockRefTrack == null) return false;

            // LABELS INV
            psrLabelInv = cls_00_GetEntityByLayer.GetTextAndMTextByLayer(
                docLayers, ed, solarSet.LabelInvTag, out psrLabelInvLayer, solarSet.LabelInvLayer
            );
            // Validamos
            if (psrLabelInv == null) return false;

            // return
            return true; 
        }

    }
}
