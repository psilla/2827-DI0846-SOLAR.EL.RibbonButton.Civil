using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.SelectEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetEntMeasureCablesMV
    {
        public static bool GetEntMeasureCablesMV(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out PromptSelectionResult psrLabelCt,
            out PromptSelectionResult psrBlockRefCt,
            out PromptSelectionResult psrBlockRefEst,
            out PromptSelectionResult psrPolyMvCable,
            out List<string> psrLabelCtLayers,
            out List<string> psrBlockRefCtLayers,
            out List<string> psrBlockRefEstLayers,
            out List<string> psrPolyMvCableLayers
        )
        {
            // Valores por defecto
            psrLabelCt = null;
            psrBlockRefCt = null;
            psrBlockRefEst = null;
            psrPolyMvCable = null;

            psrLabelCtLayers = null;
            psrBlockRefCtLayers = null;
            psrBlockRefEstLayers = null;
            psrPolyMvCableLayers = null;

            List<string> defaultLayersLabelCt = new List<string> { solarSet.LabelCtLayer};
            List<string> defaultLayersBlockRefCt = new List<string> { solarSet.BlockRefCtLayer };
            List<string> defaultLayersBlockRefEst = new List<string> { solarSet.BlockRefEstLayer };
            List<string> defaultLayersPolyMvCable = docLayers.Where(l => l.IndexOf(
                solarSet.CableCtToEstLayer,
                System.StringComparison.OrdinalIgnoreCase
            ) >= 0).ToList();

            // LABEL CT
            psrLabelCt = cls_00_GetEntityByLayer.GetTextAndMTextByLayers(
                docLayers, ed, solarSet.LabelCtTag, out psrLabelCtLayers, defaultLayersLabelCt
            );
            // Validamos
            if (psrLabelCt == null) return false;

            // BLOCKREF CT
            psrBlockRefCt = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefCtTag, "INSERT", out psrBlockRefCtLayers, defaultLayersBlockRefCt
            );
            // Validamos
            if (psrBlockRefCt == null) return false;

            // BLOCKREF EST
            psrBlockRefEst = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefEstTag, "INSERT", out psrBlockRefEstLayers, defaultLayersBlockRefEst
            );
            // Validamos
            if (psrBlockRefEst == null) return false;

            // CABLE MV
            psrPolyMvCable = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.CableMVTag, "LWPOLYLINE", out psrPolyMvCableLayers, defaultLayersPolyMvCable
            );
            // Validamos
            if (psrPolyMvCable == null) return false;

            // return
            return true;
        }
    }
}
