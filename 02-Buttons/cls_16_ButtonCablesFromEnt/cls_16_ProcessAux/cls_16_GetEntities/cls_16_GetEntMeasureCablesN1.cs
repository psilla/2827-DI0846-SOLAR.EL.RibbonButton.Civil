using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetEntMeasureCablesN1
    {
        private static class EntityLayerKeysN1
        {
            public static string PolyStringLayer(SolarSettings s) => $"Select layer for {s.PolyStringTag}:";
            public static string LabelStringLayer(SolarSettings s) => $"Select layer for {s.LabelStringTag}:";
            public static string CableN1Layer(SolarSettings s) => $"Select layer for {s.CableN1Tag}:";
        }

        private static bool GetEntityLayersFromUserN1(
            List<string> docLayers,
            SolarSettings solarSet,
            out string polyStrLayer,
            out string labelStrLayer,
            out string cableN1Layer
        )
        {
            // Por defecto
            polyStrLayer = null;
            labelStrLayer = null;
            cableN1Layer = null;

            var comboFields = new List<(string, List<string>, string)>
            {
                (EntityLayerKeysN1.PolyStringLayer(solarSet), docLayers, solarSet.PolyStringLayer),
                (EntityLayerKeysN1.LabelStringLayer(solarSet), docLayers, solarSet.LabelStringLayer),
                (EntityLayerKeysN1.CableN1Layer(solarSet), docLayers, solarSet.CableN1Layer)
            };
            // Form
            Dictionary<string, string> result = cls_00_InstaForm_ComboBox.ComboBoxFormOut_NextToLabel(
                "Select layers for N1 entities:", comboFields, formTitle: "N1 Layer Selection"
            );
            // Validamos
            if (result == null) return false;

            // Asignamos
            polyStrLayer = result[EntityLayerKeysN1.PolyStringLayer(solarSet)];
            labelStrLayer = result[EntityLayerKeysN1.LabelStringLayer(solarSet)];
            cableN1Layer = result[EntityLayerKeysN1.CableN1Layer(solarSet)];

            // return
            return true;
        }

        public static bool GetEntMeasureCablesN1(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out PromptSelectionResult psrPolyStr,
            out PromptSelectionResult psrLabelStr,
            out PromptSelectionResult psrPolyN1Cable,
            out string psrPolyStrLayer,
            out string psrLabelStrLayer,
            out string psrPolyN1CableLayer
        )
        {
            EntityTypes entityTypes = EntityTypes.GetDefaultEntityTypes();

            // Por defecto
            psrPolyStr = null;
            psrLabelStr = null;
            psrPolyN1Cable = null;

            // Obtenemos las capas
            if (!GetEntityLayersFromUserN1(
                docLayers, solarSet, out psrPolyStrLayer, out psrLabelStrLayer, out psrPolyN1CableLayer
            )) return false;

            // POLY STR
            psrPolyStr = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrPolyStrLayer, solarSet.PolyStringTag, entityTypes.Polyline
            );
            // Validamos
            if (psrPolyStr == null) return false;

            // LABEL STR
            psrLabelStr = cls_00_GetEntityByLayer.GetTextAndMTextByLayer(
                ed, solarSet.LabelStringTag, psrLabelStrLayer
            );
            // Validamos
            if (psrLabelStr == null) return false;

            // CABLE N1
            psrPolyN1Cable = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrPolyN1CableLayer, solarSet.CableN1Tag, entityTypes.Polyline
            );
            // Validamos
            if (psrPolyN1Cable == null) return false;

            // return
            return true;
        }

    }
}
