using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetEntMeasureCablesMV
    {
        private static class EntityLayerKeysMV
        {
            public static string LabelCtLayer(SolarSettings s) => $"Select layer for {s.LabelCtTag}:";
            public static string BlockRefCtLayer(SolarSettings s) => $"Select layer for {s.BlockRefCtTag}:";
            public static string BlockRefEstLayer(SolarSettings s) => $"Select layer for {s.BlockRefEstTag}:";
        }

        private static bool GetEntityLayersFromUserMV(
            List<string> docLayers,
            SolarSettings solarSet,
            out string labelCtLayer,
            out string blockRefCtLayer,
            out string blockRefEstLayer
        )
        {
            // Por defecto
            labelCtLayer = null;
            blockRefCtLayer = null;
            blockRefEstLayer = null;

            var comboFields = new List<(string, List<string>, string)>
            {
                (EntityLayerKeysMV.LabelCtLayer(solarSet), docLayers, solarSet.LabelCtLayer),
                (EntityLayerKeysMV.BlockRefCtLayer(solarSet), docLayers, solarSet.BlockRefCtLayer),
                (EntityLayerKeysMV.BlockRefEstLayer(solarSet), docLayers, solarSet.BlockRefEstLayer)
            };
            // Form
            Dictionary<string, string> result = cls_00_InstaForm_ComboBox.ComboBoxFormOut_NextToLabel(
                "Select layers for MV entities:", comboFields, formTitle: "MV Layer Selection"
            );
            // Validamos
            if (result == null) return false;

            // Asignamos
            labelCtLayer = result[EntityLayerKeysMV.LabelCtLayer(solarSet)];
            blockRefCtLayer = result[EntityLayerKeysMV.BlockRefCtLayer(solarSet)];
            blockRefEstLayer = result[EntityLayerKeysMV.BlockRefEstLayer(solarSet)];

            // return
            return true;
        }

        public static bool GetEntMeasureCablesMV(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out PromptSelectionResult psrLabelCt,
            out PromptSelectionResult psrBlockRefCt,
            out PromptSelectionResult psrBlockRefEst,
            out PromptSelectionResult psrPolyMvCable,
            out string psrLabelCtLayer,
            out string psrBlockRefCtLayer,
            out string psrBlockRefEstLayer,
            out List<string> psrPolyMvCableLayers
        )
        {
            EntityTypes entityTypes = EntityTypes.GetDefaultEntityTypes();

            // Por defecto
            psrLabelCt = null;
            psrBlockRefCt = null;
            psrBlockRefEst = null;
            psrPolyMvCable = null;

            psrPolyMvCableLayers = null;
            List<string> defaultLayersPolyMvCable = docLayers.Where(l => l.IndexOf(
                solarSet.CableMVLayer, System.StringComparison.OrdinalIgnoreCase
            ) >= 0).ToList();

            // Obtenemos las capas
            if (!GetEntityLayersFromUserMV(
                docLayers, solarSet, out psrLabelCtLayer, out psrBlockRefCtLayer, out psrBlockRefEstLayer
            )) return false;

            // LABEL CT
            psrLabelCt = cls_00_GetEntityByLayer.GetTextAndMTextByLayer(
                ed, solarSet.LabelCtTag, psrLabelCtLayer
            );
            // Validamos
            if (psrLabelCt == null) return false;

            // BLOCKREF CT
            psrBlockRefCt = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrBlockRefCtLayer, solarSet.BlockRefCtTag, entityTypes.BlockReference
            );
            // Validamos
            if (psrBlockRefCt == null) return false;

            // BLOCKREF EST
            psrBlockRefEst = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrBlockRefEstLayer, solarSet.BlockRefEstTag, entityTypes.BlockReference
            );
            // Validamos
            if (psrBlockRefEst == null) return false;

            // CABLE MV
            psrPolyMvCable = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.CableMVTag, entityTypes.Polyline, out psrPolyMvCableLayers, defaultLayersPolyMvCable
            );
            // Validamos
            if (psrPolyMvCable == null) return false;

            // return
            return true;
        }

    }
}
