using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.Autocad.GetEntityCoordinates;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_ProcessInverter
    {
        public static int ProcessInverter(
            Region invRegion,
            Transaction tr,
            BlockTableRecord btr,
            HashSet<ObjectId> trackersIds,
            HashSet<ObjectId> invLabelIds,
            string trackTag,
            string contInvLabelTag,
            string stringLayer,
            Dictionary<string, string> propPreDict,
            string contGenProp, int ctStartIndex,
            string contInvProp, int invIndex,
            string trackProp, ref int trackIndex,
            string stringProp,
            bool isHorizontal,
            string labelsTrackLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool MPPtSelBool,
            int totalTrackersCT,
            string charSepSel,
            ref HashSet<ObjectId> createdLabelIds,
            List<Entity> allTrackersInCT
        )
        {
            // Contador de etiquetas creadas dentro de este inversor
            int labelsCreatedTotal = 0;

            // Obtenemos la etiqueta del inversor
            List<Entity> invLabelEntities = cls_12_GetTrackInRegInv.
                GetEntInRegByPoint(tr, invRegion, invLabelIds);
            // Validamos
            if (invLabelEntities == null || invLabelEntities.Count != 1) return 0;

            // Obtenemos la etiqueta del inversor
            Entity invLabelEnt = invLabelEntities.First();
            // Validamos
            if (!TryGetNumInvLabel(invLabelEnt, out string numCenTranAsString, out string numInvAsString)) return 0;

            // Obtenemos los trackers del inversor por pto unico
            List<Entity> trackerEntities = cls_12_GetTrackInRegInv.
                GetEntInRegByPoint(tr, invRegion, trackersIds);
            // Validamos
            if (trackerEntities == null || trackerEntities.Count == 0) return 0;
            // Obtenemos los trackers del inversor por varios puntos
            List<Entity> trackerEntitiesByPts = cls_12_GetTrackInRegInv.GetEntInRegByPoints(
                tr, invRegion, trackersIds
            );

            // Listas de handles para identificar diferencias
            var handlesByPoint = new HashSet<string>(trackerEntities.Select(t => t.Handle.ToString()));
            var handlesByPoints = new HashSet<string>(trackerEntitiesByPts.Select(t => t.Handle.ToString()));
            // Trackers detectados solo por un 1 pto
            var onlyInByPoint = handlesByPoint.Except(handlesByPoints).ToList();

            // Ordenamos lista de Trackers por centroide
            trackerEntities = cls_00_GetEntityCentroid.OrderByColumns(
                trackerEntities,
                e => cls_00_GetEntityCentroid.GetEntityCentroid(e)
            );

            // Contador de strings global dentro del inversor
            int stringIndex = 1;
            // Iteramos trackers
            foreach (BlockReference tracker in trackerEntities)
            {
                // Definimos bool
                bool shouldDrawLabels = !onlyInByPoint.Contains(tracker.Handle.ToString());
                // Procesamos el Tracker
                int labelsCreated = cls_12_ProcessTrack.ProcessTrack(
                    tracker, tr, btr, stringLayer, propPreDict,
                    contGenProp, ctStartIndex,
                    contInvProp, invIndex,
                    trackProp, trackIndex,
                    stringProp, ref stringIndex,
                    isHorizontal, labelsTrackLayer,
                    chosenStyle, chosenJustification, infoRegiones,
                    MPPtSelBool, numCenTranAsString, numInvAsString,
                    totalTrackersCT, charSepSel,
                    ref createdLabelIds, shouldDrawLabels
                );
                // Actualizamos contador
                trackIndex++;
                // Sumamos al total
                labelsCreatedTotal += labelsCreated;
            }
            // return
            return labelsCreatedTotal;
        }

        public static bool TryGetNumInvLabel(
            Entity invLabelEnt,
            out string numCenTranAsString,
            out string numInvAsString
        )
        {
            // Inicializamos valores de salida
            numCenTranAsString = string.Empty;
            numInvAsString = string.Empty;
            // Validamos
            if (!(invLabelEnt is MText mtext))
            {
                // Mensaje
                MessageBox.Show(
                    $"⚠ The inverter label entity is not an MText.",
                    "Invalid Entity Type",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                // Finalizamos
                return false;
            }

            // Obtenemos el valor
            string invLabelText = mtext.Contents.Trim();

            // Separadores válidos posibles
            char[] validSeparators = { '.', '-', '_', ',', ';' };

            // Detectamos separador
            char? detectedSeparator = validSeparators.FirstOrDefault(s => invLabelText.Contains(s));
            // Validamos 
            if (detectedSeparator == null || detectedSeparator == '\0')
            {
                // Mensaje
                MessageBox.Show(
                    $"⚠ The label '{invLabelText}' does not contain any valid separator.\n" +
                    "Accepted separators: . - _ , ;",
                    "Invalid Label Format",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                // Finalizamos
                return false;
            }

            // Dividimos por el separador detectado
            string[] parts = invLabelText.Split(new[] { detectedSeparator.Value }, StringSplitOptions.None);
            // Validamos 
            if (parts.Length != 2)
            {
                // Mensaje
                MessageBox.Show(
                    $"⚠ The text '{invLabelText}' contains more than one separator or has an unexpected format.",
                    "Invalid Label Format",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                // Finalizamos
                return false;
            }

            // Extraemos solo digitos
            numCenTranAsString = new string(parts[0].Where(char.IsDigit).ToArray());
            numInvAsString = new string(parts[1].Where(char.IsDigit).ToArray());
            // Validamos 
            if (string.IsNullOrEmpty(numCenTranAsString) || string.IsNullOrEmpty(numInvAsString))
            {
                // Mensaje
                MessageBox.Show(
                    $"⚠ The label '{invLabelText}' does not contain valid numeric values before/after the separator.",
                    "Invalid Label Numbers",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                // Finalizamos
                return false;
            }
            // return
            return true;
        }







    }
}
