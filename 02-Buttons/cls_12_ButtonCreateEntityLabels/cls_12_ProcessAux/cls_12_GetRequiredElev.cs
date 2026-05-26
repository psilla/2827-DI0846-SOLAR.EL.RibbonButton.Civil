using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetEntityElevation;
using TYPSA.SharedLib.Autocad.Main;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_GetRequiredElev
    {
        public static bool GetRequiredElevationsByInv(
            Transaction tr,
            SolarSettings solarSet,
            PromptSelectionResult psrCtPoly,
            PromptSelectionResult psrInvPoly,
            PromptSelectionResult psrTrackBlock,
            PromptSelectionResult psrInvLabel,
            out double elevCtPoly,
            out double elevInvPoly,
            out double elevTrackBlock,
            out double elevInvLabel
        )
        {
            // Valores por defecto
            elevCtPoly = 0.0;
            elevInvPoly = 0.0;
            elevTrackBlock = 0.0;
            elevInvLabel = 0.0;

            // -----------------------------
            // Seleccionar Polys CT 
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrCtPoly, solarSet.PolyCtTag, out elevCtPoly
            )) return false;

            // -----------------------------
            // Seleccionar Polys Inversores 
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrInvPoly, solarSet.PolyInvTag, out elevInvPoly
            )) return false;

            // -----------------------------
            // Seleccionar BlockRef Trackers
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrTrackBlock, solarSet.BlockRefTrackTag, out elevTrackBlock
            )) return false;

            // -----------------------------
            // Seleccionar Labels Inversores 
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrInvLabel, solarSet.LabelInvTag, out elevInvLabel
            )) return false;

            // return
            return true;
        }

        public static bool GetRequiredElevationsByComBox(
            Transaction tr,
            SolarSettings solarSet,
            PromptSelectionResult psrCtPoly,
            PromptSelectionResult psrInvPoly,
            PromptSelectionResult psrTrackBlock,
            PromptSelectionResult psrInvLabel,
            PromptSelectionResult psrN2Cable,
            PromptSelectionResult psrCtBlock,
            PromptSelectionResult psrComBoxBlock,
            out double elevCtPoly,
            out double elevInvPoly,
            out double elevTrackBlock,
            out double elevInvLabel,
            out double elevN2Cable,
            out double elevCtBlock,
            out double elevComBoxBlock
        )
        {
            // Valores por defecto
            elevCtPoly = 0.0;
            elevInvPoly = 0.0;
            elevTrackBlock = 0.0;
            elevInvLabel = 0.0;
            elevN2Cable = 0.0;
            elevCtBlock = 0.0;
            elevComBoxBlock = 0.0;

            // -----------------------------
            // Seleccionar Polys CT 
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrCtPoly, solarSet.PolyCtTag, out elevCtPoly
            )) return false;

            // -----------------------------
            // Seleccionar Polys Inversores 
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrInvPoly, solarSet.PolyInvTag, out elevInvPoly
            )) return false;

            // -----------------------------
            // Seleccionar BlockRef Trackers
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrTrackBlock, solarSet.BlockRefTrackTag, out elevTrackBlock
            )) return false;

            // -----------------------------
            // Seleccionar Labels Inversores/Combiner
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrInvLabel, solarSet.LabelInvTag, out elevInvLabel
            )) return false;

            // -----------------------------
            // Seleccionar Cables N2
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrN2Cable, solarSet.CableN2Tag, out elevN2Cable
            )) return false;

            // -----------------------------
            // Seleccionar BlockRef CT 
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrCtBlock, solarSet.BlockRefCtTag, out elevCtBlock
            )) return false;

            // -----------------------------
            // Seleccionar BlockRef Combiner
            // -----------------------------

            if (!cls_00_GetEntityElev.AllEntHaveSameElev(
                tr, psrComBoxBlock, solarSet.BlockRefComBoxTag, out elevComBoxBlock
            )) return false;

            // return
            return true;
        }

    }
}
