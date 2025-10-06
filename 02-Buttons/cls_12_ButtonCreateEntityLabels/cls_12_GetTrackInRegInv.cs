using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Civil.GetEntities;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_GetTrackInRegInv
    {
        public static List<Entity> GetTrackInRegInv(
            Transaction tr,
            Region invRegion,
            HashSet<ObjectId> trackersIds,
            string trackTag,
            StringBuilder infoRegiones
        )
        {
            // Mostramos
            infoRegiones.AppendLine($"\t- Inverter Region Handle: {invRegion.Handle}");

            // Diccionario de Trackers por region
            Dictionary<string, List<DBObject>> dictTrackersByRegion =
                cls_00_GetBlockRefOrPolyByRegion.GetBlockRefOrPolyByRegion(
                    tr, invRegion, trackersIds, true
                );

            // Obtenemos lista de Trackers por region 
            List<DBObject> trackerList =
                dictTrackersByRegion.SelectMany(kv => kv.Value).ToList();

            // Convertimos a entidades
            List<Entity> trackerEntities = trackerList.OfType<Entity>().ToList();

            // Mostramos
            infoRegiones.AppendLine($"\t\t• Total {trackTag}: {trackerEntities.Count}");

            // return
            return trackerEntities;
        }




    }
}
