using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictLabelByCt
    {
        public static Dictionary<ObjectId, ObjectId> GetDictLabelByCt(
            Transaction tr,
            HashSet<ObjectId> labelIds,
            HashSet<ObjectId> blockIds
        )
        {
            Dictionary<ObjectId, ObjectId> result = new Dictionary<ObjectId, ObjectId>();

            Dictionary<ObjectId, Point3d> blockPositions = new Dictionary<ObjectId, Point3d>();
            // Iteramos BlockRef
            foreach (ObjectId blockId in blockIds)
            {
                // Obtenemos entidad
                Entity ent = tr.GetObject(blockId, OpenMode.ForRead) as Entity;
                // Validamos
                if (ent is BlockReference br)
                {
                    // Almacenamos
                    blockPositions[blockId] = br.Position;
                }
            }

            // Iteramos Labels
            foreach (ObjectId labelId in labelIds)
            {
                // Obtenemos entidad
                Entity ent = tr.GetObject(labelId, OpenMode.ForRead) as Entity;

                Point3d? labelPoint = null;
                // Calculamos posicion
                if (ent is DBText dbText)
                    labelPoint = dbText.Position;
                else if (ent is MText mText)
                    labelPoint = mText.Location;
                // Validamos
                if (!labelPoint.HasValue) continue;

                double minDist = double.MaxValue;
                ObjectId closestBlockId = ObjectId.Null;
                // Iteramos posiciones
                foreach (var kvp in blockPositions)
                {
                    // Obtenemos distancia
                    double dist = labelPoint.Value.DistanceTo(kvp.Value);
                    // Validamos
                    if (dist < minDist)
                    {
                        // Almacenamos
                        minDist = dist;
                        closestBlockId = kvp.Key;
                    }
                }
                // Validamos
                if (!closestBlockId.IsNull)
                {
                    // Almacenamos
                    result[labelId] = closestBlockId;
                }
            }
            // return
            return result;
        }


    }
}
