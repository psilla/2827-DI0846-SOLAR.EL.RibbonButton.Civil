using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_AnalyzeTrackPolys
    {
        public static void AnalyzeTrackPolys(
            Transaction tr,
            ObjectId trkId,
            SolarSettings solarSet,
            HashSet<ObjectId> trackersWithMultiplePoly,
            HashSet<ObjectId> trackersToIsolate,
            HashSet<ObjectId> trackersWithElevMismatch
        )
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
            int trackerCount = 0;
            int stringCount = 0;
            List<double> trackElevs = new List<double>();
            List<double> stringElevs = new List<double>();

            // Iteramos
            foreach (ObjectId entId in trackerBtr)
            {
                // Analizamos poly
                if (tr.GetObject(entId, OpenMode.ForRead) is Polyline poly)
                {
                    // Validamos por capa
                    if (poly.Layer.Equals(solarSet.BlockRefTrackLayer, StringComparison.OrdinalIgnoreCase))
                    {
                        // Actualizamos contador
                        trackerCount++;
                        // Añadimos
                        trackElevs.Add(poly.Elevation);
                    }
                    else if (poly.Layer.Equals(solarSet.PolyStringLayer, StringComparison.OrdinalIgnoreCase))
                    {
                        // Actualizamos contador
                        stringCount++;
                        // Añadimos
                        stringElevs.Add(poly.Elevation);
                    }
                }
            }
            // Validamos
            // Caso 1: mas de 1 poly en capa trackLayer
            if (trackerCount > 1)
            {
                // Añadimos
                trackersWithMultiplePoly.Add(trkId);
            }
            // Caso 2: no tiene ambas capas
            else if (trackerCount == 0 || stringCount == 0)
            {
                // Añadimos
                trackersToIsolate.Add(trkId);
            }
            // Validamos elevation
            if ((trackElevs.Count > 1 && !AllElevationsEqual(trackElevs)) ||
                (stringElevs.Count > 1 && !AllElevationsEqual(stringElevs))
            )
            {
                // Añadimos
                trackersWithElevMismatch?.Add(trkId);
            }
        }

        private static bool AllElevationsEqual(List<double> elevations, double tolerance = 1e-3)
        {
            // Validamos
            if (elevations == null || elevations.Count <= 1) return true;
            // Obtenemos elevacion
            double reference = elevations[0];
            // return
            return elevations.All(e => Math.Abs(e - reference) <= tolerance);
        }






    }
}
