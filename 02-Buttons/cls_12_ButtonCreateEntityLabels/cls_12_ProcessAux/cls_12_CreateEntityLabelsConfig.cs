using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.DrawEntities;
using TYPSA.SharedLib.UserForms;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_CreateEntityLabelsConfig
    {
        public static bool CreateEntityLabelsConfig(
            SolarSettings solarSet,
            List<string> availableTextStyles,
            out bool isHorizontal,
            out bool hasMPPT,
            out bool hasTrackerInfo,
            out string separatorChar,
            out Dictionary<string, string> labelFieldsDict,
            out string selectedTextStyle,
            out AttachmentPoint selectedTextJust,
            out bool analyzeAllDoc
        )
        {
            // Valores por defecto
            isHorizontal = false;
            hasMPPT = false;
            hasTrackerInfo = false;
            separatorChar = string.Empty;
            labelFieldsDict = new Dictionary<string, string>();
            selectedTextStyle = string.Empty;
            selectedTextJust = AttachmentPoint.MiddleCenter;
            analyzeAllDoc = false;

            // Definimos valores
            string tipTrack = solarSet.TipTrack;
            string tipEstFija = solarSet.TipEstFija;
            string contGenTag = solarSet.PolyCtTag;

            // DEFINIMOS ORIENTACION DE LOS TRACKERS (VERTICAL U HORIZONTAL)
            string trackSel = InstanciarFormularios.DropDownFormListOut(
                "Select the String configuration typology:",
                new List<string> { tipTrack, tipEstFija },
                "String Typology", tipTrack
            );
            // Validamos
            if (trackSel == null) return false;
            // Definimos orientacion label
            isHorizontal = (trackSel == tipEstFija);

            // DEFINIMOS CONFIGURACION CON/SIN MPPT
            string MPPtConfigMess =
                "True: Label configuration with MPPT ($).\n" +
                "False: Label configuration without MPPT ($).";
            // Form
            object MPPtSel = InstanciarFormularios.DropDownFormOut(
                MPPtConfigMess, false
            );
            // Validamos
            if (MPPtSel == null) return false;
            // Convertimos a boolean
            hasMPPT = Convert.ToBoolean(MPPtSel);

            // DEFINIMOS CONFIGURACION CON/SIN TRACKER INFO
            string trackLabelConfigMess =
                "True: Label configuration with Tracker info.\n" +
                "False: Label configuration without Tracker info.";
            // Form
            object trackLabelSel = InstanciarFormularios.DropDownFormOut(
                trackLabelConfigMess, true
            );
            // Validamos
            if (trackLabelSel == null) return false;
            // Convertimos a boolean
            hasTrackerInfo = Convert.ToBoolean(trackLabelSel);

            // DEFINIMOS CARACTER SEPARADOR LABELS
            separatorChar = InstanciarFormularios.DropDownFormListOut(
                "Select the separator character for the label:",
                new List<string> { ".", "-", "_", ",", ";" },
                "Label Separator Selection", "-"
            );
            // Validamos la selección
            if (string.IsNullOrEmpty(separatorChar)) return false;

            List<(string propiedad, string valorDefecto)> props = new List<(string, string)>
            {
                (solarSet.ContGenProp, "P"),
                (solarSet.ContInvProp, "INV"),
                (solarSet.TrackProp, "TR"),
                (solarSet.StringProp, "S"),
            };
            // DEFINIMOS PREFIJOS PARA ETIQUETA
            labelFieldsDict = InstanciarFormularios.TextBoxFormOut_NextToLabel(
                "Enter a prefix for each entity contained in the label", props
            );
            // Validamos
            if (labelFieldsDict == null) return false;

            // DEFINIMOS TEXT STYLE
            string chosenStyle = cls_00_DrawEntities.AskTextStyleFromUser(
                availableTextStyles, solarSet.LabelStyle
            );
            // Validamos
            if (chosenStyle == null) return false;

            // DEFINIMOS TEXT JUSTIFICATION
            // Form para definir text justification
            if (isHorizontal)
            {
                // BottomLeft
                selectedTextJust = cls_00_DrawEntities
                    .AskMTextJustificationFromUser(AttachmentPoint.BottomLeft);
            }
            else
            {
                // TopLeft
                selectedTextJust = cls_00_DrawEntities
                    .AskMTextJustificationFromUser(AttachmentPoint.TopLeft);
            }

            // DEFINIMOS TRUE/FALSE TODO EL DOCUMENTO
            string boolDocMess =
                $"True: Analyze all {contGenTag} in the document.\n" +
                $"False: Manually select {contGenTag} to analyze.";
            // Form
            object boolDoc = InstanciarFormularios.DropDownFormOut(
                boolDocMess, true
            );
            // Validamos
            if (boolDoc == null) return false;
            // Convertimos a boolean
            analyzeAllDoc = Convert.ToBoolean(boolDoc);

            // return
            return true;
        }

    }
}
