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
    internal class cls_16_ProcessCablesFromInv
    {
        public static int? ProcessCablesFromInverter(
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
                if (!cls_16_GetRequiredEntInv.GetRequiredEntInv(
                    ed, docLayers, solarSet,
                    out SelectionSet analyzePoly,
                    out PromptSelectionResult psrContInv, out PromptSelectionResult psrInvLab,
                    out PromptSelectionResult psrInvBlock, out PromptSelectionResult psrInvCab
                )) return null;

                // Validamos elevaciones
                if (!cls_16_GetRequiredElevInv.GetRequiredElevInv(
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

                // Obtenemos Regiones de los Contornos Inversores
                if (!cls_00_ProcessPolysToRegions.ProcessPolysToRegions(
                    ed, tr, btr, psrContInv.Value, solarSet.PolyInvTag, offsetDistance, projectUnits,
                    out List<Region> validRegionEntity,
                    out Dictionary<Handle, Handle> dictPolyToRegionContInv
                )) return null;

                // Obtenemos los Ids
                HashSet<ObjectId> psrInvLabIds = new HashSet<ObjectId>(psrInvLab.Value.GetObjectIds());
                HashSet<ObjectId> psrInvBlockIds = new HashSet<ObjectId>(psrInvBlock.Value.GetObjectIds());
                HashSet<ObjectId> psrInvCabIds = new HashSet<ObjectId>(psrInvCab.Value.GetObjectIds());

                string noLabelValue = "Inversor sin etiqueta";
                string multipleLabelValue = "Inversor con más de 1 etiqueta";
                // Obtenemos Label por Inversor
                Dictionary<Region, string> labelByEntity = 
                    cls_16_GetDictLabelByEnt.GetDictLabelByEnt(
                        tr, validRegionEntity, psrInvLabIds, noLabelValue, multipleLabelValue
                    );

                // Identificamos regiones con etiquetas invalidas
                List<Region> invalidRegions = labelByEntity
                    .Where(kvp =>
                        kvp.Value == noLabelValue ||
                        kvp.Value == multipleLabelValue
                    )
                    .Select(kvp => kvp.Key).ToList();
                // Validar y Aislar
                if (invalidRegions.Count > 0)
                {
                    cls_16_ValidateDataEnt.ValidateRegEnt(ed, validRegionEntity, invalidRegions);
                    // Finalizamos
                    return null;
                }

                string noCableValue = "Inversor sin cable conectado";
                string multipleCableValue = "Inversor con más de 1 cable conectado";
                // Obtenemos Cable por Inversor
                Dictionary<ObjectId, object> cableByEntity = cls_16_GetDictCableByInv.GetDictCableByInv(
                    tr, psrInvBlockIds, psrInvCabIds, noCableValue, multipleCableValue,
                    out HashSet<ObjectId> usedCableIds, tolerance: 1e-4
                );

                // Inversores con error de cable
                List<ObjectId> invalidInverterIds = cableByEntity
                    .Where(kvp =>
                        kvp.Value is string s &&
                        (s == noCableValue || s == multipleCableValue)
                    )
                    .Select(kvp => kvp.Key).ToList();

                // Contadores
                int invertersWithNoCable = cableByEntity.Count(kvp =>kvp.Value is string s && s == noCableValue);
                int invertersWithMultipleCables = cableByEntity.Count(kvp =>kvp.Value is string s && s == multipleCableValue);

                // Cables no usados en ningun inversor
                HashSet<ObjectId> unusedCableIds = new HashSet<ObjectId>(psrInvCabIds);
                unusedCableIds.ExceptWith(usedCableIds);
                // Validamos
                if (invalidInverterIds.Count > 0 || unusedCableIds.Count > 0)
                {
                    cls_16_ValidateDataEnt.ValidatCableInv(
                        ed, invertersWithNoCable, invertersWithMultipleCables, unusedCableIds,
                        invalidInverterIds, validRegionEntity
                    );
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
                Dictionary<string, object> excelData = cls_16_BuildInvData.BuildInvData(inverterDataByRegion);

                // Definimos headers
                List<string> headers = new List<string>
                {
                    nameof(EntityExcelRow.InverterHandle),
                    nameof(EntityExcelRow.InverterLabel),
                    nameof(EntityExcelRow.CableHandle),
                    nameof(EntityExcelRow.CableLayer),
                    nameof(EntityExcelRow.CableLength)
                };

                // Exportamos a Excel
                cls_00_ExportToExcelObjectDictExi_OpenXml.ExportObjectDictToExcelExi(
                    excelPath, excelData, headers,
                    sheetName: "CablesFromInverters",
                    tableName: "Inverters"
                );

                List<EntityExcelRow> rows =
                    excelData.Values.OfType<EntityExcelRow>().ToList();
                // Exportamos Tablas Resumen

                char[] validSeparators = solarSet.ValidSeparators;

                // CT: 1 campo
                var summaryByCT =
                    cls_16_GetCabLengthSummary.BuildCableSummaryInv(rows, 1, validSeparators);

                // Primera columna libre a la derecha de la tabla principal
                int col = headers.Count + 2; // deja 1 columna de margen

                cls_00_ExportStringCabSummary.ExportStringCabSummary(
                    excelPath, "CablesFromInverters", summaryByCT, "Summary by CT", col
                );

                // return
                return excelData.Count;
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show($"ERROR in ProcessCablesFromInverter:\n{ex.Message}\n{ex.StackTrace}");
                // Finalizamos
                return null;
            }
        }

        





    }
}
