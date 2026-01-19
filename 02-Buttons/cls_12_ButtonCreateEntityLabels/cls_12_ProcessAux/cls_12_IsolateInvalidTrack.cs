using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.IsolateEntities;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_12_IsolateInvalidTrack
    {
        public static bool IsolateInvalidTrack(
            Editor ed,
            SolarSettings solarSet,
            HashSet<ObjectId> trackersWithMultiplePoly,
            HashSet<ObjectId> trackersToIsolate,
            HashSet<ObjectId> trackersWithElevMismatch
        )
        {
            // Definimos valores
            string trackTag = solarSet.BlockRefTrackTag;
            string trackLayer = solarSet.BlockRefTrackLayer;
            string stringLayer = solarSet.PolyStringLayer;

            // Trackers con varias polys en trackLayer
            if (trackersWithMultiplePoly.Count > 0)
            {
                // Mensaje
                MessageBox.Show(
                    $"{trackersWithMultiplePoly.Count} {trackTag} were " +
                    $"discarded because they contain more than one " +
                    $"polyline in layer {trackLayer}. They will be isolated in AutoCAD.",
                    "Invalid Trackers - Multiple Polylines"
                );
                // Aislamos
                cls_00_IsolateEntities.IsolateObjects(ed, trackersWithMultiplePoly);
                // Finalizamos
                return false;
            }
            // Trackers sin polys minimas
            if (trackersToIsolate.Count > 0)
            {
                // Mensaje
                MessageBox.Show(
                    $"{trackersToIsolate.Count} {trackTag} were " +
                    $"discarded because they do not contain both " +
                    $"a polyline in {trackLayer} and a polyline in {stringLayer}. " +
                    $"They will be isolated in AutoCAD.",
                    "Invalid Trackers"
                );
                // Aislamos
                cls_00_IsolateEntities.IsolateObjects(ed, trackersToIsolate);
                // Finalizamos
                return false;
            }
            // Trackers con elevaciones inconsistentes
            if (trackersWithElevMismatch.Count > 0)
            {
                // Mensaje
                MessageBox.Show(
                    $"{trackersWithElevMismatch.Count} {trackTag} were " +
                    $"discarded because their polylines in layers " +
                    $"{trackLayer} and/or {stringLayer} have inconsistent elevations. " +
                    $"They will be isolated in AutoCAD.",
                    "Invalid Trackers - Elevation Mismatch"
                );
                // Aislamos
                cls_00_IsolateEntities.IsolateObjects(ed, trackersWithElevMismatch);
                // Finalizamos
                return false;
            }
            // return
            return true;
        }

    }
}
