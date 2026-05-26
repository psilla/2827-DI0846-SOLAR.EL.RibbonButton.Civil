using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.DrawEntities;
using TYPSA.SharedLib.UserForms;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_CreateEntityLabelsConfig
    {
        private static class LabelConfigKeys
        {
            // String configuration
            public const string StringTypology = "Select the string configuration typology (Tracker / Fixed Structure):";

            // MPPT
            public const string MPPTConfiguration = "Enable MPPT in label configuration ($):";

            // Tracker info
            public const string TrackerInfo = "Include tracker information in label:";

            // Separator
            public const string LabelSeparator = "Select the separator character for the label:";

            // Text style
            public const string TextStyle = "Select the text style for labels:";

            // Document analysis
            public const string AnalyzeDocument = "Analyze all elements in the document (True) or select manually (False):";

            // Inverter location
            public const string InverterOutsideCt = "Are the inverters outside the CT (True) or inside the CT (False):";
        }

        public static bool CreateEntityLabelsConfig(
            SolarSettings solarSet,
            List<string> availableTextStyles,
            out bool isHorizontal,
            out bool hasMPPT,
            out bool hasTrackerInfo,
            out bool inverterOutsideCt,
            out string separatorChar,
            out Dictionary<string, string> labelFieldsDict,
            out string selectedTextStyle,
            out AttachmentPoint selectedTextJust,
            out bool analyzeAllDoc
        )
        {
            // Defaults
            isHorizontal = false;
            hasMPPT = false;
            hasTrackerInfo = false;
            inverterOutsideCt = true;
            separatorChar = string.Empty;
            labelFieldsDict = new Dictionary<string, string>();
            selectedTextStyle = string.Empty;
            selectedTextJust = AttachmentPoint.MiddleCenter;
            analyzeAllDoc = false;

            string tipTrack = solarSet.TipTrack;
            string tipEstFija = solarSet.TipEstFija;
            string contGenTag = solarSet.PolyCtTag;

            // Definimos campos
            var fields = new List<ComboTextBoxForm_NextToLabel.FieldDefinition>()
            {
                new ComboTextBoxForm_NextToLabel.FieldDefinition
                {
                    Propiedad = LabelConfigKeys.StringTypology,
                    Type = FieldType.ComboBox,
                    Opciones = new List<string> { tipTrack, tipEstFija },
                    ValorDefecto = tipTrack
                },

                new ComboTextBoxForm_NextToLabel.FieldDefinition
                {
                    Propiedad = LabelConfigKeys.MPPTConfiguration,
                    Type = FieldType.ComboBox,
                    Opciones = new List<string> { "True", "False" },
                    ValorDefecto = "False"
                },

                new ComboTextBoxForm_NextToLabel.FieldDefinition
                {
                    Propiedad = LabelConfigKeys.TrackerInfo,
                    Type = FieldType.ComboBox,
                    Opciones = new List<string> { "True", "False" },
                    ValorDefecto = "True"
                },

                new ComboTextBoxForm_NextToLabel.FieldDefinition
                {
                    Propiedad = LabelConfigKeys.InverterOutsideCt,
                    Type = FieldType.ComboBox,
                    Opciones = new List<string> { "True", "False" },
                    ValorDefecto = "False"
                },

                new ComboTextBoxForm_NextToLabel.FieldDefinition
                {
                    Propiedad = LabelConfigKeys.LabelSeparator,
                    Type = FieldType.ComboBox,
                    Opciones = new List<string> { ".", "-", "_", ",", ";" },
                    ValorDefecto = "-"
                },

                new ComboTextBoxForm_NextToLabel.FieldDefinition
                {
                    Propiedad = LabelConfigKeys.TextStyle,
                    Type = FieldType.ComboBox,
                    Opciones = availableTextStyles,
                    ValorDefecto = solarSet.LabelStyle
                },

                new ComboTextBoxForm_NextToLabel.FieldDefinition
                {
                    Propiedad = LabelConfigKeys.AnalyzeDocument,
                    Type = FieldType.ComboBox,
                    Opciones = new List<string> { "True", "False" },
                    ValorDefecto = "True"
                }
            };
            // Form
            var form = new ComboTextBoxForm_NextToLabel(
                "Define label configuration settings:", fields, formTitle: "Label Configuration Form"
            );
            // Validamos
            if (form.ShowDialog() != DialogResult.OK) return false;

            // Obtenemos salida
            Dictionary<string, string> comboResult = form.salida;

            // Asignamos
            isHorizontal = comboResult[LabelConfigKeys.StringTypology] == tipEstFija;
            hasMPPT = Convert.ToBoolean(comboResult[LabelConfigKeys.MPPTConfiguration]);
            hasTrackerInfo = Convert.ToBoolean(comboResult[LabelConfigKeys.TrackerInfo]);
            inverterOutsideCt = Convert.ToBoolean(comboResult[LabelConfigKeys.InverterOutsideCt]);
            separatorChar = comboResult[LabelConfigKeys.LabelSeparator];
            selectedTextStyle = comboResult[LabelConfigKeys.TextStyle];
            analyzeAllDoc = Convert.ToBoolean(comboResult[LabelConfigKeys.AnalyzeDocument]);

            // Justificacion Text
            selectedTextJust = isHorizontal
                ? cls_00_DrawEntities.AskMTextJustificationFromUser(AttachmentPoint.BottomLeft)
                : cls_00_DrawEntities.AskMTextJustificationFromUser(AttachmentPoint.TopLeft);

            List<(string propiedad, string valorDefecto)> props = new List<(string, string)>
            {
                (solarSet.ContGenProp, "P"),
            };
            // Inversor fuera CT
            if (inverterOutsideCt)
            {
                props.Add((solarSet.ContInvProp, "INV"));
            }
            // Inversor dentro CT
            else
            {
                props.Add((solarSet.ContInvInCtProp, "INV"));
                props.Add((solarSet.ComBoxProp, "SCB"));
            }
            // Genericas
            props.Add((solarSet.TrackProp, "TR"));
            props.Add((solarSet.StringProp, "S"));

            // Prefijos
            labelFieldsDict = cls_00_InstaForm_TextBox.TextBoxFormOut_NextToLabel(
                "Enter a prefix for each entity included in the label. " + "All fields are required.\n\n" +
                "Tracker field will be ignored depending on previous selection.", props
            );
            // Validamos
            if (labelFieldsDict == null) return false;

            // return
            return true;
        }

     

    }
}
