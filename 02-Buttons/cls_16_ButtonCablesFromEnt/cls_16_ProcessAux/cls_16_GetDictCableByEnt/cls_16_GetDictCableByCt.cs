using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictCableByCt
    {
        public static Dictionary<ObjectId, CableCtInfo> GetDictCableByCt(
            Transaction tr,
            IEnumerable<ObjectId> cablePolyIds,
            Dictionary<ObjectId, ObjectId> labelByCtDict,
            string defaultValue
        )
        {
            var result = new Dictionary<ObjectId, CableCtInfo>();

            // Invertimos el diccionario: block → labels
            Dictionary<ObjectId, List<ObjectId>> blockToLabels = new Dictionary<ObjectId, List<ObjectId>>();

            // Iteramos
            foreach (var kvp in labelByCtDict)
            {
                // Validamos
                if (!blockToLabels.ContainsKey(kvp.Value))
                    blockToLabels[kvp.Value] = new List<ObjectId>();
                // Almacenamos
                blockToLabels[kvp.Value].Add(kvp.Key);
            }

            // Iteramos polys
            foreach (ObjectId cableId in cablePolyIds)
            {
                // Obtenemos poly
                Polyline pl = tr.GetObject(cableId, OpenMode.ForRead) as Polyline;
                // Validamos
                if (pl == null) continue;

                // Obtenemos extremos
                Point3d pStart = pl.StartPoint;
                Point3d pEnd = pl.EndPoint;

                // Almacenamos
                CableCtInfo cableInfo = new CableCtInfo
                {
                    CableLength = pl.Length
                };

                ObjectId? startLabel = null;
                ObjectId? endLabel = null;
                // Iteramos blockRef
                foreach (var kvp in blockToLabels)
                {
                    // Obtenemos blockRef
                    BlockReference br = tr.GetObject(kvp.Key, OpenMode.ForRead) as BlockReference;
                    // Validamos
                    if (br == null) continue;

                    Extents3d ext = br.GeometricExtents;
                    // Comprobamos ptos extremos cables
                    if (PointInsideExtents(ext, pStart))
                    {
                        startLabel = kvp.Value.First();
                    }
                    if (PointInsideExtents(ext, pEnd))
                    {
                        endLabel = kvp.Value.First();
                    }
                }
                // Validamos
                if (startLabel.HasValue)
                    // Almacenamos
                    cableInfo.RelatedLabels.Add(startLabel.Value);

                // Validamos
                if (endLabel.HasValue && endLabel != startLabel)
                    // Almacenamos
                    cableInfo.RelatedLabels.Add(endLabel.Value);

                // Caso: solo un extremo conectado
                if (cableInfo.RelatedLabels.Count == 1)
                {
                    // Marcador lógico: Subestacion
                }
                // Almacenamos
                result[cableId] = cableInfo;
            }
            // return
            return result;
        }

        private static bool PointInsideExtents(Extents3d ext, Point3d pt, double tol = 1e-6)
        {
            return
                pt.X >= ext.MinPoint.X - tol && pt.X <= ext.MaxPoint.X + tol &&
                pt.Y >= ext.MinPoint.Y - tol && pt.Y <= ext.MaxPoint.Y + tol &&
                pt.Z >= ext.MinPoint.Z - tol && pt.Z <= ext.MaxPoint.Z + tol;
        }


    }
}
