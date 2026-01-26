using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.DeleteEntities;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.IsolateEntities;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;
using TYPSA.SharedLib.Autocad.ProcessPolyAndRegion;
using TYPSA.SharedLib.Excel;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_ProcessMeasureCablesN2
    {
        public static int? ProcessMeasureCablesN2(
            Editor ed, 
            Database db, 
            Transaction tr, 
            BlockTableRecord btr, 
            string excelPath,
            string projectUnits,
            double cableLengthCorrectionFactor,
            double cableLengthFixedAllowance,
            int cableNumberOfConductors
        )
        {
            // try
            try
            {
                // Obtenemos settings
                SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();
                AutocadSettings autoSettings = AutocadSettings.GetDefaultSettings();

                // Obtenemos el listado de capas del documento
                List<string> docLayers = 
                    cls_00_GetLayerNamesFromDoc.GetLayerNamesFromDoc(db);

                // Seleccionamos Entidades
                if (!cls_16_GetEntMeasureCablesN2.GetEntMeasureCablesN2(
                    ed, docLayers, solarSet,
                    out SelectionSet analyzePoly,
                    out PromptSelectionResult psrContInv, out PromptSelectionResult psrInvLab,
                    out PromptSelectionResult psrInvBlock, out PromptSelectionResult psrInvCab
                )) return null;

                // Validamos elevaciones
                if (!cls_16_GetElevMeasureCablesN2.GetElevMeasureCablesN2(
                    tr, solarSet, psrContInv, psrInvLab, psrInvBlock, psrInvCab,
                    out double elevInvCont, out double elevInvLabel, out double elevInvBlock, out double elevInvCab
                )) return null;

                // Validamos elevaciones entre Entidades
                if (Math.Abs(elevInvCont - elevInvLabel) > 1e-6 ||
                    Math.Abs(elevInvCont - elevInvBlock) > 1e-6 ||
                    Math.Abs(elevInvCont - elevInvCab) > 1e-6
                )
                {
                    // Mensaje
                    MessageBox.Show(
                        $"⚠ Elevations are inconsistent across entities.\n\n" +
                        $"{solarSet.PolyInvTag} Z: {elevInvCont:F3}\n" +
                        $"{solarSet.LabelInvTag} Z: {elevInvLabel:F3}\n" +
                        $"{solarSet.BlockRefInvTag} Z: {elevInvBlock:F3}\n" +
                        $"{solarSet.CableInvToCtTag} Z: {elevInvCab:F3}",
                        "Elevation Mismatch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return null;
                }

                // Definimos offset por defecto
                double offsetDistance = 0.15;

                // Obtenemos Regiones de los Inversores
                if (!cls_00_ProcessPolysToRegions.ProcessPolysToRegions(
                    ed, tr, btr, psrContInv.Value, solarSet.PolyInvTag, offsetDistance, projectUnits,
                    out List<Region> validRegionEntity,
                    out Dictionary<Handle, Handle> dictPolyToRegionContInv
                )) return null;

                // Obtenemos los Ids
                HashSet<ObjectId> psrInvLabIds = new HashSet<ObjectId>(psrInvLab.Value.GetObjectIds());
                HashSet<ObjectId> psrInvBlockIds = new HashSet<ObjectId>(psrInvBlock.Value.GetObjectIds());
                HashSet<ObjectId> psrInvCabIds = new HashSet<ObjectId>(psrInvCab.Value.GetObjectIds());

                // Obtenemos Label por Inversor
                Dictionary<Region, string> labelByEntity = 
                    cls_16_GetDictLabelByEnt.GetDictLabelByEnt(
                        tr, validRegionEntity, psrInvLabIds, solarSet.EntNoLabelValue, solarSet.EntMultiLabelValue, 
                        out HashSet<ObjectId> unusedLabelIds
                    );
                // Identificamos regiones con etiquetas invalidas
                List<Region> invalidRegions = labelByEntity
                    .Where(kvp =>
                        kvp.Value == solarSet.EntNoLabelValue ||
                        kvp.Value == solarSet.EntMultiLabelValue
                    )
                    .Select(kvp => kvp.Key).ToList();

                // Obtenemos Cable por Inversor
                Dictionary<ObjectId, object> cableByEntity = cls_16_GetDictMeasureCablesN2.GetDictMeasureCablesN2(
                    tr, psrInvBlockIds, psrInvCabIds, solarSet.EntNoCableValue, solarSet.EntMultiCableValue, cableLengthCorrectionFactor, cableLengthFixedAllowance, cableNumberOfConductors,
                    out HashSet<ObjectId> usedCableIds
                );
                // Inversores con error de cable
                List<ObjectId> invalidInverterIds = cableByEntity
                    .Where(kvp =>
                        kvp.Value is string s &&
                        (s == solarSet.EntNoCableValue || s == solarSet.EntMultiCableValue)
                    )
                    .Select(kvp => kvp.Key).ToList();

                // Contadores
                int invertersWithNoCable = cableByEntity.Count(kvp => kvp.Value is string s && s == solarSet.EntNoCableValue);
                int invertersWithMultipleCables = cableByEntity.Count(kvp => kvp.Value is string s && s == solarSet.EntMultiCableValue);

                // Cables no usados en ningun inversor
                HashSet<ObjectId> unusedCableIds = new HashSet<ObjectId>(psrInvCabIds);
                unusedCableIds.ExceptWith(usedCableIds);

                bool hasLabelErrors = invalidRegions.Count > 0 || unusedLabelIds.Count > 0;
                bool hasCableErrors = invalidInverterIds.Count > 0 || unusedCableIds.Count > 0;
                // Ids a aislar
                HashSet<ObjectId> idsToIsolate = new HashSet<ObjectId>();
                // Validamos
                if (hasLabelErrors)
                {
                    // Regiones
                    idsToIsolate.UnionWith(cls_16_ValidateDataEnt.GetInvalidLabelRegionIds(invalidRegions));
                    // Etiquetas no usadas
                    idsToIsolate.UnionWith(unusedLabelIds);
                }
                // Validamos
                if (hasCableErrors)
                {
                    // Inversores con error de cable
                    idsToIsolate.UnionWith(invalidInverterIds);
                    // Cables sin uso
                    idsToIsolate.UnionWith(unusedCableIds);
                    // Mensaje
                    cls_16_ValidateDataEnt.ShowCableValidationMessageInv(
                        invertersWithNoCable, invertersWithMultipleCables, unusedCableIds.Count
                    );
                }
                // Si hay cualquier error → borrar + aislar una sola vez
                if (idsToIsolate.Count > 0)
                {
                    // Obtenemos regiones a borrar
                    List<Region> regionsToDelete = validRegionEntity
                        .Where(r => !idsToIsolate.Contains(r.ObjectId))
                        .ToList();
                    // Iteramos
                    foreach (Region region in regionsToDelete)
                    {
                        // Borramos
                        cls_00_DeleteEntity.DeleteEntity(region);
                    }
                    // Aislamos
                    cls_00_IsolateEntities.IsolateObjects(ed, idsToIsolate);
                    // Finalizamos
                    return null;
                }

                HashSet<ObjectId> psrInvBlockIdsInRegion = new HashSet<ObjectId>();
                // Creamos el diccionario Region → Inversores
                Dictionary<Region, List<DBObject>> regionData =
                    new Dictionary<Region, List<DBObject>>();
                // Asignamos Inversores por interseccion
                int blockRefInvAddedByInter = cls_16_ElemByRegionByInter.AssignEntitiesByInter(
                    tr, psrInvBlockIds, psrInvBlockIdsInRegion, validRegionEntity, regionData
                );

                // Diccionario final: Region → Inversores + Label + Cable
                Dictionary<Region, List<(ObjectId, string, object)>> inverterDataByRegion =
                    cls_16_GetDictByInvCable.GetDictByInvCable(regionData, labelByEntity, cableByEntity);

                // Obtenemos el dict a exportar
                Dictionary<string, object> excelData = cls_16_BuildDataN2.BuildDataN2(inverterDataByRegion);

                // Definimos headers
                List<string> headers = new List<string>
                {
                    nameof(EntityExcelRow.InverterHandle),
                    nameof(EntityExcelRow.InverterLabel),
                    nameof(EntityExcelRow.CableHandle),
                    nameof(EntityExcelRow.CableLayer),
                    nameof(EntityExcelRow.CableLength),
                    nameof(EntityExcelRow.CableLengthCorrectionFactor),
                    nameof(EntityExcelRow.CableExtraLength),
                    nameof(EntityExcelRow.CableLengthFixedAllowance),
                    nameof(EntityExcelRow.CableLengthCorrectedTotal),
                    nameof(EntityExcelRow.NumberOfConductors),
                    nameof(EntityExcelRow.TotalInstalledCableLength)
                };

                // Exportamos a Excel
                cls_00_ExportToExcelObjectDictExi_OpenXml.ExportObjectDictToExcelExi(
                    excelPath, excelData, headers,
                    sheetName: solarSet.SheetNameInv,
                    tableName: solarSet.TableNameInv
                );

                List<EntityExcelRow> rows = excelData.Values.OfType<EntityExcelRow>().ToList();
                char[] validSeparators = autoSettings.ValidSeparators;

                // Campos
                // CT
                Dictionary<string, (double cableLength, double cableLengthCorrected, double cableLengthCorrectedTotal, double totalInstalledCableLength)> summaryByCT =
                    cls_16_GetCabLengthSummary.BuildCableSummaryWithPhases(rows, r => r.InverterLabel, 1, validSeparators);

                // Primera columna libre a la derecha de la tabla principal
                int col = headers.Count + 2; 

                // Exportamos Tablas Resumen
                cls_00_ExportEntCabSummary.ExportEntCabSummaryWithPhases(
                    excelPath, solarSet.SheetNameInv, summaryByCT, solarSet.SummaryNameCt, col
                );

                // return
                return excelData.Count;
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show($"ERROR in ProcessMeasureCablesN2:\n{ex.Message}\n{ex.StackTrace}");
                // Finalizamos
                return null;
            }
        }

        





    }
}
