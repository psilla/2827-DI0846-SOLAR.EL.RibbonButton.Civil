using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_GetRequiredEntities
    {
        private static class EntityLayerKeys
        {
            // Por Inversor
            public static string PolyCtLayer(SolarSettings s) => $"Select layer for {s.PolyCtTag}:";
            public static string PolyInvLayer(SolarSettings s) => $"Select layer for {s.PolyInvTag}:";
            public static string TrackBlockLayer(SolarSettings s) => $"Select layer for {s.BlockRefTrackTag}:";
            public static string InverterLabelLayer(SolarSettings s) => $"Select layer for {s.LabelInvTag}:";
            // Por Combiner 
            public static string CableN2Layer(SolarSettings s) => $"Select layer for {s.CableN2Tag}:";
            public static string BlockRefCtLayer(SolarSettings s) => $"Select layer for {s.BlockRefCtTag}:";
        }

        private static bool GetEntityLayersFromUserByInv(
            List<string> docLayers,
            SolarSettings solarSet,
            out string polyCtLayer,
            out string polyInvLayer,
            out string trackBlockLayer,
            out string labelInvLayer
        )
        {
            polyCtLayer = null;
            polyInvLayer = null;
            trackBlockLayer = null;
            labelInvLayer = null;

            var comboFields = new List<(string, List<string>, string)>
            {
                (EntityLayerKeys.PolyCtLayer(solarSet), docLayers, solarSet.PolyCtLayer),
                (EntityLayerKeys.PolyInvLayer(solarSet), docLayers, solarSet.PolyInvLayer),
                (EntityLayerKeys.TrackBlockLayer(solarSet), docLayers, solarSet.BlockRefTrackLayer),
                (EntityLayerKeys.InverterLabelLayer(solarSet), docLayers, solarSet.LabelInvLayer)
            };
            // Form
            Dictionary<string, string> result = cls_00_InstaForm_ComboBox.ComboBoxFormOut_NextToLabel(
                "Select layers for required entities:", comboFields, formTitle: "Entity Layer Selection"
            );
            // Validamos
            if (result == null) return false;

            // Asignamos
            polyCtLayer = result[EntityLayerKeys.PolyCtLayer(solarSet)];
            polyInvLayer = result[EntityLayerKeys.PolyInvLayer(solarSet)];
            trackBlockLayer = result[EntityLayerKeys.TrackBlockLayer(solarSet)];
            labelInvLayer = result[EntityLayerKeys.InverterLabelLayer(solarSet)];

            // return
            return true;
        }

        private static bool GetEntityLayersFromUserByComBox(
            List<string> docLayers,
            SolarSettings solarSet,
            out string polyCtLayer,
            out string polyInvLayer,
            out string trackBlockLayer,
            out string labelInvLayer,
            out string cableN2Layer,
            out string blockRefCtLayer
        )
        {
            polyCtLayer = null;
            polyInvLayer = null;
            trackBlockLayer = null;
            labelInvLayer = null;
            cableN2Layer = null;
            blockRefCtLayer = null;

            var comboFields = new List<(string, List<string>, string)>
            {
                (EntityLayerKeys.PolyCtLayer(solarSet), docLayers, solarSet.PolyCtLayer),
                (EntityLayerKeys.PolyInvLayer(solarSet), docLayers, solarSet.PolyInvLayer),
                (EntityLayerKeys.TrackBlockLayer(solarSet), docLayers, solarSet.BlockRefTrackLayer),
                (EntityLayerKeys.InverterLabelLayer(solarSet), docLayers, solarSet.LabelInvLayer),
                (EntityLayerKeys.CableN2Layer(solarSet), docLayers, solarSet.CableN2Layer),
                (EntityLayerKeys.BlockRefCtLayer(solarSet), docLayers, solarSet.BlockRefCtLayer)
            };
            // Form
            Dictionary<string, string> result = cls_00_InstaForm_ComboBox.ComboBoxFormOut_NextToLabel(
                "Select layers for required entities:", comboFields, formTitle: "Entity Layer Selection"
            );
            // Validamos
            if (result == null) return false;

            // Asignamos
            polyCtLayer = result[EntityLayerKeys.PolyCtLayer(solarSet)];
            polyInvLayer = result[EntityLayerKeys.PolyInvLayer(solarSet)];
            trackBlockLayer = result[EntityLayerKeys.TrackBlockLayer(solarSet)];
            labelInvLayer = result[EntityLayerKeys.InverterLabelLayer(solarSet)];
            cableN2Layer = result[EntityLayerKeys.CableN2Layer(solarSet)];
            blockRefCtLayer = result[EntityLayerKeys.BlockRefCtLayer(solarSet)];

            // return
            return true;
        }

        public static bool GetRequiredEntitiesByInv(
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
            EntityTypes entityTypes = EntityTypes.GetDefaultEntityTypes();

            // Por defecto
            analyzePoly = null;
            psrPolyCt = null;
            psrPolyInv = null;
            psrBlockRefTrack = null;
            psrLabelInv = null;

            // -----------------------------
            // Obtener Capas 
            // -----------------------------

            if (!GetEntityLayersFromUserByInv(
                docLayers, solarSet, out psrPolyCtLayer, out psrPolyInvLayer, 
                out psrBlockRefTrackLayer, out psrLabelInvLayer
            )) return false;

            // -----------------------------
            // Seleccionar Polys CT 
            // -----------------------------
          
            psrPolyCt = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrPolyCtLayer, solarSet.PolyCtTag, entityTypes.Polyline
            );
            // Validamos
            if (psrPolyCt == null) return false;

            analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUserByLayer(
                ed, analyzeAllDoc, psrPolyCt, solarSet.PolyCtTag, psrPolyCtLayer
            );
            // Validamos
            if (analyzePoly == null) return false;

            // -----------------------------
            // Seleccionar Polys Inversores 
            // -----------------------------
        
            psrPolyInv = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrPolyInvLayer, solarSet.PolyInvTag, entityTypes.Polyline
            );
            // Validamos
            if (psrPolyInv == null) return false;

            // -----------------------------
            // Seleccionar BlockRef Trackers
            // -----------------------------
           
            psrBlockRefTrack = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrBlockRefTrackLayer, solarSet.BlockRefTrackTag, entityTypes.BlockReference
            );
            // Validamos
            if (psrBlockRefTrack == null) return false;

            // -----------------------------
            // Seleccionar Labels Inversores 
            // -----------------------------

            psrLabelInv = cls_00_GetEntityByLayer.GetTextAndMTextByLayer(
                ed, solarSet.LabelInvTag, psrLabelInvLayer
            );
            // Validamos
            if (psrLabelInv == null) return false;

            // return
            return true;
        }

        public static bool GetRequiredEntitiesByComBox(
            Editor ed,
            SolarSettings solarSet,
            List<string> docLayers,
            bool analyzeAllDoc,
            out SelectionSet analyzePoly,
            out PromptSelectionResult psrPolyCt,
            out PromptSelectionResult psrPolyInv,
            out PromptSelectionResult psrBlockRefTrack,
            out PromptSelectionResult psrLabelInv,
            out PromptSelectionResult psrPolyN2Cable,
            out PromptSelectionResult psrBlockRefCt,
            out PromptSelectionResult psrBlockRefComBox,
            out string psrPolyCtLayer,
            out string psrPolyInvLayer,
            out string psrBlockRefTrackLayer,
            out string psrLabelInvLayer,
            out string psrPolyN2CableLayer,
            out string psrBlockRefCtLayer,
            out List<string> psrBlockRefComBoxLayers
        )
        {
            EntityTypes entityTypes = EntityTypes.GetDefaultEntityTypes();

            // Por defecto
            analyzePoly = null;
            psrPolyCt = null;
            psrPolyInv = null;
            psrBlockRefTrack = null;
            psrLabelInv = null;
            psrPolyN2Cable = null;
            psrBlockRefCt = null;
            psrBlockRefComBox = null;
            psrBlockRefComBoxLayers = null;

            List<string> defaultLayersBlockRefInv = docLayers.Where(l => l.IndexOf(
                solarSet.BlockRefInvLayer, System.StringComparison.OrdinalIgnoreCase
            ) >= 0).ToList();

            // -----------------------------
            // Obtener Capas 
            // -----------------------------

            if (!GetEntityLayersFromUserByComBox(
                docLayers, solarSet, out psrPolyCtLayer, out psrPolyInvLayer,
                out psrBlockRefTrackLayer, out psrLabelInvLayer, out psrPolyN2CableLayer,
                out psrBlockRefCtLayer
            )) return false;

            // -----------------------------
            // Seleccionar Polys CT 
            // -----------------------------

            psrPolyCt = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrPolyCtLayer, solarSet.PolyCtTag, entityTypes.Polyline
            );
            // Validamos
            if (psrPolyCt == null) return false;

            analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUserByLayer(
                ed, analyzeAllDoc, psrPolyCt, solarSet.PolyCtTag, psrPolyCtLayer
            );
            // Validamos
            if (analyzePoly == null) return false;

            // -----------------------------
            // Seleccionar Polys Inversores 
            // -----------------------------

            psrPolyInv = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrPolyInvLayer, solarSet.PolyInvTag, entityTypes.Polyline
            );
            // Validamos
            if (psrPolyInv == null) return false;

            // -----------------------------
            // Seleccionar BlockRef Trackers
            // -----------------------------

            psrBlockRefTrack = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrBlockRefTrackLayer, solarSet.BlockRefTrackTag, entityTypes.BlockReference
            );
            // Validamos
            if (psrBlockRefTrack == null) return false;

            // -----------------------------
            // Seleccionar Labels Inversores 
            // -----------------------------

            psrLabelInv = cls_00_GetEntityByLayer.GetTextAndMTextByLayer(
                ed, solarSet.LabelInvTag, psrLabelInvLayer
            );
            // Validamos
            if (psrLabelInv == null) return false;

            // -----------------------------
            // Seleccionar Cables N2
            // -----------------------------

            psrPolyN2Cable = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrPolyN2CableLayer, solarSet.CableN2Tag, entityTypes.Polyline
            );
            // Validamos
            if (psrPolyN2Cable == null) return false;

            // -----------------------------
            // Seleccionar BlockRef CT 
            // -----------------------------

            psrBlockRefCt = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrBlockRefCtLayer, solarSet.BlockRefCtTag, entityTypes.BlockReference
            );
            // Validamos
            if (psrBlockRefCt == null) return false;

            // -----------------------------
            // Seleccionar BlockRef Combiner
            // -----------------------------

            psrBlockRefComBox = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefComBoxTag, entityTypes.BlockReference, 
                out psrBlockRefComBoxLayers, defaultLayersBlockRefInv
            );
            // Validamos
            if (psrBlockRefComBox == null) return false;

            // return
            return true;
        }


    }
}
