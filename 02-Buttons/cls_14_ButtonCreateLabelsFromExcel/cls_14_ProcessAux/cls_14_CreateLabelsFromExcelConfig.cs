using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using TYPSA.SharedLib.Autocad.DrawEntities;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_14_CreateLabelsFromExcelConfig
    {
        public static bool CreateLabelsFromExcelConfig(
            string tipTrack,
            string tipEstFija,
            string defaultTextStyle,
            List<string> availableTextStyles,
            out bool isHorizontal,
            out string selectedTextStyle,
            out AttachmentPoint selectedTextJust
        )
        {
            // -------------------------------
            // Valores por defecto
            // -------------------------------

            isHorizontal = false;
            selectedTextStyle = string.Empty;
            selectedTextJust = AttachmentPoint.MiddleCenter;

            // -------------------------------
            // Definir orientación
            // -------------------------------

            string trackSel = cls_00_InstaForm_ComboBox.ComboBoxFormListOut(
                "Select the String configuration typology:",
                new List<string> { tipTrack, tipEstFija },
                formText: "String Typology", defaultValue: tipTrack
            );
            // Validamos
            if (trackSel == null) return false;
            // Definimos orientacion label
            isHorizontal = (trackSel == tipEstFija);

            // -------------------------------
            // Definir Text Style
            // -------------------------------

            string chosenStyle = cls_00_DrawEntities.AskTextStyleFromUser(
                availableTextStyles, defaultTextStyle
            );
            // Validamos
            if (chosenStyle == null) return false;

            // Asignamos
            selectedTextStyle = chosenStyle;

            // -------------------------------
            // Definir Text Justification
            // -------------------------------

            if (isHorizontal)
            {
                // BottomLeft
                selectedTextJust = cls_00_DrawEntities.AskMTextJustificationFromUser(
                    AttachmentPoint.BottomLeft
                );
            }
            else
            {
                // TopLeft
                selectedTextJust = cls_00_DrawEntities.AskMTextJustificationFromUser(
                    AttachmentPoint.TopLeft
                );
            }

            // -------------------------------
            // Return
            // -------------------------------

            return true;
        }


    }
}
