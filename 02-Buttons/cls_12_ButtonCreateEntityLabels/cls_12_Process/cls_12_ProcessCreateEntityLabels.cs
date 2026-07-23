using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.DeleteEntities;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetEntityCoordinates;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;
using TYPSA.SharedLib.Autocad.ProcessPolyAndRegion;
using TYPSA.SharedLib.Autocad.ProjectUnits;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_ProcessCreateEntityLabels
    {
        private static void ShowFieldMapResult(
            Dictionary<string, string> fieldMapResult
        )
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("Field Mapping Summary");
            sb.AppendLine("═══════════════════════════════════════");

            foreach (var kvp in fieldMapResult)
            {
                sb.AppendLine(
                    $"Field: {kvp.Key}  →  Assigned Type: {kvp.Value}"
                );
            }

            ShowStringBuilder.ShowInfo(
                "📌 Field Map Result",
                sb.ToString()
            );
        }

        private static void ShowValidRegionOrderList(
            List<(int ctNumber, int invNumber, Region ctRegion, Region invRegion)> validRegionOrderList
        )
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("CT / INV Region Order Summary");
            sb.AppendLine("═══════════════════════════════════════");

            foreach (var item in validRegionOrderList)
            {
                sb.AppendLine(
                    $"CT: {item.ctNumber} | " +
                    $"INV: {item.invNumber} | " +
                    $"CT Region: {item.ctRegion.Handle} | " +
                    $"INV Region: {item.invRegion.Handle}"
                );
            }

            ShowStringBuilder.ShowInfo(
                "📌 Valid Region Order List",
                sb.ToString()
            );
        }

        private static void ShowInvInCtLayerByInvRegionDebug(
            Dictionary<Region, string> invInCtLayerByInvRegion
        )
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("📌 INV IN CT LAYER BY INV REGION");
            sb.AppendLine();

            // Validamos
            if (
                invInCtLayerByInvRegion == null ||
                invInCtLayerByInvRegion.Count == 0
            )
            {
                sb.AppendLine("❌ No data found.");
            }
            else
            {
                // Iteramos
                foreach (var kvp in invInCtLayerByInvRegion)
                {
                    // Obtener region
                    Region invRegion = kvp.Key;

                    // Obtener layer
                    string layer = kvp.Value;

                    // Obtener handle region
                    string regionHandle =
                        invRegion != null
                            ? invRegion.Handle.ToString()
                            : "NULL";

                    sb.AppendLine(
                        $"🔹 InvRegion: {regionHandle}"
                    );

                    sb.AppendLine(
                        $"   Layer: {layer}"
                    );

                    sb.AppendLine();
                }
            }

            // -----------------------------
            // Mostrar
            // -----------------------------

            ShowStringBuilder.ShowInfo(
                "📌 Inv In CT Layer By Inv Region",
                sb.ToString()
            );
        }

        public static int? ProcessCreateEntityLabels(
            Editor ed, 
            Database db, 
            Transaction tr, 
            BlockTableRecord btr,
            SolarSettings solarSet,
            AutocadSettings autoSettings
        )
        {
            // try
            try
            {
                // -----------------------------
                // Obtener las unidades del proyecto (actuales o elegidas por user)
                // -----------------------------

                string projectUnits = cls_00_ProjectUnits.GetAndSetProjectUnits();
                // Validamos
                if (string.IsNullOrEmpty(projectUnits)) return null;

                // -----------------------------
                // Obtener Text Styles
                // -----------------------------

                List<string> availableTextStyles = cls_00_DocumentInfo.GetAllTextStylesFromDrawing(db);

                // -----------------------------
                // Obtener Capas Documento
                // -----------------------------

                List<string> docLayers = cls_00_GetLayerNamesFromDoc.GetLayerNamesFromDoc(db);

                // -----------------------------
                // Obtener Config Proyecto
                // -----------------------------

                if (!cls_12_CreateEntityLabelsConfig.CreateEntityLabelsConfig(
                    solarSet, availableTextStyles, 
                    out bool isHorizontal, out bool hasMPPT, out bool hasTrackerInfo, 
                    out bool inverterOutsideCt, out string separatorChar, out Dictionary<string, string> labelFieldsDict,
                    out string selectedTextStyle, out AttachmentPoint selectedTextJust, out bool analyzeAllDoc
                )) return null;

                // -----------------------------
                // Seleccionar Entidades
                // -----------------------------

                // Variables comunes
                SelectionSet analyzePoly;
                PromptSelectionResult psrPolyCt;
                PromptSelectionResult psrPolyInv;
                PromptSelectionResult psrBlockRefTrack;
                PromptSelectionResult psrLabelInv;
                string psrPolyCtLayer;
                string psrPolyInvLayer;
                string psrBlockRefTrackLayer;
                string psrLabelInvLayer;

                // Variables adicionales
                PromptSelectionResult psrPolyN2Cable = null;
                PromptSelectionResult psrCtBlock = null;
                PromptSelectionResult psrBlockRefComBox = null;
                string psrPolyN2CableLayer = null;
                string psrCtBlockLayer = null;
                List<string> psrBlockRefComBoxLayers = null;
              
                // Fuera del CT
                if (inverterOutsideCt)
                {
                    // Seleccionamos Entidades
                    if (!cls_12_GetRequiredEntities.GetRequiredEntitiesByInv(
                        ed, solarSet, docLayers, analyzeAllDoc, out analyzePoly,
                        out psrPolyCt, out psrPolyInv, out psrBlockRefTrack, out psrLabelInv,
                        out psrPolyCtLayer, out psrPolyInvLayer, out psrBlockRefTrackLayer, out psrLabelInvLayer
                    )) return null;
                }
                // Dentro del CT
                else
                {
                    // Seleccionamos Entidades
                    if (!cls_12_GetRequiredEntities.GetRequiredEntitiesByComBox(
                        ed, solarSet, docLayers, analyzeAllDoc, out analyzePoly,
                        out psrPolyCt, out psrPolyInv, out psrBlockRefTrack, out psrLabelInv,
                        out psrPolyN2Cable, out psrCtBlock, out psrBlockRefComBox, out psrPolyCtLayer, 
                        out psrPolyInvLayer, out psrBlockRefTrackLayer, out psrLabelInvLayer, 
                        out psrCtBlockLayer, out psrPolyN2CableLayer, out psrBlockRefComBoxLayers
                    )) return null;
                }

                // -----------------------------
                // Crear Capas por defecto
                // -----------------------------

                List<string> layersToCreate = new List<string>
                {
                    solarSet.PolyCtLayer, solarSet.PolyInvLayer, solarSet.BlockRefTrackLayer,
                    solarSet.PolyStringLayer, solarSet.LabelStringLayer, solarSet.LabelInvLayer
                };
                // Creamos las capas por defecto si no existen
                cls_00_CreateLayerIfNotExists.CreateLayersIfNotExist(layersToCreate, db);

                // -----------------------------
                // Comprobar elevaciones
                // -----------------------------

                // Fuera del CT
                if (inverterOutsideCt)
                {
                    if (!cls_12_GetRequiredElev.GetRequiredElevationsByInv(
                        tr, solarSet,
                        psrPolyCt, psrPolyInv, psrBlockRefTrack, psrLabelInv,
                        out double elevPolyCt, out double elevPolyInv,
                        out double elevBlockRefTrack, out double elevLabelInv
                    )) return null;
                    // Validamos elevaciones entre Entidades
                    if (Math.Abs(elevPolyCt - elevPolyInv) > 1e-6 ||
                        Math.Abs(elevPolyCt - elevBlockRefTrack) > 1e-6 ||
                        Math.Abs(elevPolyCt - elevLabelInv) > 1e-6
                    )
                    {
                        // Mensaje
                        MessageBox.Show(
                            $"⚠ Elevations are inconsistent across entities.\n\n" +
                            $"{solarSet.PolyCtTag} Z: {elevPolyCt:F3}\n" +
                            $"{solarSet.PolyInvTag} Z: {elevPolyInv:F3}\n" +
                            $"{solarSet.BlockRefTrackTag} Z: {elevBlockRefTrack:F3}\n" +
                            $"{solarSet.LabelInvTag} Z: {elevLabelInv:F3}",
                            "Elevation Mismatch",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning
                        );
                        // Finalizamos
                        return null;
                    }
                }
                // Dentro del CT
                else
                {
                    if (!cls_12_GetRequiredElev.GetRequiredElevationsByComBox(
                        tr, solarSet,
                        psrPolyCt, psrPolyInv, psrBlockRefTrack, psrLabelInv, psrPolyN2Cable, psrCtBlock, psrBlockRefComBox,
                        out double elevPolyCt, out double elevPolyInv, out double elevBlockRefTrack, out double elevLabelInv,
                        out double elevPolyN2Cable, out double elevBlockRefCt, out double elevBlockRefComBox
                    )) return null;

                    // Validamos elevaciones entre Entidades
                    if (Math.Abs(elevPolyCt - elevPolyInv) > 1e-6 ||
                        Math.Abs(elevPolyCt - elevBlockRefTrack) > 1e-6 ||
                        Math.Abs(elevPolyCt - elevLabelInv) > 1e-6 ||
                        Math.Abs(elevPolyCt - elevPolyN2Cable) > 1e-6 ||
                        Math.Abs(elevPolyCt - elevBlockRefCt) > 1e-6 ||
                        Math.Abs(elevPolyCt - elevBlockRefComBox) > 1e-6
                    )
                    {
                        // Mensaje
                        MessageBox.Show(
                            $"⚠ Elevations are inconsistent across entities.\n\n" +
                            $"{solarSet.PolyCtTag} Z: {elevPolyCt:F3}\n" +
                            $"{solarSet.PolyInvTag} Z: {elevPolyInv:F3}\n" +
                            $"{solarSet.BlockRefTrackTag} Z: {elevBlockRefTrack:F3}\n" +
                            $"{solarSet.LabelInvTag} Z: {elevLabelInv:F3}\n" +
                            $"{solarSet.CableN2Tag} Z: {elevPolyN2Cable:F3}\n" +
                            $"{solarSet.BlockRefComBoxTag} Z: {elevBlockRefComBox:F3}",
                            "Elevation Mismatch",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning
                        );
                        // Finalizamos
                        return null;
                    }
                }

                // -----------------------------
                // Obtener Ids
                // -----------------------------

                HashSet<ObjectId> psrPolyInvIds;
                HashSet<ObjectId> psrBlockRefTrackIds;
                HashSet<ObjectId> psrLabelInvIds;
                // Variables adicionales
                HashSet<ObjectId> psrInvCabIds = null;
                HashSet<ObjectId> psrCtBlockIds = null;
                HashSet<ObjectId> psrBlockRefComBoxIds = null;

                // Fuera del CT
                if (inverterOutsideCt)
                {
                    psrPolyInvIds = new HashSet<ObjectId>(psrPolyInv.Value.GetObjectIds());
                    psrBlockRefTrackIds = new HashSet<ObjectId>(psrBlockRefTrack.Value.GetObjectIds());
                    psrLabelInvIds = new HashSet<ObjectId>(psrLabelInv.Value.GetObjectIds());
                }
                // Dentro del CT
                else
                {
                    psrPolyInvIds = new HashSet<ObjectId>(psrPolyInv.Value.GetObjectIds());
                    psrBlockRefTrackIds = new HashSet<ObjectId>(psrBlockRefTrack.Value.GetObjectIds());
                    psrLabelInvIds = new HashSet<ObjectId>(psrLabelInv.Value.GetObjectIds());
                    psrInvCabIds = new HashSet<ObjectId>(psrPolyN2Cable.Value.GetObjectIds());
                    psrCtBlockIds = new HashSet<ObjectId>(psrCtBlock.Value.GetObjectIds());
                    psrBlockRefComBoxIds = new HashSet<ObjectId>(psrBlockRefComBox.Value.GetObjectIds());
                }

                // -----------------------------
                // Validar estructura etiquetas Inversores/combiner
                // -----------------------------
               
                if (!cls_00_MTextObjectsByLayer.AllLabelsHaveSameFieldCount(
                    tr, psrLabelInvIds, autoSettings, out int fieldCount, out List<string> referenceFields
                )) return null;

                // -----------------------------
                // Obtener claves alfabeticas
                // -----------------------------

                Dictionary<string, string> fieldOrderDict = 
                    cls_12_ProcessOrderEntityLabels.BuildFieldOrderDictionary(referenceFields);
                // Validamos
                if (fieldOrderDict == null) return null;

                // -----------------------------
                // Opciones fijas
                // -----------------------------

                List<string> fixedOptions = new List<string>
                {
                    solarSet.PolyCtTag, solarSet.PolyInvTag, solarSet.PolyInvInCtTag
                };

                // -----------------------------
                // Construir Combo Fields
                // -----------------------------

                List<(string propiedad, List<string> options, string valorDefecto)> comboFields =
                    new List<(string, List<string>, string)>();

                int i = 0;
                // Iteramos
                foreach (string fieldKey in fieldOrderDict.Keys)
                {
                    comboFields.Add((fieldKey, fixedOptions, i < fixedOptions.Count
                        ? fixedOptions[i] 
                        : fixedOptions[0]
                    ));
                    // Contamos
                    i++;
                }

                // -----------------------------
                // Mostrar Combo Form
                // -----------------------------

                Dictionary<string, string> fieldMapResult = cls_00_InstaForm_ComboBox.ComboBoxFormOut_NextToLabel(
                    "The following fields were detected in the Inverter/Combiner labels.\n" +
                    "Please assign the corresponding label type to each field:", comboFields
                );
                // Validamos
                if (fieldMapResult == null) return null;

                // -----------------------------
                // Validar Trackers/Strings
                // -----------------------------

                HashSet<ObjectId> blockRefTrackToIsolate = new HashSet<ObjectId>();
                HashSet<ObjectId> blockRefTrackMultPoly = new HashSet<ObjectId>();
                HashSet<ObjectId> blockRefTrackElevMismatch = new HashSet<ObjectId>();
                // Iteramos
                foreach (ObjectId trkId in psrBlockRefTrack.Value.GetObjectIds())
                {
                    // Validamos Trackers/Strings
                    cls_12_AnalyzeTrackPolys.AnalyzeTrackPolys(
                        tr, trkId, solarSet, blockRefTrackMultPoly,
                        blockRefTrackToIsolate, blockRefTrackElevMismatch
                    );
                }
                // Validamos
                if (!cls_12_IsolateInvalidTrack.IsolateInvalidTrack(
                    ed, solarSet, blockRefTrackMultPoly,
                    blockRefTrackToIsolate, blockRefTrackElevMismatch
                )) return null;

                // -----------------------------
                // Procesar Polys CT/Inversores
                // -----------------------------

                // Definimos offset por defecto
                double offsetDistance = 0.15;
                // Obtenemos Regiones de los Contornos CT
                if (!cls_00_ProcessPolysToRegions.ProcessPolysToRegions(
                    ed, tr, btr, analyzePoly, solarSet.PolyCtTag, offsetDistance, projectUnits,
                    out List<Region> validRegionCt, out Dictionary<Handle, Handle> dictPolyToRegionContGen
                )) return null;

                // Obtenemos Regiones de los Contornos Inversores
                if (!cls_00_ProcessPolysToRegions.ProcessPolysToRegions(
                    ed, tr, btr, psrPolyInv.Value, solarSet.PolyInvTag, offsetDistance, projectUnits,
                    out List<Region> validRegionInv, out Dictionary<Handle, Handle> dictPolyToRegionContInv
                )) return null;

                // -----------------------------
                // Inversores dentro del CT
                // -----------------------------

                Dictionary<ObjectId, object> cableByEntity = null;
                Dictionary<Region, string> invInCtLayerByInvRegion = new Dictionary<Region, string>();
                // Dentro del CT
                if (!inverterOutsideCt)
                {
                    // -----------------------------
                    // Obtener Info Cable por Inversor
                    // -----------------------------

                    cableByEntity = cls_16_GetDictMeasureCablesN2.GetDictMeasureCablesN2(
                        tr, psrBlockRefComBoxIds, psrCtBlockIds, psrInvCabIds, solarSet.EntNoCableValue, solarSet.EntMultiCableValue,
                        psrBlockRefComBoxLayers, out HashSet<ObjectId> cablesConnectedToInv, out HashSet<ObjectId> cablesConnectedToCt
                    );

                    // -----------------------------
                    // Diccionario Region con Inversor
                    // -----------------------------

                    HashSet<ObjectId> psrComBoxBlockIdsInRegion = new HashSet<ObjectId>();
                    // Creamos el diccionario Region-Inversores
                    Dictionary<Region, List<DBObject>> regionData = new Dictionary<Region, List<DBObject>>();

                    // -----------------------------
                    // Asignar Inversores por interseccion
                    // -----------------------------

                    int blockRefInvAddedByInter = cls_16_ElemByRegionByInter.AssignEntitiesByInterIter(
                        tr, psrBlockRefComBoxIds, psrComBoxBlockIdsInRegion, validRegionInv, regionData,
                        radTolerance: 2, boolByGeometryExt: false, toleranceStep: 1
                    );

                    bool showInfoByInter = false;
                    // Debug
                    if (showInfoByInter)
                        cls_16_ElemByRegionByInter.ShowAssignEntitiesByInterSummary(
                            regionData, psrComBoxBlockIdsInRegion, blockRefInvAddedByInter
                        );

                    // -----------------------------
                    // Obtener Layer Inversor en CT por Region Inversor
                    // -----------------------------

                    foreach (var kvp in regionData)
                    {
                        // Obtener region inversor
                        Region invRegion = kvp.Key;

                        // Obtener inversores
                        List<DBObject> invList = kvp.Value;
                        // Validamos
                        if (invList == null || invList.Count != 1) continue;

                        // Obtener inversor
                        BlockReference invBr = invList.First() as BlockReference;
                        // Validamos
                        if (invBr == null) continue;

                        // Buscar info cable por Id
                        if (
                            cableByEntity.ContainsKey(invBr.ObjectId) &&
                            cableByEntity[invBr.ObjectId] is EntityExcelRow row
                        )
                        {
                            // Validamos
                            if (!string.IsNullOrWhiteSpace(row.CableLayer)
                            )
                            {
                                // Almacenamos
                                invInCtLayerByInvRegion[invRegion] = row.CableLayer;
                            }
                        }
                    }

                    bool showInfoByCt = false;
                    // Debug
                    if (showInfoByCt)
                    {
                        ShowInvInCtLayerByInvRegionDebug(invInCtLayerByInvRegion);
                    }
                }

                // -----------------------------
                // Almacenar regiones para borrado posterior
                // -----------------------------

                List<Region> allValidRegion = new List<Region>();
                allValidRegion.AddRange(validRegionCt);
                allValidRegion.AddRange(validRegionInv);

                // -----------------------------
                // Ordenar Regiones CT por centroide
                // -----------------------------

                validRegionCt.Sort((a, b) => cls_00_GetEntityCentroid.CompareEntitiesByPosition(a, b, 10.0));

                // -----------------------------
                // Definir comienzo en caso de analizar parcialmente el proyecto
                // -----------------------------

                int ctStartIndex;
                int trackStartIndex;
                // Si no analizamos todo el documento
                if (!analyzeAllDoc)
                {
                    Dictionary<string, string> fields = new Dictionary<string, string>
                    {
                        { solarSet.PolyCtTag, "1" },
                        { solarSet.BlockRefTrackTag, "1" }
                    };
                    // Form
                    Dictionary<string, int> result = cls_00_InstaForm_TextBox.TextBoxFormOut_NextToLabel_Integer(
                        "Enter the starting numbering values:", fields, formTitle: "Start Numbering Form"
                    );
                    // Validamos
                    if (result == null)
                    {
                        // Iteramos
                        foreach (Region region in allValidRegion)
                        {
                            // Validamos
                            if (region != null && !region.IsErased)
                            {
                                // Borramos region
                                cls_00_DeleteEntity.DeleteEntity(region);
                            }
                        }
                        // Finalizamos
                        return null;
                    }

                    // Asignamos
                    ctStartIndex = result[solarSet.PolyCtTag];
                    trackStartIndex = result[solarSet.BlockRefTrackTag];
                }
                // En caso de analizarlo completo
                else
                {
                    ctStartIndex = 1;
                    trackStartIndex = 1;
                }

                // -----------------------------
                // Procesar Regiones CT para ordenarlas
                // -----------------------------

                StringBuilder infoRegion = new StringBuilder();
                // Lista para almacenar todos los pares (CT, INV) detectados
                List<(int ctNumber, int invNumber, Region ctRegion, Region invRegion)> validRegionOrderList =
                    new List<(int, int, Region, Region)>();
                // Iteramos por los CT
                foreach (Region regionCt in validRegionCt)
                {
                    // -----------------------------
                    // Obtener Inversores contenidos en este CT
                    // -----------------------------

                    List<Entity> polyInvAsEntList = cls_00_GetEntityListByRegion.GetEntityListByRegionByPoint(
                        tr, regionCt, psrPolyInvIds
                    );
                    // Validamos
                    if (polyInvAsEntList == null || polyInvAsEntList.Count == 0) continue;

                    // -----------------------------
                    // Obtener regiones de Inversores
                    // -----------------------------

                    List<Region> regionInvList = cls_12_GetRegionsByInvOrder.GetRegionsByInvOrder(
                        polyInvAsEntList, dictPolyToRegionContInv, validRegionInv, infoRegion
                    );

                    // Iteramos regiones de los Inversores
                    foreach (Region regionInv in regionInvList)
                    {
                        // Obtener etiqueta del Inversor
                        List<Entity> labelInvAsEntList = cls_00_GetEntityListByRegion.GetEntityListByRegionByPoint(
                            tr, regionInv, psrLabelInvIds
                        );
                        // Validamos
                        if (labelInvAsEntList == null || labelInvAsEntList.Count != 1) continue;

                        // Obtenemos la etiqueta del inversor
                        Entity labelInvAsEnt = labelInvAsEntList.First();
                        // Validamos
                        string textValue = null;
                        // mtext
                        if (labelInvAsEnt is MText mtext)
                        {
                            textValue = mtext.Contents;
                        }
                        // dbtext
                        else if (labelInvAsEnt is DBText dbtext)
                        {
                            textValue = dbtext.TextString;
                        }
                        // Validamos
                        if (!string.IsNullOrWhiteSpace(textValue))
                        {
                            // Obtenemos el valor
                            string cleanedValue = textValue.Trim();
                            // Separadores válidos posibles
                            char[] validSeparators = autoSettings.ValidSeparators;
                            // Detectamos el separador 
                            char? sep = validSeparators.FirstOrDefault(s => cleanedValue.Contains(s));

                            int ctNum = 0;
                            int invNum = 0;
                            // Validamos
                            if (sep != null && sep != '\0')
                            {
                                // Dividimos por el separador detectado
                                string[] parts = cleanedValue.Split(new[] { sep.Value }, StringSplitOptions.None);
                                // Iteramos partes
                                foreach (string part in parts)
                                {
                                    // Obtener clave alfabetica
                                    string fieldKey = cls_00_MTextObjectsByLayer.GetAlphabeticFieldKey(part);
                                    // Validamos
                                    if (string.IsNullOrWhiteSpace(fieldKey)) continue;
                                    if (!fieldMapResult.ContainsKey(fieldKey)) continue;

                                    // Obtener tipo asignado
                                    string mappedType = fieldMapResult[fieldKey];

                                    // Obtener numero
                                    int number = 0;
                                    // Parseamos
                                    int.TryParse(
                                        new string(part.Where(char.IsDigit).ToArray()), out number
                                    );

                                    // CT
                                    if (mappedType == solarSet.PolyCtTag)
                                    {
                                        ctNum = number;
                                    }
                                    // Inversor
                                    else if (mappedType == solarSet.PolyInvTag)
                                    {
                                        invNum = number;
                                    }
                                }
                            }
                            // Almacenamos
                            validRegionOrderList.Add((ctNum, invNum, regionCt, regionInv));
                        }
                    }
                }

                bool showInfo = false;
                // Debug
                if (showInfo)
                {
                    ShowValidRegionOrderList(validRegionOrderList);
                }

                // -----------------------------
                // Ordenar Regiones por CT y por Inversor
                // -----------------------------

                var validRegionOrderListByCt = validRegionOrderList
                    .OrderBy(x => x.ctNumber).ThenBy(x => x.invNumber)
                    .GroupBy(x => x.ctNumber).ToList();

                // -----------------------------
                // Procesar Regiones CT 
                // -----------------------------

                HashSet<ObjectId> stringLabelIds = new HashSet<ObjectId>();
                // Contador global de etiquetas
                int totalLabelsCreated = 0;
                // Procesamos en el orden correcto
                foreach (var ctGroup in validRegionOrderListByCt)
                {
                    // -----------------------------
                    // Obtener la region del CT
                    // -----------------------------

                    Region regionCt = ctGroup.First().ctRegion;

                    // -----------------------------
                    // Lista de inversores ordenados dentro del CT
                    // -----------------------------

                    List<(int invNumber, Region invRegion)> invRegionsOrdered =
                        ctGroup.Select(x => (x.invNumber, x.invRegion)).ToList();

                    // -----------------------------
                    // Procesar CT
                    // -----------------------------

                    int labelsCreated = cls_12_ProcessCT.ProcessCtByInvLabel(
                        solarSet, regionCt, tr, btr, psrBlockRefTrackIds, labelFieldsDict, invInCtLayerByInvRegion, ctStartIndex,
                        trackStartIndex, isHorizontal, selectedTextStyle, selectedTextJust, infoRegion,
                        hasMPPT, separatorChar, invRegionsOrdered, inverterOutsideCt, ref stringLabelIds
                    );
                    // Actualizamos contador
                    ctStartIndex++;
                    // Acumulamos etiquetas
                    totalLabelsCreated += labelsCreated;
                }

                // Mostrar información
                ShowStringBuilder.ShowInfo(
                    $"📌 Entities by Document Summary:", infoRegion.ToString()
                );

                // -----------------------------
                // Actualizar etiquetas creadas 
                // -----------------------------

                // Definimos propiedad inversor
                string invProp = inverterOutsideCt
                    ? solarSet.ContInvProp
                    : solarSet.ComBoxProp;
                // Prefijo inversor
                string invPrefix = labelFieldsDict[invProp];

                // Iteramos 
                foreach (var ctGroup in validRegionOrderListByCt)
                {
                    // -----------------------------
                    // Obtener region del CT
                    // -----------------------------

                    Region regionCt = ctGroup.First().ctRegion;

                    // -----------------------------
                    // Acceder a sus Inversores/Combiner
                    // -----------------------------

                    List<(int invNumber, Region invRegion)> invRegionsOrdered =
                        ctGroup.Select(x => (x.invNumber, x.invRegion)).ToList();
                    // Iteramos Inversores
                    foreach (var (invNumber, invRegion) in invRegionsOrdered)
                    {
                        // Validamos
                        if (invRegion == null) continue;

                        // -----------------------------
                        // Obtener etiquetas por Inversor/Combiner
                        // -----------------------------

                        List<Entity> invLabelEntities = cls_00_GetEntityListByRegion.GetEntityListByRegionByPoint(
                            tr, invRegion, stringLabelIds
                        );
                        // Validamos
                        if (invLabelEntities == null || invLabelEntities.Count == 0) continue;

                        // Iteramos etiquetas
                        foreach (Entity ent in invLabelEntities)
                        {
                            // Validamos
                            if (!(ent is MText mText)) continue;

                            // -----------------------------
                            // Obtener campos 
                            // -----------------------------

                            string[] fields = cls_12_RemoveFieldFromLabel.SplitLabelFields(
                                mText.Contents, separatorChar
                            );

                            // -----------------------------
                            // Buscar campo Inversor/Combiner
                            // -----------------------------

                            int invFieldIndex = Array.FindIndex(
                                fields, f => f.Trim().StartsWith(invPrefix)
                            );
                            // Validamos
                            if (invFieldIndex < 0) continue;

                            // Obtenemos
                            string invField = fields[invFieldIndex];

                            // -----------------------------
                            // Reemplazar la incognita
                            // -----------------------------

                            if (invField.Contains("X"))
                            {
                                // Formateamos
                                string formattedInvNum = invNumber.ToString("D2");
                                invField = Regex.Replace(invField, "X+", formattedInvNum);
                                // Actualizamos
                                fields[invFieldIndex] = invField;
                                // Recomponemos la etiqueta
                                string newText = string.Join(separatorChar, fields);
                                // Actualizamos valor
                                cls_12_RemoveFieldFromLabel.UpdateMTextContents(
                                    mText, newText, infoRegion,
                                    "Label updated", "Error updating label"
                                );
                            }
                        }

                        // -----------------------------
                        // Agrupar etiquetas por Tracker
                        // -----------------------------

                        // Prefijo tracker
                        string trackerPrefix = labelFieldsDict[solarSet.TrackProp];

                        // Numeracion de strings global por inversor
                        int stringIndex = 1;
                        // Agrupamos por Tracker (antes limpiamos todo antes del espacio)
                        var groupedByTracker = invLabelEntities.OfType<MText>().GroupBy(m =>
                        {
                            // -----------------------------
                            // Obtener contenido
                            // -----------------------------

                            string cleanLabel = cls_12_RemoveFieldFromLabel.GetCleanLabel(m.Contents);

                            // -----------------------------
                            // Obtener campos 
                            // -----------------------------

                            string[] fields = cls_12_RemoveFieldFromLabel.SplitLabelFields(
                                cleanLabel, separatorChar
                            );

                            // -----------------------------
                            // Buscar campo Tracker
                            // -----------------------------

                            int trackerFieldIndex = Array.FindIndex(
                                fields, f => f.Trim().StartsWith(trackerPrefix)
                            );
                            // Validamos
                            if (trackerFieldIndex < 0) return string.Empty;

                            // Obtenemos
                            string trackerField = fields[trackerFieldIndex].Trim();

                            // return
                            return trackerField;
                        })
                        .ToList();

                        // -----------------------------
                        // Iterar etiquetas por Tracker
                        // -----------------------------

                        foreach (var group in groupedByTracker)
                        {
                            // -----------------------------
                            // Ordenar etiquetas por String
                            // -----------------------------

                            // Prefijo String
                            string stringPrefix = labelFieldsDict[solarSet.StringProp];
                            // Ordenamos
                            var orderedGroup = group
                                .OrderBy(m => m, Comparer<MText>.Create((m1, m2) =>
                                {
                                    return isHorizontal
                                        ? cls_00_GetEntityCentroid.CompareEntitiesByPositionHorizontal(m1, m2, 0.01)
                                        : cls_00_GetEntityCentroid.CompareEntitiesByPosition(m1, m2, 0.01);
                                }))
                                .ToList();
                            // Iteramos
                            foreach (MText mText in orderedGroup)
                            {
                                // -----------------------------
                                // Obtener contenido
                                // -----------------------------

                                string cleanLabel = cls_12_RemoveFieldFromLabel.GetCleanLabel(mText.Contents);

                                // -----------------------------
                                // Obtener campos 
                                // -----------------------------

                                string[] fields = cls_12_RemoveFieldFromLabel.SplitLabelFields(
                                    cleanLabel, separatorChar
                                );

                                // -----------------------------
                                // Buscar campo String
                                // -----------------------------

                                int stringFieldIndex = Array.FindIndex(
                                    fields,
                                    f =>
                                    {
                                        string cleanField = f.Trim();
                                        // return
                                        return Regex.IsMatch(
                                            cleanField,
                                            $"^{Regex.Escape(stringPrefix)}(X+|\\d+)$",
                                            RegexOptions.IgnoreCase
                                        );
                                    }
                                );
                                // Validamos
                                if (stringFieldIndex < 0) continue;

                                // Obtenemos
                                string stringField = fields[stringFieldIndex].Trim();

                                // -----------------------------
                                // Reemplazar la incognita
                                // -----------------------------

                                if (stringField.Contains("X"))
                                {
                                    // Formateamos
                                    string formattedStringNum = stringIndex.ToString("D2");
                                    stringField = Regex.Replace(stringField, "X+", formattedStringNum);
                                    // Actualizamos
                                    fields[stringFieldIndex] = stringField;
                                    // Reconstruir etiqueta completa con " +/-" otra vez
                                    string newText = string.Join(separatorChar, fields) + " +/-";
                                    // Actualizamos contador
                                    stringIndex++;
                                    // Actualizamos valor
                                    cls_12_RemoveFieldFromLabel.UpdateMTextContents(
                                        mText, newText, infoRegion,
                                        "String label updated", "Error updating string label"
                                    );
                                }
                            }
                        }
                    }
                }

                // -----------------------------
                // Eliminar campo Tracker de la etiqueta
                // -----------------------------

                if (!hasTrackerInfo)
                {
                    // Prefijo tracker
                    string trackerPrefix = labelFieldsDict[solarSet.TrackProp];
                    // Iteramos
                    foreach (ObjectId lblId in stringLabelIds)
                    {
                        // Actualizamos etiqueta
                        cls_12_RemoveFieldFromLabel.RemoveFieldFromLabel(
                            tr, lblId, separatorChar, infoRegion, trackerPrefix
                        );
                    }
                }

                // -----------------------------
                // Borrar regiones de CT
                // -----------------------------

                foreach (Region region in validRegionCt)
                {
                    // Validamos
                    if (region != null && !region.IsErased)
                    {
                        cls_00_DeleteEntity.DeleteEntity(region);
                    }
                }

                // -----------------------------
                // Borrar regiones de Inversores
                // -----------------------------

                foreach (Region region in validRegionInv)
                {
                    // Validamos
                    if (region != null && !region.IsErased)
                    {
                        cls_00_DeleteEntity.DeleteEntity(region);
                    }
                }

                // return
                return totalLabelsCreated;
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show($"ERROR in MainCreateEntityLabels:\n{ex.Message}\n{ex.StackTrace}");
                // Finalizamos
                return null;
            }

            // Por defecto
            return 0;
        }






    }
}
