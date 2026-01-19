using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictCableByInv
    {
        public static Dictionary<ObjectId, object>
        GetDictCableByInv(
            Transaction tr,
            HashSet<ObjectId> blockRefIds,
            HashSet<ObjectId> polyIds,
            string noCableValue,
            string multipleCableValue,
            out HashSet<ObjectId> usedCableIds,
            double tolerance = 1e-6
        )
        {
            Dictionary<ObjectId, object> result = new Dictionary<ObjectId, object>();
            usedCableIds = new HashSet<ObjectId>();

            // Dict de datos de polylines
            Dictionary<ObjectId, (Point3d start, Point3d end, double length)> polyData =
                new Dictionary<ObjectId, (Point3d, Point3d, double)>();
            // Iteramos polys
            foreach (ObjectId polyId in polyIds)
            {
                // Obtenemos poly
                Polyline poly = tr.GetObject(polyId, OpenMode.ForRead) as Polyline;
                // Validamos
                if (poly == null || poly.NumberOfVertices < 2) continue;

                // Almacenamos
                polyData[polyId] = (
                    poly.GetPoint3dAt(0),
                    poly.GetPoint3dAt(poly.NumberOfVertices - 1),
                    poly.Length
                );
            }

            // Iteramos blockRef
            foreach (ObjectId blockId in blockRefIds)
            {
                // Obtenemos blockRef
                BlockReference br =
                    tr.GetObject(blockId, OpenMode.ForRead) as BlockReference;
                // Validamos
                if (br == null) continue;

                // Obtenemos basepoint
                Point3d basePt = br.Position;

                List<ObjectId> connectedPolys = new List<ObjectId>();
                // Iteramos dict de polys
                foreach (var kvp in polyData)
                {
                    // Obtenemos info
                    ObjectId polyId = kvp.Key;
                    Point3d pStart = kvp.Value.start;
                    Point3d pEnd = kvp.Value.end;

                    bool connected = false;
                    // Comprobamos conexion por base point
                    if (basePt.DistanceTo(pStart) <= tolerance ||
                        basePt.DistanceTo(pEnd) <= tolerance
                    )
                    {
                        connected = true;
                    }
                    //// Comprobamos conexion por GeometryExtents
                    //else
                    //{
                    //    Extents3d ext = br.GeometricExtents;
                    //    if (PointInsideExtents(ext, pStart, tolerance) ||
                    //        PointInsideExtents(ext, pEnd, tolerance)
                    //    )
                    //    {
                    //        connected = true;
                    //    }
                    //}
                    // Validamos
                    if (connected)
                    {
                        // Almacenamos
                        connectedPolys.Add(polyId);
                        usedCableIds.Add(polyId);
                    }
                }

                // Validacion final
                if (connectedPolys.Count == 0)
                {
                    result[blockId] = noCableValue;
                }
                else if (connectedPolys.Count > 1)
                {
                    result[blockId] = multipleCableValue;
                }
                else
                {
                    // Obtenemos cable
                    ObjectId polyId = connectedPolys[0];
                    Polyline cable = tr.GetObject(polyId, OpenMode.ForRead) as Polyline;
                    // Validamos
                    if (cable == null)
                    {
                        result[blockId] = noCableValue;
                        continue;
                    }
                    // Almacenamos
                    result[blockId] = new EntityExcelRow
                    {
                        CableHandle = cable.Handle.ToString(),
                        CableLayer = cable.Layer,
                        CableLength = Math.Round(cable.Length, 2)
                    };
                }
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
