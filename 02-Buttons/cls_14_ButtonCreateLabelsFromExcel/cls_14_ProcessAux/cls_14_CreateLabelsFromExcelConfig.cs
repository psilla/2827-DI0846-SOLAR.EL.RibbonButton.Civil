using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using TYPSA.SharedLib.Autocad.DrawEntities;
using TYPSA.SharedLib.UserForms;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_14_CreateLabelsFromExcelConfig
    {
        public static bool CreateLabelsFromExcelConfig(
            SolarSettings solarSet,
            List<string> availableTextStyles,
            out bool isHorizontal,
            out string selectedTextStyle,
            out AttachmentPoint selectedTextJust
        )
        {
            // Valores por defecto
            isHorizontal = false;
            selectedTextStyle = string.Empty;
            selectedTextJust = AttachmentPoint.MiddleCenter;

            // Obtenemos valores
            string tipTrack = solarSet.TipTrack;
            string tipEstFija = solarSet.TipEstFija;

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

            // return
            return true;
        }
    }
}
