using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetEntMeasureCablesN2
    {
        private static class EntityLayerKeysN2
        {
            public static string PolyInvLayer(SolarSettings s) => $"Select layer for {s.PolyInvTag}:";
            public static string LabelInvLayer(SolarSettings s) => $"Select layer for {s.LabelInvTag}:";
            public static string BlockRefCtLayer(SolarSettings s) => $"Select layer for {s.BlockRefCtTag}:";
            public static string CableN2Layer(SolarSettings s) => $"Select layer for {s.CableN2Tag}:";
        }

        private static void ShowEntitiesByDocumentSummary(
            PromptSelectionResult psrPolyInv,
            PromptSelectionResult psrLabelInv,
            PromptSelectionResult psrBlockRefInv,
            PromptSelectionResult psrBlockRefCt,
            PromptSelectionResult psrPolyN2Cable,
            string psrPolyInvLayer,
            string psrLabelInvLayer,
            string psrBlockRefCtLayer,
            string psrPolyN2CableLayer,
            List<string> psrBlockRefInvLayers
        )
        {
            // -----------------------------
            // Info
            // -----------------------------

            StringBuilder infoRegion = new StringBuilder();

            infoRegion.AppendLine($"📌 Poly Inv Layer: {psrPolyInvLayer}");
            infoRegion.AppendLine($"📌 Label Inv Layer: {psrLabelInvLayer}");
            infoRegion.AppendLine($"📌 BlockRef CT Layer: {psrBlockRefCtLayer}");
            infoRegion.AppendLine($"📌 Poly N2 Cable Layer: {psrPolyN2CableLayer}");

            infoRegion.AppendLine();

            infoRegion.AppendLine($"📌 BlockRef INV Layers:");

            if (psrBlockRefInvLayers != null && psrBlockRefInvLayers.Count > 0)
            {
                foreach (string layer in psrBlockRefInvLayers)
                {
                    infoRegion.AppendLine($"   - {layer}");
                }
            }
            else
            {
                infoRegion.AppendLine("   - NONE");
            }

            infoRegion.AppendLine();

            infoRegion.AppendLine($"📌 Selection Results:");
            infoRegion.AppendLine($"   - Poly Inv: {(psrPolyInv != null ? psrPolyInv.Value.Count.ToString() : "NULL")}");
            infoRegion.AppendLine($"   - Label Inv: {(psrLabelInv != null ? psrLabelInv.Value.Count.ToString() : "NULL")}");
            infoRegion.AppendLine($"   - BlockRef CT: {(psrBlockRefCt != null ? psrBlockRefCt.Value.Count.ToString() : "NULL")}");
            infoRegion.AppendLine($"   - Poly N2 Cable: {(psrPolyN2Cable != null ? psrPolyN2Cable.Value.Count.ToString() : "NULL")}");
            infoRegion.AppendLine($"   - BlockRef INV: {(psrBlockRefInv != null ? psrBlockRefInv.Value.Count.ToString() : "NULL")}");

            // -----------------------------
            // Mostrar
            // -----------------------------

            ShowStringBuilder.ShowInfo(
                $"📌 Entities by Document Summary:",
                infoRegion.ToString()
            );
        }

        private static bool GetEntityLayersFromUserN2(
            List<string> docLayers,
            SolarSettings solarSet,
            out string polyInvLayer,
            out string labelInvLayer,
            out string blockRefCtLayer,
            out string cableN2Layer
        )
        {
            polyInvLayer = null;
            labelInvLayer = null;
            blockRefCtLayer = null;
            cableN2Layer = null;

            var comboFields = new List<(string, List<string>, string)>
            {
                (EntityLayerKeysN2.PolyInvLayer(solarSet), docLayers, solarSet.PolyInvLayer),
                (EntityLayerKeysN2.LabelInvLayer(solarSet), docLayers, solarSet.LabelInvLayer),
                (EntityLayerKeysN2.BlockRefCtLayer(solarSet), docLayers, solarSet.BlockRefCtLayer),
                (EntityLayerKeysN2.CableN2Layer(solarSet), docLayers, solarSet.CableN2Layer)
            };
            // Form
            Dictionary<string, string> result = cls_00_InstaForm_ComboBox.ComboBoxFormOut_NextToLabel(
                "Select layers for N2 entities:", comboFields, formTitle: "N2 Layer Selection"
            );
            // Validamos
            if (result == null) return false;

            // Asignamos
            polyInvLayer = result[EntityLayerKeysN2.PolyInvLayer(solarSet)];
            labelInvLayer = result[EntityLayerKeysN2.LabelInvLayer(solarSet)];
            blockRefCtLayer = result[EntityLayerKeysN2.BlockRefCtLayer(solarSet)];
            cableN2Layer = result[EntityLayerKeysN2.CableN2Layer(solarSet)];

            // return
            return true;
        }

        public static bool GetEntMeasureCablesN2(
            Editor ed,
            List<string> docLayers,
            SolarSettings solarSet,
            out PromptSelectionResult psrPolyInv,
            out PromptSelectionResult psrLabelInv,
            out PromptSelectionResult psrBlockRefInv,
            out PromptSelectionResult psrBlockRefCt,
            out PromptSelectionResult psrPolyN2Cable,
            out string psrPolyInvLayer,
            out string psrLabelInvLayer,
            out string psrBlockRefCtLayer,
            out string psrPolyN2CableLayer,
            out List<string> psrBlockRefInvLayers
        )
        {
            EntityTypes entityTypes = EntityTypes.GetDefaultEntityTypes();

            // Por defecto
            psrPolyInv = null;
            psrLabelInv = null;
            psrBlockRefInv = null;
            psrBlockRefCt = null;
            psrPolyN2Cable = null;

            psrBlockRefInvLayers = null;
            List<string> defaultLayersBlockRefInv = docLayers.Where(l => l.IndexOf(
                solarSet.BlockRefInvLayer, System.StringComparison.OrdinalIgnoreCase
            ) >= 0).ToList();

            // -----------------------------
            // Obtener Capas 
            // -----------------------------

            if (!GetEntityLayersFromUserN2(
                docLayers, solarSet, out psrPolyInvLayer, out psrLabelInvLayer, 
                out psrBlockRefCtLayer, out psrPolyN2CableLayer
            )) return false;

            // -----------------------------
            // Seleccionar Polys Inversores 
            // -----------------------------

            psrPolyInv = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrPolyInvLayer, solarSet.PolyInvTag, entityTypes.Polyline
            );
            // Validamos
            if (psrPolyInv == null) return false;

            // -----------------------------
            // Seleccionar Labels Inversores 
            // -----------------------------

            psrLabelInv = cls_00_GetEntityByLayer.GetTextAndMTextByLayer(
                ed, solarSet.LabelInvTag, psrLabelInvLayer
            );
            // Validamos
            if (psrLabelInv == null) return false;

            // -----------------------------
            // Seleccionar BlockRef CT 
            // -----------------------------

            psrBlockRefCt = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrBlockRefCtLayer, solarSet.BlockRefCtTag, entityTypes.BlockReference
            );
            // Validamos
            if (psrBlockRefCt == null) return false;

            // -----------------------------
            // Seleccionar Cables N2
            // -----------------------------

            psrPolyN2Cable = cls_00_GetEntityByLayer.GetEntityByLayer(
                ed, psrPolyN2CableLayer, solarSet.CableN2Tag, entityTypes.Polyline
            );
            // Validamos
            if (psrPolyN2Cable == null) return false;

            // -----------------------------
            // Seleccionar BlockRef Inversores
            // -----------------------------

            psrBlockRefInv = cls_00_GetEntityByLayer.GetEntityByLayers(
                docLayers, ed, solarSet.BlockRefInvTag, entityTypes.BlockReference, out psrBlockRefInvLayers, defaultLayersBlockRefInv
            );
            // Validamos
            if (psrBlockRefInv == null) return false;

            // -----------------------------
            // Debug
            // -----------------------------

            bool showInfo = false;
            // Debug
            if (showInfo)
                ShowEntitiesByDocumentSummary(
                    psrPolyInv, psrLabelInv, psrBlockRefInv, psrBlockRefCt,
                    psrPolyN2Cable, psrPolyInvLayer, psrLabelInvLayer, psrBlockRefCtLayer,
                    psrPolyN2CableLayer, psrBlockRefInvLayers
                );

            // return
            return true;
        }

        


    }
}
