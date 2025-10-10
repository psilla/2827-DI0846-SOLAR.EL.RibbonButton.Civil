using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_ValidateTracker
    {
        public static void ValidateTracker(
            Transaction tr,
            ObjectId trkId,
            string trackLayer,
            string stringLayer,
            HashSet<ObjectId> trackersWithMultipleEtracker,
            HashSet<ObjectId> trackersToIsolate)
        {
            // Obtenemos el tracker
            BlockReference tracker =
                tr.GetObject(trkId, OpenMode.ForRead) as BlockReference;
            // Validamos
            if (tracker == null) return;

            // Obtenemos la btr
            BlockTableRecord trackerBtr =
                tr.GetObject(tracker.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
            // Validamos
            if (trackerBtr == null) return;

            // Iniciamos contadores
            int etrackerCount = 0;
            int stringTxtCount = 0;

            // Iteramos
            foreach (ObjectId entId in trackerBtr)
            {
                // Validamos poly
                if (tr.GetObject(entId, OpenMode.ForRead) is Polyline poly)
                {
                    // Validamos capas
                    if (poly.Layer.Equals(trackLayer, StringComparison.OrdinalIgnoreCase))
                        etrackerCount++;
                    if (poly.Layer.Equals(stringLayer, StringComparison.OrdinalIgnoreCase))
                        stringTxtCount++;
                }
            }
            // Validamos
            if (etrackerCount > 1)
            {
                // Caso 1: más de 1 poly en capa trackLayer
                trackersWithMultipleEtracker.Add(trkId);
            }
            else if (etrackerCount == 0 || stringTxtCount == 0)
            {
                // Caso 2: no tiene ambas capas
                trackersToIsolate.Add(trkId);
            }
        }






    }
}
