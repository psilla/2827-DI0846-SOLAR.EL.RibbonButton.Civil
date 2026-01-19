using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using TYPSA.SharedLib.Autocad.DrawEntities;
using TYPSA.SharedLib.Autocad.GetEntityCoordinates;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_ProcessString
    {
        public static ObjectId ProcessString(
            Transaction tr,
            BlockTableRecord btr,
            SolarSettings solarSet,
            Polyline str,
            BlockReference tracker,
            Dictionary<string, string> propPreDict,
            int ctStartIndex,
            int invIndex,
            int trackIndex,
            int stringIndex,
            bool isHorizontal,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool MPPtSelBool,
            string numCtAsString,
            int totalTrackersCT,
            string charSepSel
        )
        {
            ObjectId labelId = ObjectId.Null;

            // Número de dígitos para track
            int trackDigits = Math.Max(2, totalTrackersCT.ToString().Length);
            string trackIndexStr = trackIndex.ToString($"D{trackDigits}");

            // Número de dígitos para inversor y string
            int invDigits = Math.Max(2, invIndex.ToString().Length);
            int strDigits = Math.Max(2, stringIndex.ToString().Length);

            string regionPart = string.IsNullOrWhiteSpace(numCtAsString)
                ? ctStartIndex.ToString("D2")
                : numCtAsString.PadLeft(2, '0');

            // Siempre reemplazamos por X según la longitud
            string displayInvPart = new string('X', invDigits);
            string displayStrPart = new string('X', strDigits);

            string invPart = $"{propPreDict[solarSet.ContInvProp]}{displayInvPart}";
            invPart += MPPtSelBool
                ? $"{charSepSel}${charSepSel}"
                : $"{charSepSel}";

            // Construimos la etiqueta final (siempre incluye Tracker)
            string tagText =
                $"{propPreDict[solarSet.ContGenProp]}{regionPart}{charSepSel}" +
                $"{invPart}" +
                $"{propPreDict[solarSet.TrackProp]}{trackIndexStr}{charSepSel}" +
                $"{propPreDict[solarSet.StringProp]}{displayStrPart} +/-";

            // try
            try
            {
                // Definimos pto insercion de la etiqueta
                Point3d basePoint = isHorizontal
                    ? cls_00_GetBottomPoint.GetBottomEdgeLeftPoint(str, tracker)
                    : cls_00_GetBottomPoint.GetBottomEdgeMidPoint(str, tracker);

                double margin = 0.1;
                // Aplicamos margen
                if (isHorizontal)
                {
                    // Hacia la derecha (X+)
                    basePoint = new Point3d(
                        basePoint.X + margin, basePoint.Y, basePoint.Z
                    );
                }
                else
                {
                    // Hacia arriba (Y+)
                    basePoint = new Point3d(
                        basePoint.X, basePoint.Y + margin, basePoint.Z
                    );
                }
                // Dibujamos etiqueta
                labelId = cls_00_DrawMtext.DrawMTextOnPoint(
                    basePoint, tagText, tr, btr,
                    isHorizontal, 1, 7, solarSet.LabelStringLayer,
                    chosenStyle, chosenJustification
                );
            }
            // catch
            catch (System.Exception) { }
            // return
            return labelId;
        }





    }
}
