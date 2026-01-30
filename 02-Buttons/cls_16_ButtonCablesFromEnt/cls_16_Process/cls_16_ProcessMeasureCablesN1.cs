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
    internal class cls_16_ProcessMeasureCablesN1
    {
        public static int? ProcessMeasureCablesN1(
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
                if (!cls_16_GetEntMeasureCablesN1.GetEntMeasureCablesN1(
                    ed, docLayers, solarSet, 
                    out SelectionSet analyzePoly, out PromptSelectionResult psrString, 
                    out PromptSelectionResult psrStringLab, out PromptSelectionResult psrStringCab
                )) return null;

                // Validamos elevaciones
                if (!cls_16_GetElevMeasureCablesN1.GetElevMeasureCablesN1(
                    tr, solarSet, psrString, psrStringLab, psrStringCab,
                    out double elevString, out double elevStringLabel, out double elevStringCab
                )) return null;

                // Validamos elevaciones entre Entidades
                if (Math.Abs(elevString - elevStringLabel) > 1e-6 ||
                    Math.Abs(elevString - elevStringCab) > 1e-6 
                )
                {
                    // Mensaje
                    MessageBox.Show(
                        $"⚠ Elevations are inconsistent across entities.\n\n" +
                        $"{solarSet.PolyStringTag} Z: {elevString:F3}\n" +
                        $"{solarSet.LabelStringTag} Z: {elevStringLabel:F3}\n" +
                        $"{solarSet.CableN1Tag} Z: {elevStringCab:F3}",
                        "Elevation Mismatch",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return null;
                }

                // Definimos offset por defecto
                double offsetDistance = 0.01;

                // Obtenemos Regiones de los Strings
                if (!cls_00_ProcessPolysToRegions.ProcessPolysToRegions(
                    ed, tr, btr, psrString.Value, solarSet.PolyStringTag, offsetDistance, projectUnits,
                    out List<Region> validRegionEntity, out Dictionary<Handle, Handle> dictPolyToRegionString
                )) return null;

                // Obtenemos los Ids
                HashSet<ObjectId> psrStringIds = new HashSet<ObjectId>(psrString.Value.GetObjectIds());
                HashSet<ObjectId> psrStringLabIds = new HashSet<ObjectId>(psrStringLab.Value.GetObjectIds());
                HashSet<ObjectId> psrStringCabIds = new HashSet<ObjectId>(psrStringCab.Value.GetObjectIds());

                // Validamos estructura de las etiquetas
                if (!cls_00_MTextObjectsByLayer.AllLabelsHaveSameFieldCount(
                    tr, psrStringLabIds, autoSettings,
                    out int fieldCount, out List<string> referenceFields
                )) return null;

                // Valores por defecto
                string[] defaultSummaries =
                {
                    solarSet.SummaryNameCtKey,
                    solarSet.SummaryNameInvKey,
                    solarSet.SummaryNameTrackKey
                };
                // Summary (usuario)
                Dictionary<string, string> fieldOrderDict =
                    cls_16_GetCabLengthSummary.BuildFieldSummaryMappingDictionary(referenceFields, defaultSummaries);

                // Obtenemos Label por String
                Dictionary<Region, string> labelByEntity = cls_16_GetDictLabelByEnt.GetDictLabelByEnt(
                    tr, validRegionEntity, psrStringLabIds, solarSet.EntNoLabelValue, solarSet.EntMultiLabelValue, 
                    out HashSet<ObjectId> unusedLabelIds
                );
                // Identificamos regiones con etiquetas invalidas
                List<Region> invalidRegions = labelByEntity
                    .Where(kvp =>
                        kvp.Value == solarSet.EntNoLabelValue ||
                        kvp.Value == solarSet.EntMultiLabelValue
                    )
                    .Select(kvp => kvp.Key).ToList();
                
                // Obtenemos Cable por String
                Dictionary<Region, object> cableByEntity = cls_16_GetDictMeasureCablesN1.GetDictMeasureCablesN1BIS(
                    tr, db, validRegionEntity, dictPolyToRegionString,
                    psrStringCabIds, solarSet.EntNoCableValue, solarSet.EntMultiCableValue, 
                    cableLengthCorrectionFactor, cableLengthFixedAllowance, cableNumberOfConductors,
                    out HashSet<ObjectId> usedCableIds
                );
                // Regiones con error de cable
                List<Region> invalidCableRegions = cableByEntity
                    .Where(kvp =>
                        kvp.Value is string s &&
                        (s == solarSet.EntNoCableValue || s == solarSet.EntMultiCableValue)
                    )
                    .Select(kvp => kvp.Key).ToList();

                // Contadores
                int regionsWithNoCable = cableByEntity.Count(kvp => kvp.Value is string s && s == solarSet.EntNoCableValue);
                int regionsWithMultipleCables = cableByEntity.Count(kvp =>kvp.Value is string s && s == solarSet.EntMultiCableValue);

                // Cables no usados en ninguna region
                HashSet<ObjectId> unusedCableIds = new HashSet<ObjectId>(psrStringCabIds);
                unusedCableIds.ExceptWith(usedCableIds);

                bool hasLabelErrors = invalidRegions.Count > 0 || unusedLabelIds.Count > 0;
                bool hasCableErrors = invalidCableRegions.Count > 0 || unusedCableIds.Count > 0;
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
                    // Cables sin uso
                    idsToIsolate.UnionWith(cls_16_ValidateDataEnt.GetInvalidCableEntityIds(invalidCableRegions, unusedCableIds));
                    // Mensaje
                    cls_16_ValidateDataEnt.ShowCableValidationMessageStr(
                        regionsWithNoCable, regionsWithMultipleCables, unusedCableIds.Count
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

                // Obtenemos el dict a exportar
                Dictionary<string, object> excelData = cls_16_BuildDataN1.BuildDataN1(
                    validRegionEntity, labelByEntity, cableByEntity, dictPolyToRegionString
                );

                // Definimos headers
                List<string> headers = new List<string>
                {
                    nameof(EntityExcelRow.StringHandle),
                    nameof(EntityExcelRow.StringLabel),
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
                    sheetName: solarSet.SheetNameStr,
                    tableName: solarSet.TableNameStr
                );

                // Summary → índice físico
                Dictionary<string, int> summaryIndexDict =
                    cls_16_GetCabLengthSummary.BuildSummaryIndexDictionary(referenceFields, fieldOrderDict);

                // Summary → índices jerárquicos
                Dictionary<string, int[]> hierarchyIndices =
                    cls_16_GetCabLengthSummary.BuildHierarchicalGroupIndices(summaryIndexDict, solarSet);

                // Definimos resumenes
                var summaryDefinitions = new[]
                {
                    new
                    {
                        Key = solarSet.SummaryNameCtKey,
                        DisplayName = solarSet.SummaryNameCt
                    },
                    new
                    {
                        Key = solarSet.SummaryNameInvKey,
                        DisplayName = solarSet.SummaryNameInv
                    },
                    new
                    {
                        Key = solarSet.SummaryNameTrackKey,
                        DisplayName = solarSet.SummaryNameTrack
                    }
                };

                List<EntityExcelRow> rows = excelData.Values.OfType<EntityExcelRow>().ToList();
                char[] validSeparators = autoSettings.ValidSeparators;
                // Construcción de resumenes
                Dictionary<string, Dictionary<string, (double, double, double, double)>> summaries =
                    new Dictionary<string, Dictionary<string, (double, double, double, double)>>();
                // Iteramos
                foreach (var def in summaryDefinitions)
                {
                    // Validamos
                    if (hierarchyIndices.TryGetValue(def.Key, out int[] groupIndices))
                    {
                        summaries[def.Key] = cls_16_GetCabLengthSummary.BuildCableSummaryWithHierarchy(
                            rows, r => r.StringLabel, groupIndices, validSeparators
                        );
                    }
                }

                int col = headers.Count + 2;
                // Iteramos 
                foreach (var def in summaryDefinitions)
                {
                    // Validamos
                    if (!summaries.TryGetValue(def.Key, out var summaryData)) continue;
                    // Exportamos a Excel
                    cls_00_ExportEntCabSummary.ExportEntCabSummaryWithPhases(
                        excelPath, solarSet.SheetNameStr, summaryData, def.DisplayName, col
                    );

                    // Cada resumen ocupa 6 columnas
                    col += 6;
                }

                // return
                return excelData.Count;
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show(
                    $"ERROR in ProcessMeasureCablesN1:\n{ex.Message}\n{ex.StackTrace}"
                );
                // Finalizamos
                return null;
            }
        }

        





    }
}
