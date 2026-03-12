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
        public static int? ProcessCreateEntityLabels(
            Editor ed, 
            Database db, 
            Transaction tr, 
            BlockTableRecord btr
        )
        {
            // try
            try
            {
                // Obtenemos settings
                SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();
                AutocadSettings autoSettings = AutocadSettings.GetDefaultSettings();

                // Obtenemos las unidades del proyecto (actuales o elegidas por user)
                string projectUnits = cls_00_ProjectUnits.GetAndSetProjectUnits();
                // Validamos
                if (string.IsNullOrEmpty(projectUnits)) return null;

                // Obtenemos Text Styles
                List<string> availableTextStyles = 
                    cls_00_DocumentInfo.GetAllTextStylesFromDrawing(db);

                // Validamos Config
                if (!cls_12_CreateEntityLabelsConfig.CreateEntityLabelsConfig(
                    solarSet, availableTextStyles,
                    out bool isHorizontal, out bool hasMPPT,
                    out bool hasTrackerInfo, out string separatorChar,
                    out Dictionary<string, string> labelFieldsDict,
                    out string selectedTextStyle, out AttachmentPoint selectedTextJust,
                    out bool analyzeAllDoc
                )) return null;

                // Obtenemos el listado de capas del documento
                List<string> docLayers = cls_00_GetLayerNamesFromDoc.GetLayerNamesFromDoc(db);

                // Seleccionamos Entidades
                if (!cls_12_GetRequiredEntities.GetRequiredEntities(
                    ed, solarSet, docLayers, analyzeAllDoc,
                    out SelectionSet analyzePoly,
                    out PromptSelectionResult psrPolyCt, out PromptSelectionResult psrPolyInv,
                    out PromptSelectionResult psrBlockRefTrack, out PromptSelectionResult psrLabelInv,
                    out string psrPolyCtLayer, out string psrPolyInvLayer,
                    out string psrBlockRefTrackLayer, out string psrLabelInvLayer
                )) return null;

                List<string> layersToCreate = new List<string>
                {
                    solarSet.PolyCtLayer, solarSet.PolyInvLayer, solarSet.BlockRefTrackLayer,
                    solarSet.PolyStringLayer, solarSet.LabelStringLayer, solarSet.LabelInvLayer
                };
                // Creamos las capas por defecto si no existen
                cls_00_CreateLayerIfNotExists.CreateLayersIfNotExist(layersToCreate, db);

                // Validamos elevaciones
                if (!cls_12_GetRequiredElev.GetRequiredElevations(
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
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return null;
                }

                HashSet<ObjectId> blockRefTrackToIsolate = new HashSet<ObjectId>();
                HashSet<ObjectId> blockRefTrackMultPoly = new HashSet<ObjectId>();
                HashSet<ObjectId> blockRefTrackElevMismatch = new HashSet<ObjectId>();
                // Iteramos
                foreach (ObjectId trkId in psrBlockRefTrack.Value.GetObjectIds())
                {
                    // Validamos Trackers/Strings
                    cls_12_AnalyzeTrackPolys.AnalyzeTrackPolys(
                        tr, trkId, solarSet, 
                        blockRefTrackMultPoly, blockRefTrackToIsolate, blockRefTrackElevMismatch
                    );
                }
                // Validamos
                if (!cls_12_IsolateInvalidTrack.IsolateInvalidTrack(
                    ed, solarSet,
                    blockRefTrackMultPoly, blockRefTrackToIsolate, blockRefTrackElevMismatch
                )) return null;

                // Obtenemos HashSet de Ids
                HashSet<ObjectId> psrPolyInvIds = new HashSet<ObjectId>(psrPolyInv.Value.GetObjectIds());
                HashSet<ObjectId> psrBlockRefTrackIds = new HashSet<ObjectId>(psrBlockRefTrack.Value.GetObjectIds());
                HashSet<ObjectId> psrLabelInvIds = new HashSet<ObjectId>(psrLabelInv.Value.GetObjectIds());

                // Definimos offset por defecto
                double offsetDistance = 0.15;

                // Obtenemos Regiones de los Contornos CT
                if (!cls_00_ProcessPolysToRegions.ProcessPolysToRegions(
                    ed, tr, btr, analyzePoly, solarSet.PolyCtTag, offsetDistance, projectUnits,
                    out List<Region> validRegionCt,
                    out Dictionary<Handle, Handle> dictPolyToRegionContGen
                )) return null;

                // Obtenemos Regiones de los Contornos Inversores
                if (!cls_00_ProcessPolysToRegions.ProcessPolysToRegions(
                    ed, tr, btr, psrPolyInv.Value, solarSet.PolyInvTag, offsetDistance, projectUnits,
                    out List<Region> validRegionInv,
                    out Dictionary<Handle, Handle> dictPolyToRegionContInv
                )) return null;

                // Almacenamos regiones para borrarlas
                List<Region> allValidRegion = new List<Region>();
                allValidRegion.AddRange(validRegionCt);
                allValidRegion.AddRange(validRegionInv);

                StringBuilder infoRegion = new StringBuilder();
                // Diccionario para almacenar los elementos por region
                Dictionary<Region, List<DBObject>> dictRegionCtData = new Dictionary<Region, List<DBObject>>();

                // Ordenar lista de regiones por centroide
                validRegionCt.Sort((a, b) => cls_00_GetEntityCentroid.CompareEntitiesByPosition(a, b, 10.0));

                // Contador de CT
                int ctStartIndex;
                int trackStartIndex;
                // Si no analizamos todo el documento
                if (!analyzeAllDoc)
                {
                    // CT
                    string msgCT = $"Enter the starting number for {solarSet.PolyCtTag}:";
                    string inputCT = InstanciarFormularios.TextBoxFormOut(msgCT, "1");
                    // Validamos
                    if (string.IsNullOrWhiteSpace(inputCT) || !int.TryParse(inputCT, out ctStartIndex))
                    {
                        // Mensaje
                        MessageBox.Show("Invalid CT number. Operation will be canceled.", "Input Error");
                        // Borramos regiones
                        foreach (Region region in allValidRegion)
                        {
                            if (region != null && !region.IsErased)
                            {
                                cls_00_DeleteEntity.DeleteEntity(region);
                            } 
                        }
                        // Finalizamos
                        return null;
                    }
                    // Tracker
                    string msgTrack = "Enter the starting number for Tracker:";
                    string inputTrack = InstanciarFormularios.TextBoxFormOut(msgTrack, "1");
                    // Validamos
                    if (string.IsNullOrWhiteSpace(inputTrack) || !int.TryParse(inputTrack, out trackStartIndex))
                    {
                        // Mensaje
                        MessageBox.Show("Invalid Tracker number. Operation will be canceled.", "Input Error");
                        // Borramos regiones
                        foreach (Region region in allValidRegion)
                        {
                            if (region != null && !region.IsErased)
                            {
                                cls_00_DeleteEntity.DeleteEntity(region);
                            }
                        }
                        // Finalizamos
                        return null;
                    }
                }
                // En caso de analizarlo
                else
                {
                    ctStartIndex = 1;
                    trackStartIndex = 1;
                }

                // Lista para almacenar todos los pares (CT, INV) detectados
                List<(int ctNumber, int invNumber, Region ctRegion, Region invRegion)> validRegionOrderList =
                    new List<(int, int, Region, Region)>();
                // Iteramos por los CT
                foreach (Region regionCt in validRegionCt)
                {
                    // Obtenemos los Inversores contenidos en este CT
                    List<Entity> polyInvAsEntList = cls_00_GetEntityListByRegion.GetEntityListByRegionByPoint(
                        tr, regionCt, psrPolyInvIds
                    );
                    // Validamos
                    if (polyInvAsEntList == null || polyInvAsEntList.Count == 0) continue;

                    // Obtenemos regiones de inversores
                    List<Region> regionInvList = cls_12_GetRegionsByInvOrder.GetRegionsByInvOrder(
                        polyInvAsEntList, dictPolyToRegionContInv, validRegionInv, infoRegion
                    );

                    // Iteramos por las regiones de los Inversores
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
                                // Validamos
                                if (parts.Length >= 2)
                                {
                                    // Extraemos campos numericos
                                    int.TryParse(new string(parts[0].Where(char.IsDigit).ToArray()), out ctNum);
                                    int.TryParse(new string(parts[1].Where(char.IsDigit).ToArray()), out invNum);
                                }
                            }
                            // Almacenamos
                            validRegionOrderList.Add((ctNum, invNum, regionCt, regionInv));
                        }
                    }
                }

                // Ordenar por CT y por Inversor
                var validRegionOrderListByCt = validRegionOrderList
                    .OrderBy(x => x.ctNumber).ThenBy(x => x.invNumber)
                    .GroupBy(x => x.ctNumber).ToList();

                HashSet<ObjectId> createdLabelIds = new HashSet<ObjectId>();
                // Contador global de etiquetas
                int totalLabelsCreated = 0;
                // Procesamos en el orden correcto
                foreach (var ctGroup in validRegionOrderListByCt)
                {
                    // Obtenemos la region del CT
                    Region regionCt = ctGroup.First().ctRegion;
                    // Lista de inversores ordenados dentro de este CT
                    List<(int invNumber, Region invRegion)> invRegionsOrdered =
                        ctGroup.Select(x => (x.invNumber, x.invRegion)).ToList();
                    // Procesamos CT
                    int labelsCreated = cls_12_ProcessCT.ProcessCtByInvLabel(
                        solarSet, regionCt, tr, btr,
                        psrBlockRefTrackIds, labelFieldsDict, ctStartIndex, trackStartIndex,
                        isHorizontal, selectedTextStyle, selectedTextJust, infoRegion,
                        hasMPPT, separatorChar, invRegionsOrdered, ref createdLabelIds
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

                // Actualizamos etiquetas creadas 
                foreach (var ctGroup in validRegionOrderListByCt)
                {
                    // Obtenemos la region del CT
                    Region regionCt = ctGroup.First().ctRegion;
                    // Accedemos a sus Inversores
                    List<(int invNumber, Region invRegion)> invRegionsOrdered =
                        ctGroup.Select(x => (x.invNumber, x.invRegion)).ToList();
                    // Iteramos Inversores
                    foreach (var (invNumber, invRegion) in invRegionsOrdered)
                    {
                        // Validamos
                        if (invRegion == null) continue;

                        // Obtenemos etiquetas por Inversor
                        List<Entity> invLabelEntities = cls_00_GetEntityListByRegion.
                            GetEntityListByRegionByPoint(tr, invRegion, createdLabelIds);
                        // Validamos
                        if (invLabelEntities == null || invLabelEntities.Count == 0) continue;

                        // Iteramos etiquetas
                        foreach (Entity ent in invLabelEntities)
                        {
                            // Validamos
                            if (!(ent is MText mText)) continue;
                            // Obtenemos los campos
                            string[] fields = cls_12_RemoveFieldFromLabel.
                                SplitLabelFields(mText.Contents, separatorChar); ;
                            // Accedemos al campo del Inversor (segundo)
                            string secondField = fields[1];
                            // Validamos
                            if (secondField.Contains("X"))
                            {
                                // Formateamos
                                string formattedInvNum = invNumber.ToString("D2");
                                secondField = Regex.Replace(secondField, "X+", formattedInvNum);
                                // Actualizamos
                                fields[1] = secondField;
                                // Recomponemos la etiqueta
                                string newText = string.Join(separatorChar, fields);
                                // Actualizamos valor
                                cls_12_RemoveFieldFromLabel.UpdateMTextContents(
                                    mText, newText, infoRegion,
                                    "Label updated", "Error updating label"
                                );
                            }
                        }

                        // Numeracion de strings global por inversor
                        int stringIndex = 1;
                        // Agrupamos por Tracker (antes limpiamos todo antes del espacio)
                        var groupedByTracker = invLabelEntities
                            .OfType<MText>()
                            .GroupBy(m =>
                            {
                                // Obtenemos contenido
                                string cleanLabel = cls_12_RemoveFieldFromLabel.GetCleanLabel(m.Contents);
                                // Obtenemos los campos
                                string[] fields = cls_12_RemoveFieldFromLabel.
                                    SplitLabelFields(cleanLabel, separatorChar);
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
                                string cleanLabel =
                                    cls_12_RemoveFieldFromLabel.GetCleanLabel(mText.Contents);
                                // Obtenemos los campos
                                string[] fields = cls_12_RemoveFieldFromLabel.
                                    SplitLabelFields(cleanLabel, separatorChar);
                                // Obtenemos el campo del String (ultimo)
                                int lastIndex = fields.Length - 1;
                                string lastField = fields[lastIndex].Trim();
                                // Validamos que tenga X
                                if (lastField.Contains("X"))
                                {
                                    // Formateamos
                                    string formattedStringNum = stringIndex.ToString("D2");
                                    lastField = Regex.Replace(lastField, "X+", formattedStringNum);
                                    // Actualizamos
                                    fields[lastIndex] = lastField;
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

                // Si el usuario no quiere mostrar el campo Tracker en la etiqueta
                if (!hasTrackerInfo)
                {
                    // Iteramos
                    foreach (ObjectId lblId in createdLabelIds)
                    {
                        // Actualizamos etiqueta
                        cls_12_RemoveFieldFromLabel.RemoveFieldFromLabel(
                            tr, lblId, separatorChar, infoRegion
                        );
                    }
                }

                // Borrar regiones de contornos generales
                foreach (Region region in validRegionCt)
                {
                    // Validamos
                    if (region != null && !region.IsErased)
                    {
                        cls_00_DeleteEntity.DeleteEntity(region);
                    }
                }

                // Borrar regiones de contornos de inversores
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
        }






    }
}
