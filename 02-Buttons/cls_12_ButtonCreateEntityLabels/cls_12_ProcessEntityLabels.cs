using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using static TYPSA.SharedLib.Autocad.ProcessPoly.cls_00_ProcessOffsetPolyResult;
using static TYPSA.SharedLib.Autocad.ProcessPoly.cls_00_ProcessPolyResult;
using static TYPSA.SharedLib.Autocad.ProcessRegion.cls_00_ProcessRegionResult;
using TYPSA.SharedLib.Autocad.DeleteEntities;
using TYPSA.SharedLib.Autocad.DrawEntities;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetEntityCoordinates;
using TYPSA.SharedLib.Autocad.GetLayersInfo;
using TYPSA.SharedLib.Autocad.IsolateEntities;
using TYPSA.SharedLib.Autocad.ProcessPoly;
using TYPSA.SharedLib.Autocad.ProcessRegion;
using TYPSA.SharedLib.Autocad.ProjectUnits;
using TYPSA.SharedLib.Autocad.ShowInfoBox;
using TYPSA.SharedLib.Autocad.UserForms;
using TYPSA.SharedLib.Autocad.EntitiesInsertionPoint;
using SOLAR.EL.RibbonButton.Autocad.UserForms;
using System.Linq;
using System.Text.RegularExpressions;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_ProcessEntityLabels
    {
        public static int ProcessEntityLabels(
            Editor ed, Database db, Transaction tr, BlockTableRecord btr,
            string contGenLayer, string contInvLayer, string trackLayer, string stringLayer, string labelsTrackLayer, string labelsInvLayer,
            string contGenTag, string contInvTag, string trackTag, string contInvLabelTag,
            string contGenProp, string contInvProp, string trackProp, string stringProp,
            SolarSettings solarSet, string tipTrack, string tipEstFija
        )
        {
            // try
            try
            {
                // Obtenemos las unidades del proyecto (actuales o elegidas por user)
                string projectUnits = cls_00_ProjectUnits.GetAndSetProjectUnits();
                // Validamos
                if (string.IsNullOrEmpty(projectUnits)) return 0;

                // Creamos las capas si no existen
                cls_00_CreateLayerIfNotExists.CreateLayerIfNotExists(contGenLayer);
                cls_00_CreateLayerIfNotExists.CreateLayerIfNotExists(contInvLayer);
                cls_00_CreateLayerIfNotExists.CreateLayerIfNotExists(trackLayer);
                cls_00_CreateLayerIfNotExists.CreateLayerIfNotExists(stringLayer);
                cls_00_CreateLayerIfNotExists.CreateLayerIfNotExists(labelsTrackLayer);
                cls_00_CreateLayerIfNotExists.CreateLayerIfNotExists(labelsInvLayer);

                // Form
                string trackSel = InstanciarFormularios.DropDownFormListOut(
                    "Select the String configuration typology:",
                    new List<string> { tipTrack, tipEstFija },
                    "String Typology", tipTrack
                );
                // Validamos
                if (trackSel == null) return 0;
                // Definimos orientacion label
                bool isHorizontal = (trackSel == tipEstFija);

                string MPPtConfigMess =
                    "True: Label configuration with MPPT ($).\n" +
                    "False: Label configuration without MPPT ($).";
                // Form
                object MPPtSel = InstanciarFormularios.DropDownFormOut(
                    MPPtConfigMess, false
                );
                // Validamos
                if (MPPtSel == null) return 0;
                // Convertimos a boolean
                bool MPPtSelBool = Convert.ToBoolean(MPPtSel);

                string trackLabelConfigMess =
                    "True: Label configuration with Tracker info.\n" +
                    "False: Label configuration without Tracker info.";
                // Form
                object trackLabelSel = InstanciarFormularios.DropDownFormOut(
                    trackLabelConfigMess, true
                );
                // Validamos
                if (trackLabelSel == null) return 0;
                // Convertimos a boolean
                bool trackLabelSelBool = Convert.ToBoolean(trackLabelSel);

                // Form
                string charSepSel = InstanciarFormularios.DropDownFormListOut(
                    "Select the separator character for the label:",
                    new List<string> { ".", "-", "_", ",", ";" },
                    "Label Separator Selection", "-"
                );
                // Validamos la selección
                if (string.IsNullOrEmpty(charSepSel)) return 0;

                // Obtenemos el listado de capas del documento
                List<string> docLayers = cls_00_GetLayerNamesFromActiveDoc.
                    GetLayerNamesFromActiveDocument();

                string boolDocMess =
                    $"True: Analyze all {contGenTag} in the document.\n" +
                    $"False: Manually select {contGenTag} to analyze.";
                // Form
                object boolDoc = InstanciarFormularios.DropDownFormOut(
                    boolDocMess, true
                );
                // Validamos
                if (boolDoc == null) return 0;
                // Convertimos a boolean
                bool boolDocbool = Convert.ToBoolean(boolDoc);

                // Seleccionamos Centros Transformacion
                PromptSelectionResult psrContGen = cls_00_GetPolylinesByLayer.GetPolylinesByLayer(
                    ed, docLayers, contGenTag, contGenLayer
                );
                // Validamos
                if (psrContGen == null) return 0;
                // Seleccionamos en funcion del bool
                SelectionSet analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUser(
                    ed, boolDocbool, psrContGen, contGenTag
                );
                // Validamos
                if (analyzePoly == null) return 0;
                // Validamos elevacion
                double elevCenTrans;
                if (!cls_12_GetEntityElev.AllEntHaveSameElev(
                    tr, psrContGen, contGenTag, out elevCenTrans
                )) return 0;

                // Seleccionamos Inversores
                PromptSelectionResult psrContInv = cls_00_GetPolylinesByLayer.GetPolylinesByLayer(
                    ed, docLayers, contInvTag, contInvLayer
                );
                // Validamos
                if (psrContInv == null) return 0;
                // Validamos elevacion
                double elevInv;
                if (!cls_12_GetEntityElev.AllEntHaveSameElev(
                    tr, psrContInv, contInvTag, out elevInv
                )) return 0;

                // Seleccionamos Trackers
                PromptSelectionResult psrTrackers = cls_00_GetBlockRefByLayer.GetBlockRefByLayer(
                    docLayers, ed, trackTag, trackLayer
                );
                // Validamos
                if (psrTrackers == null) return 0;
                // Validamos elevacion
                double elevTrack;
                if (!cls_12_GetEntityElev.AllEntHaveSameElev(
                    tr, psrTrackers, trackTag, out elevTrack
                )) return 0;

                // Seleccionamos etiquetas Inversores
                PromptSelectionResult psrInvLabels = cls_00_GetMTextByLayer.GetMTextByLayer(
                    ed, docLayers, contInvLabelTag, labelsInvLayer
                );
                // Validamos
                if (psrInvLabels == null) return 0;
                // Validamos elevacion
                double elevInvLabel;
                if (!cls_12_GetEntityElev.AllEntHaveSameElev(
                    tr, psrInvLabels, contInvLabelTag, out elevInvLabel
                )) return 0;

                // Validamos elevaciones entre Centros, Inversores, Trackers y Etiquetas Inv
                if (Math.Abs(elevCenTrans - elevInv) > 1e-6 ||
                    Math.Abs(elevCenTrans - elevTrack) > 1e-6 ||
                    Math.Abs(elevCenTrans - elevInvLabel) > 1e-6)
                {
                    // Mensaje
                    MessageBox.Show(
                        $"⚠ Elevations are inconsistent across entities.\n\n" +
                        $"{contGenTag} Z: {elevCenTrans:F3}\n" +
                        $"{contInvTag} Z: {elevInv:F3}\n" +
                        $"{trackTag} Z: {elevTrack:F3}\n" +
                        $"{contInvLabelTag} Z: {elevInvLabel:F3}",
                        "Elevation Mismatch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return 0;
                }

                HashSet<ObjectId> trackersToIsolate = new HashSet<ObjectId>();
                HashSet<ObjectId> trackersWithMultipleEtracker = new HashSet<ObjectId>();
                // Iteramos
                foreach (ObjectId trkId in psrTrackers.Value.GetObjectIds())
                {
                    // Validamos Trackers/Strings
                    cls_12_ValidateTracker.ValidateTracker(
                        tr, trkId, trackLayer, stringLayer,
                        trackersWithMultipleEtracker, trackersToIsolate
                    );
                }

                // Validamos
                // Si hay trackers con varias polys en trackLayer
                if (trackersWithMultipleEtracker.Count > 0)
                {
                    // Mensaje
                    MessageBox.Show(
                        $"{trackersWithMultipleEtracker.Count} {trackTag} were " +
                        $"discarded because they contain more than one " +
                        $"polyline in layer {trackLayer}. They will be isolated in AutoCAD.",
                        "Invalid Trackers - Multiple Polylines"
                    );
                    // Aislamos
                    cls_00_IsolateEntities.IsolateObjects(ed, trackersWithMultipleEtracker);
                    // Finalizamos
                    return 0;
                }
                // Si hay trackers sin polys mínimas
                if (trackersToIsolate.Count > 0)
                {
                    // Mensaje
                    MessageBox.Show(
                        $"{trackersToIsolate.Count} {trackTag} were " +
                        $"discarded because they do not contain both " +
                        $"a polyline in {trackLayer} and a polyline in {stringLayer}. " +
                        $"They will be isolated in AutoCAD.",
                        "Invalid Trackers"
                    );
                    // Aislamos
                    cls_00_IsolateEntities.IsolateObjects(ed, trackersToIsolate);
                    // Finalizamos
                    return 0;
                }

                // Obtenemos los Ids
                HashSet<ObjectId> contInvIds = new HashSet<ObjectId>(psrContInv.Value.GetObjectIds());
                HashSet<ObjectId> trackersIds = new HashSet<ObjectId>(psrTrackers.Value.GetObjectIds());
                HashSet<ObjectId> invLabelIds = new HashSet<ObjectId>(psrInvLabels.Value.GetObjectIds());

                // Validamos los Contornos Generales
                ProcessPolyResult dataContGen = cls_00_ProcessAllPoly.ProcessAllPoly(
                    analyzePoly, tr, contGenTag, projectUnits
                );

                // Acceso a las propiedades
                List<Polyline> validPolyContGen = dataContGen.ValidPolylines;
                HashSet<ObjectId> polyToIsolateContGen = dataContGen.PolylinesToIsolate;
                // Validamos
                if (polyToIsolateContGen.Count > 0)
                {
                    // Mensaje
                    MessageBox.Show(
                        $"{polyToIsolateContGen.Count} {contGenTag} were discarded " +
                        $"and will be isolated in AutoCAD.",
                        $"Isolated Contornos Generales"
                    );
                    // Aislamos los objetos
                    cls_00_IsolateEntities.IsolateObjects(ed, polyToIsolateContGen);
                    // Finalizamos
                    return 0;
                }

                // Validamos los Contornos Inversores
                ProcessPolyResult dataContInv = cls_00_ProcessAllPoly.ProcessAllPoly(
                    psrContInv.Value, tr, contInvTag, projectUnits
                );

                // Acceso a las propiedades
                List<Polyline> validPolyContInv = dataContInv.ValidPolylines;
                HashSet<ObjectId> polyToIsolateContInv = dataContInv.PolylinesToIsolate;
                // Validamos
                if (polyToIsolateContInv.Count > 0)
                {
                    // Mensaje
                    MessageBox.Show(
                        $"{polyToIsolateContInv.Count} {contInvTag} were discarded " +
                        $"and will be isolated in AutoCAD.",
                        $"Isolated Contornos Inversores"
                    );
                    // Aislamos los objetos
                    cls_00_IsolateEntities.IsolateObjects(ed, polyToIsolateContInv);
                    // Finalizamos
                    return 0;
                }

                double offsetDistance = 0.15;
                // Obtenemos info de los Centros de Transformacion desfasados
                ProcessOffsetPolyResult dataOffsetPolyContGen =
                    cls_00_ProcessAllOffsetPoly.ProcessAllOffsetPoly(
                        validPolyContGen, tr, btr, contGenTag, offsetDistance
                    );

                // Acceso a las propiedades
                List<Polyline> validOffsetPolyContGen =
                    dataOffsetPolyContGen.ValidOffsetPolylines;
                List<Polyline> validOffsetPolyAndPolyContGen =
                    dataOffsetPolyContGen.ValidOffsetAndOriginalPolys;
                HashSet<ObjectId> offsetPolyToIsolateContGen =
                    dataOffsetPolyContGen.OffsetPolylinesToIsolate;
                Dictionary<Handle, Handle> dictPolyToOffsetPolyContGen =
                    dataOffsetPolyContGen.DictPolyToOffset;
                // Validamos
                if (offsetPolyToIsolateContGen.Count > 0)
                {
                    // Borramos las polilíneas válidas desfasadas para evitar duplicados
                    foreach (Polyline poly in validOffsetPolyAndPolyContGen)
                    {
                        // Validamos
                        if (poly == null) continue;
                        // Borrar la polilínea
                        cls_00_DeleteEntity.DeleteEntity(poly);
                    }
                    // Aislamos las que fallaron
                    cls_00_IsolateEntities.IsolateObjects(ed, offsetPolyToIsolateContGen);
                    // Finalizamos
                    return 0;
                }

                // Obtenemos info de los Contornos de Inversores desfasados
                ProcessOffsetPolyResult dataOffsetPolyContInv =
                    cls_00_ProcessAllOffsetPoly.ProcessAllOffsetPoly(
                        validPolyContInv, tr, btr, contInvTag, offsetDistance
                    );

                // Acceso a las propiedades
                List<Polyline> validOffsetPolyContInv =
                    dataOffsetPolyContInv.ValidOffsetPolylines;
                List<Polyline> validOffsetPolyAndPolyContInv =
                    dataOffsetPolyContInv.ValidOffsetAndOriginalPolys;
                HashSet<ObjectId> offsetPolyToIsolateContInv =
                    dataOffsetPolyContInv.OffsetPolylinesToIsolate;
                Dictionary<Handle, Handle> dictPolyToOffsetPolyContInv =
                    dataOffsetPolyContInv.DictPolyToOffset;
                // Validamos
                if (offsetPolyToIsolateContInv.Count > 0)
                {
                    // Borramos las polilíneas válidas desfasadas para evitar duplicados
                    foreach (Polyline poly in validOffsetPolyAndPolyContInv)
                    {
                        // Validamos
                        if (poly == null) continue;
                        // Borrar la polilínea
                        cls_00_DeleteEntity.DeleteEntity(poly);
                    }
                    // Aislamos las que fallaron
                    cls_00_IsolateEntities.IsolateObjects(ed, offsetPolyToIsolateContInv);
                    // Finalizamos
                    return 0;
                }

                // Obtenemos la info de las regiones procesadas (ContGen)
                ProcessRegionResult dataRegionsContGen = cls_00_ProcessAllRegion.ProcessAllRegion(
                    validOffsetPolyAndPolyContGen, validOffsetPolyContGen, dictPolyToOffsetPolyContGen,
                    tr, btr, contGenTag
                );

                // Acceder a los valores
                List<Region> validRegionContGen =
                    dataRegionsContGen.ValidRegions;
                HashSet<ObjectId> offsetPolyToRegionToIsolateContGen =
                    dataRegionsContGen.FailedRegionPolylines;
                // Validamos
                if (offsetPolyToRegionToIsolateContGen.Count > 0)
                {
                    // Borrar las regiones válidas antes de aislar
                    foreach (Region region in validRegionContGen)
                    {
                        if (region != null)
                        {
                            cls_00_DeleteEntity.DeleteEntity(region);
                        }
                    }
                    // Aislar las polilíneas que fallaron
                    cls_00_IsolateEntities.IsolateObjects(ed, offsetPolyToRegionToIsolateContGen);
                    // Finalizamos
                    return 0;
                }

                // Obtenemos la info de las regiones procesadas (ContInv)
                ProcessRegionResult dataRegionsContInv = cls_00_ProcessAllRegion.ProcessAllRegion(
                    validOffsetPolyAndPolyContInv, validOffsetPolyContInv, dictPolyToOffsetPolyContInv,
                    tr, btr, contInvTag
                );

                // Acceder a los valores
                List<Region> validRegionContInv =
                    dataRegionsContInv.ValidRegions;
                HashSet<ObjectId> offsetPolyToRegionToIsolateContInv =
                    dataRegionsContInv.FailedRegionPolylines;
                Dictionary<Handle, Handle> dictPolyToRegionContInv =
                    dataRegionsContInv.PolyToRegionMap;
                // Validamos
                if (offsetPolyToRegionToIsolateContInv.Count > 0)
                {
                    // Borrar las regiones válidas antes de aislar
                    foreach (Region region in validRegionContInv)
                    {
                        if (region != null)
                        {
                            cls_00_DeleteEntity.DeleteEntity(region);
                        }
                    }
                    // Aislar las polilíneas que fallaron
                    cls_00_IsolateEntities.IsolateObjects(ed, offsetPolyToRegionToIsolateContInv);
                    // Finalizamos
                    return 0;
                }

                // Almacenamos regiones para borrarlas antes de un return
                List<Region> allTempRegions = new List<Region>();
                allTempRegions.AddRange(validRegionContGen);
                allTempRegions.AddRange(validRegionContInv);

                // Form prefijos Label
                Dictionary<string, string> propPreDict =
                    InstanciarFormulariosSOLAR.TextBoxFormOut_Solar(
                        "Enter a prefix for each entity contained in the label"
                    );
                // Validamos
                if (propPreDict == null)
                {
                    // Iteramos
                    foreach (Region region in allTempRegions)
                    {
                        // Validamos
                        if (region != null && !region.IsErased)
                            // Borramos region
                            cls_00_DeleteEntity.DeleteEntity(region);
                    }
                    // Finalizamos
                    return 0;
                }
                
                List<string> availableTextStyles =
                    cls_00_DocumentInfo.GetAllTextStylesFromDrawing(db);
                // Form para definir text style
                string chosenStyle = cls_00_DrawEntities.AskTextStyleFromUser(
                    availableTextStyles, solarSet.LabelStyle
                );
                // Validamos
                if (chosenStyle == null)
                {
                    // Iteramos
                    foreach (Region region in allTempRegions)
                    {
                        // Validamos
                        if (region != null && !region.IsErased)
                            // Borramos region
                            cls_00_DeleteEntity.DeleteEntity(region);
                    }
                    // Finalizamos
                    return 0;
                }

                AttachmentPoint chosenJustification;
                // Form para definir text justification
                if (isHorizontal)
                {
                    // BottomLeft
                    chosenJustification = cls_00_DrawEntities
                        .AskMTextJustificationFromUser(AttachmentPoint.BottomLeft);
                }
                else
                {
                    // TopLeft
                    chosenJustification = cls_00_DrawEntities
                        .AskMTextJustificationFromUser(AttachmentPoint.TopLeft);
                }

                StringBuilder infoRegiones = new StringBuilder();
                // Diccionario para almacenar los elementos por region
                Dictionary<Region, List<DBObject>> regionDataContGen =
                    new Dictionary<Region, List<DBObject>>();

                // Ordenar lista de regiones por centroide
                validRegionContGen.Sort((a, b) =>
                    cls_00_GetEntityCentroid.CompareEntitiesByPosition(a, b, 10.0));

                // Contador de CT
                int ctStartIndex;
                int trackStartIndex;
                // Si no analizamos todo el documento
                if (!boolDocbool)
                {
                    // CT
                    string msgCT = $"Enter the starting number for {contGenTag}:";
                    string inputCT = InstanciarFormularios.TextBoxFormOut(msgCT, "1");
                    if (string.IsNullOrWhiteSpace(inputCT) || !int.TryParse(inputCT, out ctStartIndex))
                    {
                        MessageBox.Show("Invalid CT number. Operation will be canceled.", "Input Error");
                        foreach (Region region in allTempRegions)
                            if (region != null && !region.IsErased)
                                cls_00_DeleteEntity.DeleteEntity(region);
                        return 0;
                    }
                    // Tracker
                    string msgTrack = "Enter the starting number for Tracker:";
                    string inputTrack = InstanciarFormularios.TextBoxFormOut(msgTrack, "1");
                    if (string.IsNullOrWhiteSpace(inputTrack) || !int.TryParse(inputTrack, out trackStartIndex))
                    {
                        MessageBox.Show("Invalid Tracker number. Operation will be canceled.", "Input Error");
                        foreach (Region region in allTempRegions)
                            if (region != null && !region.IsErased)
                                cls_00_DeleteEntity.DeleteEntity(region);
                        return 0;
                    }
                }
                // En caso de analizarlo
                else
                {
                    ctStartIndex = 1;
                    trackStartIndex = 1;
                }

                // Lista para almacenar todos los pares (CT, INV) detectados
                List<(int ctNumber, int invNumber, Region ctRegion, Region invRegion)> regionOrderList =
                    new List<(int, int, Region, Region)>();
                // Iteramos por los CT
                foreach (Region regionCT in validRegionContGen)
                {
                    // Obtenemos los Inversores contenidos en este CT
                    List<Entity> contInvEntities = cls_12_GetInvInRegGen.GetInvInRegGen(
                        tr, regionCT, contInvIds, infoRegiones
                    );
                    // Validamos
                    if (contInvEntities == null || contInvEntities.Count == 0) continue;

                    // Obtenemos regiones de inversores
                    List<Region> invRegions = cls_12_GetInvRegions.GetInvRegions(
                        contInvEntities, dictPolyToRegionContInv, validRegionContInv, infoRegiones
                    );

                    // Iteramos por las regiones de los Inversores
                    foreach (Region invRegion in invRegions)
                    {
                        // Obtener etiqueta del Inversor
                        List<Entity> invLabelEntities = cls_12_GetTrackInRegInv.
                            GetEntInRegByPoint(tr, invRegion, invLabelIds);
                        // Validamos
                        if (invLabelEntities == null || invLabelEntities.Count != 1) continue;
                        // Obtenemos la etiqueta del inversor
                        Entity invLabelEnt = invLabelEntities.First();
                        // Validamos
                        if (invLabelEnt is MText mtext)
                        {
                            // Obtenemos el valor
                            string text = mtext.Contents.Trim();
                            // Separadores válidos posibles
                            char[] validSeparators = { '.', '-', '_', ',', ';' };
                            // Detectamos el separador 
                            char? sep = validSeparators.FirstOrDefault(s => text.Contains(s));

                            int ctNum = 0;
                            int invNum = 0;
                            // Validamos
                            if (sep != null && sep != '\0')
                            {
                                // Dividimos por el separador detectado
                                string[] parts = text.Split(new[] { sep.Value }, StringSplitOptions.None);
                                // Validamos
                                if (parts.Length == 2)
                                {
                                    // Extraemos campos numericos
                                    int.TryParse(new string(parts[0].Where(char.IsDigit).ToArray()), out ctNum);
                                    int.TryParse(new string(parts[1].Where(char.IsDigit).ToArray()), out invNum);
                                }
                            }
                            // Almacenamos
                            regionOrderList.Add((ctNum, invNum, regionCT, invRegion));
                        }
                    }
                }

                // Ordenar por CT y por Inversor
                var orderedRegions = regionOrderList
                    .OrderBy(x => x.ctNumber)
                    .ThenBy(x => x.invNumber)
                    .GroupBy(x => x.ctNumber)
                    .ToList();

                HashSet<ObjectId> createdLabelIds = new HashSet<ObjectId>();
                // Contador global de etiquetas
                int totalLabelsCreated = 0;
                // Procesamos en el orden correcto
                foreach (var ctGroup in orderedRegions)
                {
                    // Obtenemos la region del CT
                    Region regionCT = ctGroup.First().ctRegion;
                    // Lista de inversores ordenados dentro de este CT
                    List<(int invNumber, Region invRegion)> invRegionsOrdered =
                        ctGroup.Select(x => (x.invNumber, x.invRegion)).ToList();
                    // Procesamos CT
                    int labelsCreated = cls_12_ProcessCentroTrans.ProcessCentroTransByInvLabel(
                        regionCT, tr, btr,
                        trackersIds, stringLayer,
                        propPreDict, contGenProp, ctStartIndex, trackStartIndex,
                        contInvProp, trackProp, stringProp,
                        isHorizontal, labelsTrackLayer,
                        chosenStyle, chosenJustification, infoRegiones,
                        MPPtSelBool, charSepSel,
                        invRegionsOrdered, ref createdLabelIds
                    );
                    // Actualizamos contador
                    ctStartIndex++;
                    // Acumulamos etiquetas
                    totalLabelsCreated += labelsCreated;
                }

                // Mostrar información
                cls_00_ShowInfoBox.ShowStringBuilder(
                    $"📌 Entities by Document Summary:", infoRegiones.ToString()
                );

                // Actualizamos etiquetas creadas por Inversor
                foreach (var ctGroup in orderedRegions)
                {
                    // Obtenemos la region del CT
                    Region regionCT = ctGroup.First().ctRegion;
                    // Accedemos a sus Inversores
                    List<(int invNumber, Region invRegion)> invRegionsOrdered =
                        ctGroup.Select(x => (x.invNumber, x.invRegion)).ToList();
                    // Iteramos Inversores
                    foreach (var (invNumber, invRegion) in invRegionsOrdered)
                    {
                        // Validamos
                        if (invRegion == null) continue;

                        // Obtenemos etiquetas por Inversor
                        List<Entity> invLabelEntities = cls_12_GetTrackInRegInv.
                            GetEntInRegByPoint(tr, invRegion, createdLabelIds);
                        // Validamos
                        if (invLabelEntities == null || invLabelEntities.Count == 0) continue;

                        // Iteramos etiquetas
                        foreach (Entity ent in invLabelEntities)
                        {
                            // Validamos
                            if (!(ent is MText mText)) continue;

                            // Obtenemos los campos
                            string[] fields = mText.Contents.Split(new[] { charSepSel }, StringSplitOptions.None);
                            // Accedemos al campo del Inversor (segundo)
                            string secondField = fields[1];
                            // Validamos
                            if (secondField.Contains("X"))
                            {
                                // Formateamos
                                string formattedInvNum = invNumber.ToString("D2");
                                secondField = Regex.Replace(secondField, "X+", formattedInvNum);
                                fields[1] = secondField;

                                // Recomponemos el texto
                                string newText = string.Join(charSepSel, fields);

                                // Actualizamos valor
                                cls_12_RemoveTrackFieldFromLabel.UpdateMTextContents(
                                    mText, newText, infoRegiones,
                                    "Label updated",
                                    "Error updating label"
                                );
                            }
                        }

                        // Numeracion de strings global por inversor
                        int stringIndex = 1;
                        // Agrupamos por Tracker (primero limpiamos todo antes del espacio)
                        var groupedByTracker = invLabelEntities
                            .OfType<MText>()
                            .GroupBy(m =>
                            {
                                // Obtenemos contenido
                                string fullText = m.Contents ?? string.Empty;
                                // Split por el espacio y cogemos primera parte
                                int spaceIndex = fullText.IndexOf(' ');
                                string cleanLabel = spaceIndex > -1
                                    ? fullText.Substring(0, spaceIndex)
                                    : fullText;

                                // Obtenemos los campos
                                string[] fields = cleanLabel.Split(new[] { charSepSel }, StringSplitOptions.None);

                                // Obtenemos el campo del Tracker (penultimo)
                                string trackerField = fields[fields.Length - 2].Trim();
                                // return
                                return trackerField;
                            })
                            .ToList();
                        // Iteramos
                        foreach (var group in groupedByTracker)
                        {
                            // Ordenamos por String
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
                                // Obtenemos contenido
                                string fullText = mText.Contents ?? string.Empty;
                                // Split por el espacio y cogemos primera parte
                                int spaceIndex = fullText.IndexOf(' ');
                                string cleanLabel = spaceIndex > -1
                                    ? fullText.Substring(0, spaceIndex)
                                    : fullText;

                                // Obtenemos los campos
                                string[] fields = cleanLabel.Split(new[] { charSepSel }, StringSplitOptions.None);

                                // Nos quedamos con el campo del String (ultimo)
                                int lastIndex = fields.Length - 1;
                                string lastField = fields[lastIndex].Trim();

                                // Validamos que tenga X
                                if (lastField.Contains("X"))
                                {
                                    // Formateamos
                                    string formattedStringNum = stringIndex.ToString("D2");
                                    lastField = Regex.Replace(lastField, "X+", formattedStringNum);

                                    // Reconstruir etiqueta completa con " +/-" otra vez
                                    fields[lastIndex] = lastField;
                                    string newText = string.Join(charSepSel, fields) + " +/-";

                                    // Actualizamos contador
                                    stringIndex++;

                                    // Actualizamos valor
                                    cls_12_RemoveTrackFieldFromLabel.UpdateMTextContents(
                                        mText, newText, infoRegiones,
                                        "String label updated",
                                        "Error updating string label"
                                    );
                                }
                            }
                        }
                    }
                }

                // Si el usuario no quiere mostrar el campo Tracker en la etiqueta
                if (!trackLabelSelBool)
                {
                    // Iteramos
                    foreach (ObjectId lblId in createdLabelIds)
                    {
                        // Actualizamos etiqueta
                        cls_12_RemoveTrackFieldFromLabel.RemoveTrackerFieldFromLabel(
                            tr, lblId, charSepSel, infoRegiones
                        );
                    }
                }

                // Borrar regiones de contornos generales
                foreach (Region region in validRegionContGen)
                {
                    // Validamos
                    if (region != null && !region.IsErased)
                    {
                        cls_00_DeleteEntity.DeleteEntity(region);
                    }
                }

                // Borrar regiones de contornos de inversores
                foreach (Region region in validRegionContInv)
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
                return 0;
            }
        }






    }
}
