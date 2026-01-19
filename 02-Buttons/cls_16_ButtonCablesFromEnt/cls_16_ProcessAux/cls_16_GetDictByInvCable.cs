using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictByInvCable
    {
        public static Dictionary<Region, List<(ObjectId InverterId, string InverterLabel, object CableInfo)>>
        GetDictByInvCable(
            Dictionary<Region, List<DBObject>> blockRefInvByRegion,
            Dictionary<Region, string> inverterLabelByRegion,
            Dictionary<ObjectId, object> cablesByInverterBlock
        )
        {
            // Diccionario final: Region → Inversores + Label + Cable
            Dictionary<Region, List<(ObjectId InverterId, string InverterLabel, object CableInfo)>>
                inverterDataByRegion = new Dictionary<Region, List<(ObjectId, string, object)>>();
            // Construimos el diccionario
            foreach (var kvp in blockRefInvByRegion)
            {
                // Obtenemos region
                Region region = kvp.Key;
                // Obtenemos Inversor
                List<DBObject> inverterObjs = kvp.Value;

                // Inicializamos lista
                inverterDataByRegion[region] = new List<(ObjectId, string, object)>();

                // Label del inversor (por region)
                string invLabel = inverterLabelByRegion.TryGetValue(region, out string lbl)
                    ? lbl
                    : "Inversor sin etiqueta";

                // Iteramos
                foreach (DBObject obj in inverterObjs)
                {
                    // Validamos
                    if (!(obj is BlockReference br)) continue;

                    // Obtenemos id
                    ObjectId invId = br.ObjectId;

                    // Cable del inversor (por blockRef)
                    object cableInfo = cablesByInverterBlock.TryGetValue(invId, out object cable)
                        ? cable
                        : "Sin información de cable";

                    // Almacenamos
                    inverterDataByRegion[region].Add(
                        (invId, invLabel, cableInfo)
                    );
                }
            }

            // return
            return inverterDataByRegion;
        }




    }
}
