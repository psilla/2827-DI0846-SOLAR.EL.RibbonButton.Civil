using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.SelectEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetEntMeasureCablesN2
    {
        public static bool GetEntMeasureCablesN2(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out SelectionSet analyzePoly,
            out PromptSelectionResult psrPolyInv,
            out PromptSelectionResult psrLabelInv,
            out PromptSelectionResult psrBlockRefInv,
            out PromptSelectionResult psrBlockRefCt,
            out PromptSelectionResult psrPolyN2Cable,
            out List<string> psrPolyInvLayers,
            out List<string> psrLabelInvLayers,
            out List<string> psrBlockRefInvLayers,
            out List<string> psrBlockRefCtLayers,
            out List<string> psrPolyN2CableLayers
        )
        {
            // Valores por defecto
            analyzePoly = null;
            psrPolyInv = null;
            psrLabelInv = null;
            psrBlockRefInv = null;
            psrBlockRefCt = null;
            psrPolyN2Cable = null;

            psrPolyInvLayers = null;
            psrLabelInvLayers = null;
            psrBlockRefInvLayers = null;
            psrBlockRefCtLayers = null;
            psrPolyN2CableLayers = null;

            List<string> defaultLayersPolyInv = new List<string> {solarSet.PolyInvLayer};
            List<string> defaultLayersLabelInv = new List<string> { solarSet.LabelInvLayer };
            List<string> defaultLayersBlockRefInv = docLayers.Where(l => l.IndexOf(
                solarSet.BlockRefInvLayer,
                System.StringComparison.OrdinalIgnoreCase
            ) >= 0).ToList();
            List<string> defaultLayersBlockRefCt = new List<string> { solarSet.BlockRefCtLayer };
            List<string> defaultLayersPolyN2Cable = new List<string> { solarSet.CableInvToCtLayer };

            // POLY INV
            psrPolyInv = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.PolyInvTag, "LWPOLYLINE", out psrPolyInvLayers, defaultLayersPolyInv
            );
            // Validamos
            if (psrPolyInv == null) return false;
            // Seleccionamos en funcion del bool (para este caso definimos TRUE)
            analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUser(
                ed, true, psrPolyInv, solarSet.PolyInvTag
            );
            // Validamos
            if (analyzePoly == null) return false;

            // LABEL INV
            psrLabelInv = cls_00_GetEntityByLayer.GetTextAndMTextByLayers(
                docLayers, ed, solarSet.LabelCtTag, out psrLabelInvLayers, defaultLayersLabelInv
            );
            // Validamos
            if (psrLabelInv == null) return false;

            // BLOCKREF INV
            psrBlockRefInv = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefInvTag, "INSERT", out psrBlockRefInvLayers, defaultLayersBlockRefInv
            );
            // Validamos
            if (psrBlockRefInv == null) return false;

            
            // BLOCKREF CT
            psrBlockRefCt = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefCtTag, "INSERT", out psrBlockRefCtLayers, defaultLayersBlockRefCt
            );
            // Validamos
            if (psrBlockRefCt == null) return false;

            // CABLES N2
            psrPolyN2Cable = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.CableN2Tag, "LWPOLYLINE", out psrPolyN2CableLayers, defaultLayersPolyN2Cable
            );
            // Validamos
            if (psrPolyN2Cable == null) return false;

            // return
            return true;
        }
    }
}
