using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.ProcessPolyAndRegion;
using TYPSA.SharedLib.Excel;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_ProcessCablesFromStr
    {
        public static int? ProcessCablesFromString(
            Editor ed, 
            Database db, 
            Transaction tr, 
            BlockTableRecord btr, 
            string excelPath,
            string projectUnits
        )
        {
            // try
            try
            {
                // Obtenemos settings
                SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();

                // Obtenemos el listado de capas del documento
                List<string> docLayers = 
                    cls_00_GetLayerNamesFromDoc.GetLayerNamesFromDoc(db);

                // Seleccionamos Entidades
                if (!cls_16_GetRequiredEntString.GetRequiredEntString(
                    ed, docLayers, solarSet,
                    out SelectionSet analyzePoly, out PromptSelectionResult psrString, 
                    out PromptSelectionResult psrStringLab, out PromptSelectionResult psrStringCab
                )) return null;

                // Validamos elevaciones
                if (!cls_16_GetRequiredElevString.GetRequiredElevString(
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
                        $"{solarSet.CableStringToInvTag} Z: {elevStringCab:F3}",
                        "Elevation Mismatch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return null;
                }

                // Definimos offset por defecto
                //double offsetDistance = 0.15;
                double offsetDistance = 0.01;

                // Obtenemos Regiones de los Contornos Strings
                if (!cls_00_ProcessPolysToRegions.ProcessPolysToRegions(
                    ed, tr, btr, psrString.Value, solarSet.PolyStringTag, offsetDistance, projectUnits,
                    out List<Region> validRegionEntity, out Dictionary<Handle, Handle> dictPolyToRegionString
                )) return null;

                // Obtenemos los Ids
                HashSet<ObjectId> psrStringIds = new HashSet<ObjectId>(psrString.Value.GetObjectIds());
                HashSet<ObjectId> psrStringLabIds = new HashSet<ObjectId>(psrStringLab.Value.GetObjectIds());
                HashSet<ObjectId> psrStringCabIds = new HashSet<ObjectId>(psrStringCab.Value.GetObjectIds());

                string noLabelValue = "String sin etiqueta";
                string multipleLabelValue = "String con más de 1 etiqueta";
                // Obtenemos Label por String
                Dictionary<Region, string> labelByEntity = cls_16_GetDictLabelByEnt.GetDictLabelByEnt(
                    tr, validRegionEntity, psrStringLabIds, noLabelValue, multipleLabelValue
                );

                // Identificamos regiones con etiquetas invalidas
                List<Region> invalidRegions = labelByEntity
                    .Where(kvp =>
                        kvp.Value == noLabelValue ||
                        kvp.Value == multipleLabelValue
                    )
                    .Select(kvp => kvp.Key).ToList();
                // Validamos
                if (invalidRegions.Count > 0)
                {
                    cls_16_ValidateDataEnt.ValidateRegEnt(ed, validRegionEntity, invalidRegions);
                    // Finalizamos
                    return null;
                }

                string noCableValue = "String sin cable conectado";
                string multipleCableValue = "String con más de 1 cable conectado";
                // Obtenemos Cable por String
                Dictionary<Region, object> cableByEntity = cls_16_GetDictCableByString.GetDictCableByString(
                    tr, db, validRegionEntity, dictPolyToRegionString, 
                    psrStringCabIds, noCableValue, multipleCableValue,
                    out HashSet<ObjectId> usedCableIds
                );

                // Regiones con error de cable
                List<Region> invalidCableRegions = cableByEntity
                    .Where(kvp =>
                        kvp.Value is string s &&
                        (s == noCableValue || s == multipleCableValue)
                    )
                    .Select(kvp => kvp.Key).ToList();

                // Contadores
                int regionsWithNoCable = cableByEntity.Count(kvp => kvp.Value is string s && s == noCableValue);
                int regionsWithMultipleCables = cableByEntity.Count(kvp =>kvp.Value is string s && s == multipleCableValue);

                // Cables no usados en ninguna region
                HashSet<ObjectId> unusedCableIds = new HashSet<ObjectId>(psrStringCabIds);
                unusedCableIds.ExceptWith(usedCableIds);
                // Validamos
                if (invalidCableRegions.Count > 0 || unusedCableIds.Count > 0)
                {
                    cls_16_ValidateDataEnt.ValidatCableStr(
                        ed, regionsWithNoCable, regionsWithMultipleCables, unusedCableIds, 
                        invalidCableRegions, validRegionEntity
                    );
                    // Finalizamos
                    return null;
                }

                // Obtenemos el dict a exportar
                Dictionary<string, object> excelData =
                    cls_16_BuildStringData.BuildStringData(
                        validRegionEntity, labelByEntity, cableByEntity, dictPolyToRegionString
                    );

                // Definimos headers
                List<string> headers = new List<string>
                {
                    nameof(EntityExcelRow.StringHandle),
                    nameof(EntityExcelRow.StringLabel),
                    nameof(EntityExcelRow.CableHandle),
                    nameof(EntityExcelRow.CableLayer),
                    nameof(EntityExcelRow.CableLength)
                };

                // Exportamos a Excel
                cls_00_ExportToExcelObjectDictExi_OpenXml.ExportObjectDictToExcelExi(
                    excelPath, excelData, headers,
                    sheetName: "CablesFromStrings",
                    tableName: "Strings"
                );

                List<EntityExcelRow> rows =
                    excelData.Values.OfType<EntityExcelRow>().ToList();
                // Exportamos Tablas Resumen

                char[] validSeparators = solarSet.ValidSeparators;

                // CT: 1 campo
                var summaryByCT =
                    cls_16_GetCabLengthSummary.BuildCableSummaryString(rows, 1, validSeparators);

                // INV: 1 + 2 campo
                var summaryByInverter =
                    cls_16_GetCabLengthSummary.BuildCableSummaryString(rows, 2, validSeparators);

                // TRACKER: 1 + 2 + 3 campo
                var summaryByTracker =
                    cls_16_GetCabLengthSummary.BuildCableSummaryString(rows, 3, validSeparators);

                // Primera columna libre a la derecha de la tabla principal
                int col = headers.Count + 2; // deja 1 columna de margen

                cls_00_ExportStringCabSummary.ExportStringCabSummary(
                    excelPath, "CablesFromStrings", summaryByCT, "Summary by CT", col
                );

                col += 3; // 2 columnas usadas + 1 de margen

                cls_00_ExportStringCabSummary.ExportStringCabSummary(
                    excelPath, "CablesFromStrings", summaryByInverter, "Summary by Inverter", col
                );

                col += 3;

                cls_00_ExportStringCabSummary.ExportStringCabSummary(
                    excelPath, "CablesFromStrings", summaryByTracker, "Summary by Tracker", col
                );

                // return
                return excelData.Count;
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show(
                    $"ERROR in ProcessCablesFromString:\n{ex.Message}\n{ex.StackTrace}"
                );
                // Finalizamos
                return null;
            }
        }

        





    }
}
