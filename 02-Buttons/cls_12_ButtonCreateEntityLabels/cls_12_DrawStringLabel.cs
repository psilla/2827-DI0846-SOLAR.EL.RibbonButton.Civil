using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using TYPSA.SharedLib.Civil.DrawEntities;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_DrawStringLabel
    {
        public static void DrawStringLabel(
            Polyline str,
            BlockReference tracker,
            Transaction tr,
            BlockTableRecord btr,
            Dictionary<string, string> propPreDict,
            string contGenProp,
            int contGenIndex,
            string contInvProp,
            int invIndex,
            string trackProp,
            int trackIndex,
            string stringProp,
            int stringIndex,
            bool isHorizontal,
            string labelsLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool dolarSelBool
        )
        {
            // Definir el separador del inversor según configuración
            string invPart = $"{propPreDict[contInvProp]}0{invIndex}";
            if (dolarSelBool)
            {
                // Si activamos MPPT
                invPart += "-$-";  
            }
            else
            {
                // En caso contrario
                invPart += "-";   
            }

            // Definir la etiqueta final
            string tagText =
                $"{propPreDict[contGenProp]}0{contGenIndex}-" +
                $"{invPart}" +
                $"{propPreDict[trackProp]}0{trackIndex}-" +
                $"{propPreDict[stringProp]}{stringIndex:D2} +/-";

            // try
            try
            {
                // Calcular punto medio de la arista inferior real
                Point3d basePoint = 
                    cls_12_GetBottomEdgeMidPoint.GetBottomEdgeMidPoint(str, tracker);

                // Dibujar etiqueta en coordenadas globales
                cls_00_DrawEntities.DrawMTextOnPoint(
                    basePoint, tagText, tr, btr,
                    isHorizontal, 1, 4, labelsLayer,
                    chosenStyle, chosenJustification
                );
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show(
                    $"Failed to calculate/draw label '{tagText}'.\n\nError: {ex.Message}",
                    "Calculation/Drawing Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            // Mostrar info
            infoRegiones.AppendLine($"\t\t\t- String {stringIndex:D2} en Tracker {trackIndex} ({tracker.Handle})");
        }




    }
}
