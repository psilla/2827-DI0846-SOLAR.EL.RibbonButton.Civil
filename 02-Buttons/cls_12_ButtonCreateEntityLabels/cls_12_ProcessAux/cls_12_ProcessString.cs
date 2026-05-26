using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.DrawEntities;
using TYPSA.SharedLib.Autocad.GetEntityCoordinates;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_ProcessString
    {
        public static ObjectId ProcessString(
            Transaction tr,
            BlockTableRecord btr,
            SolarSettings solarSet,
            Polyline polyStr,
            BlockReference blockreftrack,
            Dictionary<string, string> labelFieldsDict,
            string invInCtLayer,
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
            string charSepSel,
            bool inverterOutsideCt
        )
        {
            ObjectId labelId = ObjectId.Null;

            // -----------------------------
            // Numero de digitos para tracker
            // -----------------------------

            int trackDigits = Math.Max(2, totalTrackersCT.ToString().Length);
            string trackIndexStr = trackIndex.ToString($"D{trackDigits}");

            // -----------------------------
            // Numero de digitos para inversor
            // -----------------------------

            int invDigits = Math.Max(2, invIndex.ToString().Length);

            // -----------------------------
            // Numero de digitos para string
            // -----------------------------

            int strDigits = Math.Max(2, stringIndex.ToString().Length);

            // -----------------------------
            // Parte region
            // -----------------------------

            string regionPart = string.IsNullOrWhiteSpace(numCtAsString)
                ? ctStartIndex.ToString("D2")
                : numCtAsString.PadLeft(2, '0');

            // -----------------------------
            // Reemplazar Inversor/String por incognita
            // -----------------------------

            string displayInvPart = new string('X', invDigits);
            string displayStrPart = new string('X', strDigits);

            // -----------------------------
            // Obtener parte Inversor/Combiner
            // -----------------------------

            string invProp = inverterOutsideCt 
                ? solarSet.ContInvProp 
                : solarSet.ComBoxProp;
            // Vemos que prefijo usar
            string invPart = $"{labelFieldsDict[invProp]}{displayInvPart}";
            invPart += MPPtSelBool
                ? $"{charSepSel}${charSepSel}"
                : $"{charSepSel}";

            // -----------------------------
            // Obtener parte layer inversor en CT
            // -----------------------------

            string invInCtLayerPart = "";
            // Dentro del CT
            if (!inverterOutsideCt && !string.IsNullOrWhiteSpace(invInCtLayer)
            )
            {
                // Obtener parte numerica
                string numericPart = new string(invInCtLayer.Where(char.IsDigit).ToArray());
                // Obtenemos 2 ultimos digitos
                if (numericPart.Length > 2)
                {
                    numericPart = numericPart.Substring(numericPart.Length - 2);
                }
                // Validamos
                if (!string.IsNullOrWhiteSpace(numericPart))
                {
                    invInCtLayerPart =$"{labelFieldsDict[solarSet.ContInvInCtProp]}" + $"{numericPart}" + $"{charSepSel}";
                }
            }

            // -----------------------------
            // Construir la etiqueta final
            // -----------------------------

            string tagText =
                $"{labelFieldsDict[solarSet.ContGenProp]}{regionPart}{charSepSel}" +
                $"{invInCtLayerPart}" +
                $"{invPart}" +
                $"{labelFieldsDict[solarSet.TrackProp]}{trackIndexStr}{charSepSel}" +
                $"{labelFieldsDict[solarSet.StringProp]}{displayStrPart} +/-";

            // try
            try
            {
                // Definimos pto insercion de la etiqueta
                Point3d basePoint = isHorizontal
                    ? cls_00_GetBottomPoint.GetBottomEdgeLeftPoint(polyStr, blockreftrack)
                    : cls_00_GetBottomPoint.GetBottomEdgeMidPoint(polyStr, blockreftrack);

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
                    basePoint, tagText, tr, btr, isHorizontal, 1, 7, 
                    solarSet.LabelStringLayer, chosenStyle, chosenJustification
                );
            }
            // catch
            catch (System.Exception) { }
            // return
            return labelId;
        }





    }
}
