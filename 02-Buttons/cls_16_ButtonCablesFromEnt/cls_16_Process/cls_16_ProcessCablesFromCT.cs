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
    internal class cls_16_ProcessCablesFromCT
    {
        public static int? ProcessCablesFromCT(
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
                if (!cls_16_GetRequiredEntCt.GetRequiredEntCt(
                    ed, docLayers, solarSet, out PromptSelectionResult psrCtLab,
                    out PromptSelectionResult psrCtBlock, out PromptSelectionResult psrCtCab
                )) return null;

                // Validamos elevaciones
                if (!cls_16_GetRequiredElevCt.GetRequiredElevCt(
                    tr, solarSet, psrCtLab, psrCtBlock, psrCtCab,
                    out double elevCtLabel, out double elevCtBlock, out double elevCtCab
                )) return null;

                // Validamos elevaciones entre Entidades
                if (Math.Abs(elevCtLabel - elevCtBlock) > 1e-6 ||
                    Math.Abs(elevCtLabel - elevCtCab) > 1e-6
                )
                {
                    // Mensaje
                    MessageBox.Show(
                        $"⚠ Elevations are inconsistent across entities.\n\n" +
                        $"{solarSet.LabelCtTag} Z: {elevCtLabel:F3}\n" +
                        $"{solarSet.BlockRefCtTag} Z: {elevCtBlock:F3}\n" +
                        $"{solarSet.CableCtToEstTag} Z: {elevCtCab:F3}",
                        "Elevation Mismatch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return null;
                }

                // Obtenemos los Ids
                HashSet<ObjectId> psrCtLabIds = new HashSet<ObjectId>(psrCtLab.Value.GetObjectIds());
                HashSet<ObjectId> psrCtBlockIds = new HashSet<ObjectId>(psrCtBlock.Value.GetObjectIds());
                HashSet<ObjectId> psrCtCabIds = new HashSet<ObjectId>(psrCtCab.Value.GetObjectIds());

                // Obtenemos Label por CT
                Dictionary<ObjectId, ObjectId> labelByCtDict = 
                    cls_16_GetDictLabelByCt.GetDictLabelByCt(tr, psrCtLabIds, psrCtBlockIds);

                // Bloques CT con label
                HashSet<ObjectId> blocksWithLabel =
                    new HashSet<ObjectId>(labelByCtDict.Values);
                // Bloques CT sin label
                List<ObjectId> ctBlocksWithoutLabel = psrCtBlockIds
                    .Where(id => !blocksWithLabel.Contains(id))
                    .ToList();
                // Validamos
                if (ctBlocksWithoutLabel.Count > 0)
                {
                    // Aislamos los bloques CT sin etiqueta
                    cls_00_IsolateEntities.IsolateObjects(
                        ed, new HashSet<ObjectId>(ctBlocksWithoutLabel)
                    );
                    // Finalizamos
                    return null;
                }

                string defaultValue = "Subestacion";
                // Obtenemos info
                Dictionary<ObjectId, CableCtInfo> cableDict = 
                    cls_16_GetDictCableByCt.GetDictCableByCt(tr, psrCtCabIds, labelByCtDict, defaultValue);

                // Obtenemos el dict a exportar
                Dictionary<string, object> excelData = 
                    cls_16_BuildCtData.BuildCtData(tr, cableDict, defaultValue);

                // Definimos headers
                List<string> headers = new List<string>
                {
                    nameof(EntityExcelRow.CableHandle),
                    nameof(EntityExcelRow.CtLabelFrom),
                    nameof(EntityExcelRow.CtLabelTo),
                    nameof(EntityExcelRow.CableLength)
                };

                // Exportamos
                cls_00_ExportToExcelObjectDictExi_OpenXml.ExportObjectDictToExcelExi(
                    excelPath, excelData, headers, 
                    sheetName: "CablesFromCT", tableName: "CT"
                );

                // return
                return excelData.Count;
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show($"ERROR in ProcessCablesFromCT:\n{ex.Message}\n{ex.StackTrace}");
                // Finalizamos
                return null;
            }
        }

        





    }
}
