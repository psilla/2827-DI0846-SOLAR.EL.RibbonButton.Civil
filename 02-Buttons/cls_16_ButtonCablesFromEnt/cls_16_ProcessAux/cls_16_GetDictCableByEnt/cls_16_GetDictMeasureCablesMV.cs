using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictMeasureCablesMV
    {
        public static Dictionary<ObjectId, EntityExcelRow> GetDictMeasureCablesMV(
            Transaction tr,
            IEnumerable<ObjectId> psrCtCabIds,
            HashSet<ObjectId> psrEstBlockIds,
            Dictionary<ObjectId, ObjectId> labelByCtDict,
            double cableLengthCorrectionFactor,
            double cableLengthFixedAllowance,
            int cableNumberOfConductors,
            out HashSet<ObjectId> invalidCableIds,
            out HashSet<ObjectId> connectedCtBlockIds,
            out HashSet<ObjectId> connectedEstBlockIds,
            out HashSet<ObjectId> connectedEstCableIds
        )
        {
            Dictionary<ObjectId, EntityExcelRow> result = 
                new Dictionary<ObjectId, EntityExcelRow>();

            invalidCableIds = new HashSet<ObjectId>();
            connectedCtBlockIds = new HashSet<ObjectId>();
            connectedEstBlockIds = new HashSet<ObjectId>();
            connectedEstCableIds = new HashSet<ObjectId>();

            // Invertimos el diccionario: block → labels
            Dictionary<ObjectId, List<ObjectId>> blockToLabels = 
                new Dictionary<ObjectId, List<ObjectId>>();

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
            foreach (ObjectId cableId in psrCtCabIds)
            {
                // Obtenemos poly
                Polyline pl = tr.GetObject(cableId, OpenMode.ForRead) as Polyline;
                // Validamos
                if (pl == null) continue;

                // Obtenemos extremos
                Point3d pStart = pl.StartPoint;
                Point3d pEnd = pl.EndPoint;

                // Calculamos longitudes
                double cableLength = pl.Length;
                cableLength = System.Math.Round(cableLength, 2);
                double cableLengthCorrected = cableLength * cableLengthCorrectionFactor;
                double cableLengthCorrectedTotal = cableLengthCorrected + cableLengthFixedAllowance;
                double totalInstalledCableLength = cableLengthCorrectedTotal * cableNumberOfConductors;

                // Almacenamos
                EntityExcelRow row = new EntityExcelRow
                {
                    // Cable
                    CableHandle = pl.Handle.ToString(),
                    CableLayer = pl.Layer,
                    CableLength = cableLength,
                    CableLengthCorrectionFactor = cableLengthCorrectionFactor,
                    CableExtraLength = cableLengthCorrected,
                    CableLengthFixedAllowance = cableLengthFixedAllowance,
                    CableLengthCorrectedTotal = cableLengthCorrectedTotal,
                    NumberOfConductors = cableNumberOfConductors,
                    TotalInstalledCableLength = totalInstalledCableLength
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
                        connectedCtBlockIds.Add(kvp.Key);
                    }
                    if (PointInsideExtents(ext, pEnd))
                    {
                        endLabel = kvp.Value.First();
                        connectedCtBlockIds.Add(kvp.Key);
                    }
                }
                // Validamos
                if (startLabel.HasValue)
                    // Almacenamos
                    row.RelatedLabels.Add(startLabel.Value);

                // Validamos
                if (endLabel.HasValue && endLabel != startLabel)
                    // Almacenamos
                    row.RelatedLabels.Add(endLabel.Value);

                // Caso: ningún extremo conectado a CT
                if (row.RelatedLabels.Count == 0)
                {
                    invalidCableIds.Add(cableId);
                    continue;
                }

                // Caso: solo un extremo conectado
                if (row.RelatedLabels.Count == 1)
                {
                    // Determinamos el punto libre
                    Point3d freePoint =
                        startLabel.HasValue && !endLabel.HasValue ? pEnd :
                        !startLabel.HasValue && endLabel.HasValue ? pStart :
                        Point3d.Origin;
                    // Validamos si el punto libre cae dentro de alguna Subestacion
                    ObjectId? estId = GetEstBlockContainingPoint(
                        tr, psrEstBlockIds, freePoint
                    );
                    // Validamos
                    if (!estId.HasValue)
                    {
                        invalidCableIds.Add(cableId);
                        continue;
                    }
                    connectedEstBlockIds.Add(estId.Value);
                    connectedEstCableIds.Add(cableId);
                }
                // Almacenamos
                result[cableId] = row;
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

        private static ObjectId? GetEstBlockContainingPoint(
            Transaction tr,
            HashSet<ObjectId> estBlockRefIds,
            Point3d point
        )
        {
            // Iteramos
            foreach (ObjectId id in estBlockRefIds)
            {
                // Obtenemos bloque
                BlockReference br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                // Validamos
                if (br == null) continue;
                // Validamos
                if (PointInsideExtents(br.GeometricExtents, point)) return id;
            }
            // Por defecto
            return null;
        }





    }
}
