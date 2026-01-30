using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.IsolateEntities;
using TYPSA.SharedLib.Excel;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_ProcessMeasureCablesMV
    {
        public static int? ProcessMeasureCablesMV(
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

                // Obtenemos el listado de capas del documento
                List<string> docLayers = 
                    cls_00_GetLayerNamesFromDoc.GetLayerNamesFromDoc(db);

                // Seleccionamos Entidades
                if (!cls_16_GetEntMeasureCablesMV.GetEntMeasureCablesMV(
                    ed, docLayers, solarSet, 
                    out PromptSelectionResult psrCtLab, out PromptSelectionResult psrCtBlock, 
                    out PromptSelectionResult psrEstBlock, out PromptSelectionResult psrCtCab
                )) return null;

                // Validamos elevaciones
                if (!cls_16_GetElevMeasureCablesMV.GetElevMeasureCablesMV(
                    tr, solarSet, psrCtLab, psrCtBlock, psrEstBlock, psrCtCab,
                    out double elevCtLabel, out double elevCtBlock,
                    out double elevEstBlock, out double elevCtCab
                )) return null;

                // Validamos elevaciones entre Entidades
                if (Math.Abs(elevCtLabel - elevCtBlock) > 1e-6 ||
                    Math.Abs(elevCtLabel - elevEstBlock) > 1e-6 ||
                    Math.Abs(elevCtLabel - elevCtCab) > 1e-6
                )
                {
                    // Mensaje
                    MessageBox.Show(
                        $"⚠ Elevations are inconsistent across entities.\n\n" +
                        $"{solarSet.LabelCtTag} Z: {elevCtLabel:F3}\n" +
                        $"{solarSet.BlockRefCtTag} Z: {elevCtBlock:F3}\n" +
                        $"{solarSet.BlockRefEstTag} Z: {elevEstBlock:F3}\n" +
                        $"{solarSet.CableMVTag} Z: {elevCtCab:F3}",
                        "Elevation Mismatch",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return null;
                }

                // Obtenemos los Ids
                HashSet<ObjectId> psrCtLabIds = new HashSet<ObjectId>(psrCtLab.Value.GetObjectIds());
                HashSet<ObjectId> psrCtBlockIds = new HashSet<ObjectId>(psrCtBlock.Value.GetObjectIds());
                HashSet<ObjectId> psrEstBlockIds = new HashSet<ObjectId>(psrEstBlock.Value.GetObjectIds());
                HashSet<ObjectId> psrCtCabIds = new HashSet<ObjectId>(psrCtCab.Value.GetObjectIds());

                // Obtenemos Label por CT
                Dictionary<ObjectId, ObjectId> labelByCtDict = 
                    cls_16_GetDictLabelByCt.GetDictLabelByCt(tr, psrCtLabIds, psrCtBlockIds);

                // CT con label
                HashSet<ObjectId> blocksWithLabel = new HashSet<ObjectId>(labelByCtDict.Values);
                // CT sin label
                List<ObjectId> ctBlocksWithoutLabel = psrCtBlockIds
                    .Where(id => !blocksWithLabel.Contains(id)).ToList();
                // Labels CT sin asignar
                List<ObjectId> ctLabelsWithoutBlock = psrCtLabIds
                    .Where(id => !labelByCtDict.ContainsKey(id))
                    .ToList();

                HashSet<ObjectId> invalidCableIds;
                HashSet<ObjectId> connectedCtBlockIds;
                HashSet<ObjectId> connectedEstBlockIds;
                HashSet<ObjectId> connectedEstCableIds;
                // Obtenemos info
                Dictionary<ObjectId, EntityExcelRow> cableDict = cls_16_GetDictMeasureCablesMV.GetDictMeasureCablesMV(
                    tr, psrCtCabIds, psrEstBlockIds, labelByCtDict,
                    cableLengthCorrectionFactor, cableLengthFixedAllowance, cableNumberOfConductors,
                    out invalidCableIds, out connectedCtBlockIds, out connectedEstBlockIds, out connectedEstCableIds
                );

                // CT sin cable
                List<ObjectId> ctBlocksWithoutCable = psrCtBlockIds
                    .Where(id => !connectedCtBlockIds.Contains(id))
                    .ToList();
                // Estaciones sin cable
                List<ObjectId> estBlocksWithoutCable = psrEstBlockIds
                    .Where(id => !connectedEstBlockIds.Contains(id))
                    .ToList();

                // Numero esperado de cables CT–EST segun numero circuitos
                int expectedCableCount = psrCtCabIds
                    .Select(id => tr.GetObject(id, OpenMode.ForRead) as Entity)
                    .Where(ent => ent != null).Select(ent => ent.Layer)
                    .Distinct().Count();
                // Validamos si faltan cables por llegar a la estacion
                bool missingEstConnections = connectedEstCableIds.Count < expectedCableCount;

                // Acumulamos entidades a aislar
                HashSet<ObjectId> entToIsolate = new HashSet<ObjectId>();
                // CT sin Label
                entToIsolate.UnionWith(ctBlocksWithoutLabel);
                // Label CT sin asignar
                entToIsolate.UnionWith(ctLabelsWithoutBlock);
                // Cables CT–Estación inválidos
                entToIsolate.UnionWith(invalidCableIds);
                // CT sin cable
                entToIsolate.UnionWith(ctBlocksWithoutCable);
                // Estaciones sin cable
                entToIsolate.UnionWith(estBlocksWithoutCable);
                // Estaciones con conexiones incompletas
                if (missingEstConnections)
                {
                    entToIsolate.UnionWith(psrEstBlockIds);
                }
                // Validamos
                if (entToIsolate.Count > 0)
                {
                    // Aislamos los bloques CT sin etiqueta
                    cls_00_IsolateEntities.IsolateObjects(ed, entToIsolate);
                    // Finalizamos
                    return null;
                }

                string defaultValue = "Subestacion";
                // Obtenemos el dict a exportar
                Dictionary<string, object> excelData = cls_16_BuildDataMV.BuildCtDataByLayer(tr, cableDict, defaultValue);

                // Definimos headers
                List<string> headers = new List<string>
                {
                    nameof(EntityExcelRow.CtLabelFrom),
                    nameof(EntityExcelRow.CtLabelTo),
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

                // Exportamos
                cls_00_ExportToExcelObjectDictExi_OpenXml.ExportObjectDictToExcelExi(
                    excelPath, excelData, headers, 
                    sheetName: solarSet.SheetNameCt, tableName: solarSet.TableNameCt
                );

                // return
                return excelData.Count;
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show($"ERROR in ProcessMeasureCablesMV:\n{ex.Message}\n{ex.StackTrace}");
                // Finalizamos
                return null;
            }
        }

        





    }
}
