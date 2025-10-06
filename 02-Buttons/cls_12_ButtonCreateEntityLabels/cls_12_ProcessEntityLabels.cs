using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SOLAR.EL.RibbonButton.Civil.Buttons;
using SOLAR.EL.RibbonButton.Revit.UserForms;
using static TYPSA.SharedLib.Civil.ProcessPoly.cls_00_ProcessOffsetPolyResult;
using static TYPSA.SharedLib.Civil.ProcessPoly.cls_00_ProcessPolyResult;
using static TYPSA.SharedLib.Civil.ProcessRegion.cls_00_ProcessRegionResult;
using TYPSA.SharedLib.Civil.DeleteEntities;
using TYPSA.SharedLib.Civil.DrawEntities;
using TYPSA.SharedLib.Civil.GetDocument;
using TYPSA.SharedLib.Civil.GetEntities;
using TYPSA.SharedLib.Civil.GetEntityCoordinates;
using TYPSA.SharedLib.Civil.GetLayersInfo;
using TYPSA.SharedLib.Civil.IsolateEntities;
using TYPSA.SharedLib.Civil.ProcessPoly;
using TYPSA.SharedLib.Civil.ProcessRegion;
using TYPSA.SharedLib.Civil.ProjectUnits;
using TYPSA.SharedLib.Civil.ShowInfoBox;
using TYPSA.SharedLib.Civil.UserForms;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_ProcessEntityLabels
    {
        public static int MainCreateEntityLabels(
            Editor ed, Database db, Transaction tr, BlockTableRecord btr,
            string contGenLayer, string contInvLayer, string trackLayer, string stringLayer, string labelsLayer,
            string contGenTag, string contInvTag, string trackTag,
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
                cls_00_CreateLayerIfNotExists.CreateLayerIfNotExists(labelsLayer);

                // Form
                string trackSel = InstanciarFormularios.DropDownFormListOut(
                    "Select the String configuration typology:",
                    new List<string> { tipTrack, tipEstFija },
                    "String Typology", tipTrack
                );
                // Validamos
                if (trackSel == null) return 0;

                string dolarConfigMess =
                    "True: Label configuration with MPPT ($).\n" +
                    "False: Label configuration without MPPT ($).";
                // Form
                object dolarSel = InstanciarFormularios.DropDownFormOut(
                    dolarConfigMess, false
                );
                // Validamos
                if (dolarSel == null) return 0;

                // Convertimos a boolean
                bool dolarSelBool = Convert.ToBoolean(dolarSel);

                // Obtenemos el listado de capas del documento
                List<string> docLayers = cls_00_GetLayerNamesFromActiveDoc.
                    GetLayerNamesFromActiveDocument();

                // Definimos orientacion label
                bool isHorizontal = (trackSel == tipEstFija);

                //bool boolDocbool = true;
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

                // Seleccionamos Contornos Genericos
                PromptSelectionResult psrContGen = cls_00_GetPolylinesByLayer.GetPolylinesByLayer(
                    ed, docLayers, contGenTag, contGenLayer
                );
                // Validamos
                if (psrContGen == null) return 0;

                // Seleccionamos contornos a analizar en funcion del bool
                SelectionSet analyzePoly = cls_00_GetPolylinesByUser.GetPolylinesByUser(
                    ed, boolDocbool, psrContGen, contGenTag
                );
                // Validamos
                if (analyzePoly == null) return 0;

                double elevGen;
                // Validamos elevation
                if (!cls_12_GetEntityElev.AllEntHaveSameElev(
                    tr, psrContGen, contGenTag, out elevGen
                )) return 0;

                // Seleccionamos Contornos Inversores
                PromptSelectionResult psrContInv = cls_00_GetPolylinesByLayer.GetPolylinesByLayer(
                    ed, docLayers, contInvTag, contInvLayer
                );
                // Validamos
                if (psrContInv == null) return 0;

                double elevInv;
                // Validamos elevation
                if (!cls_12_GetEntityElev.AllEntHaveSameElev(
                    tr, psrContInv, contInvTag, out elevInv
                )) return 0;

                // Seleccionamos Trackers
                PromptSelectionResult psrTrackers = cls_00_GetBlockRefByLayer.GetBlockRefByLayer(
                    docLayers, ed, trackTag, trackLayer
                );
                // Validamos
                if (psrTrackers == null) return 0;

                double elevTrack;
                // Validamos elevation
                if (!cls_12_GetEntityElev.AllEntHaveSameElev(
                    tr, psrTrackers, trackTag, out elevTrack
                )) return 0;

                // Validamos elevaciones entre Contornos Generales, Inversores y Trackers
                if (Math.Abs(elevGen - elevInv) > 1e-6 || Math.Abs(elevGen - elevTrack) > 1e-6)
                {
                    // Mensaje
                    MessageBox.Show(
                        $"⚠ Elevations are inconsistent across entities.\n\n" +
                        $"{contGenTag} Z: {elevGen:F3}\n" +
                        $"{contInvTag} Z: {elevInv:F3}\n" +
                        $"{trackTag} Z: {elevTrack:F3}",
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

                double offsetDistance = 1;
                // Obtenemos info de los Contornos Generales desfasados
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

                // Form prefijos Label
                Dictionary<string, string> propPreDict =
                    InstanciarFormulariosSOLAR.TextBoxFormOut_Solar(
                        "Enter a prefix for each entity contained in the label"
                    );
                // Validamos
                if (propPreDict == null) return 0;

                List<string> availableTextStyles =
                    cls_00_DocumentInfo.GetAllTextStylesFromDrawing(db);
                // Form para definir text style
                string chosenStyle = cls_00_DrawEntities.AskTextStyleFromUser(
                    availableTextStyles, solarSet.LabelStyle
                );
                // Validamos
                if (chosenStyle == null) return 0;

                // Form para definir text justification
                AttachmentPoint chosenJustification =
                    cls_00_DrawEntities.AskMTextJustificationFromUser(AttachmentPoint.TopLeft);

                StringBuilder infoRegiones = new StringBuilder();
                // Diccionario para almacenar los elementos por region
                Dictionary<Region, List<DBObject>> regionDataContGen =
                    new Dictionary<Region, List<DBObject>>();

                // Ordenar lista de regiones por centroide
                validRegionContGen.Sort((a, b) =>
                    cls_00_GetEntityCentroid.CompareEntitiesByPosition(a, b, 10.0));

                // Contador de Contornos Generales
                int contGenIndex;
                // Si no analizamos todo el documento
                if (!boolDocbool)
                {
                    string msg = $"Enter the starting number for {contGenTag}:";
                    string input = InstanciarFormularios.TextBoxFormOut(msg, "1");
                    // Validamos
                    if (string.IsNullOrWhiteSpace(input)) return 0;
                    // Validamos
                    if (!int.TryParse(input, out contGenIndex))
                    {
                        // Mensaje
                        MessageBox.Show(
                            "Invalid number entered. Operation will be canceled.",
                            "Input Error"
                        );
                        // Finalizamos
                        return 0;
                    }
                }
                // En caso de analizarlo
                else
                {
                    contGenIndex = 1;
                }

                // Contador global de etiquetas
                int totalLabelsCreated = 0;
                // Iteramos por los Contornos Generales
                foreach (Region regionContGen in validRegionContGen)
                {
                    // Procesamos CCentro Transformacion
                    int labelsCreated = cls_12_ProcessCentroTrans.ProcessCentroTrans(
                        regionContGen, tr, btr, contInvIds,
                        dictPolyToRegionContInv, validRegionContInv,
                        trackersIds, trackTag, stringLayer,
                        propPreDict, contGenProp, contGenIndex,
                        contInvProp, trackProp, stringProp,
                        isHorizontal, labelsLayer,
                        chosenStyle, chosenJustification, infoRegiones,
                        dolarSelBool
                    );
                    // Actualizamos contador
                    contGenIndex++;
                    // Acumulamos etiquetas
                    totalLabelsCreated += labelsCreated;
                }

                // Mostrar información
                cls_00_ShowInfoBox.ShowStringBuilder(
                    $"📌 Entities by Document Summary:", infoRegiones.ToString()
                );

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
