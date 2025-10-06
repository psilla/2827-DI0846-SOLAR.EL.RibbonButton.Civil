using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_GetStringsInTrack
    {
        public static List<Polyline> GetStringsInTrack(
            BlockReference tracker,
            Transaction tr,
            string stringLayer
        )
        {
            List<Polyline> stringsInTracker = new List<Polyline>();
            // Accedemos al bloque del tracker
            BlockTableRecord trackerBtr =
                tr.GetObject(tracker.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;

            // Validamos
            if (trackerBtr != null)
            {
                // Iteramos
                foreach (ObjectId entId in trackerBtr)
                {
                    // Buscamos poly 
                    if (tr.GetObject(entId, OpenMode.ForRead) is Polyline poly)
                    {
                        // Solo aceptar si la capa coincide
                        if (poly.Layer.Equals(stringLayer, StringComparison.OrdinalIgnoreCase))
                        {
                            // Almacenamos
                            stringsInTracker.Add(poly);
                        }
                    }
                }
            }
            // return
            return stringsInTracker;
        }




    }
}
