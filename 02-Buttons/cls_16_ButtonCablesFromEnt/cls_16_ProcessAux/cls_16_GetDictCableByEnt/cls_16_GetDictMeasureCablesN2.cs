using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictMeasureCablesN2
    {
        public static Dictionary<ObjectId, object> GetDictMeasureCablesN2(
            Transaction tr,
            HashSet<ObjectId> psrInvBlockIds,
            HashSet<ObjectId> psrCtBlockIds,
            HashSet<ObjectId> psrInvCabIds,
            string noCableValue,
            string multipleCableValue,
            double cableLengthCorrectionFactor,
            double cableLengthFixedAllowance,
            int cableNumberOfConductors,
            out HashSet<ObjectId> cablesConnectedToInv,
            out HashSet<ObjectId> cablesConnectedToCt,
            double tolerance = 1e-6
        )
        {
            Dictionary<ObjectId, object> result = new Dictionary<ObjectId, object>();
            cablesConnectedToInv = new HashSet<ObjectId>();
            cablesConnectedToCt = new HashSet<ObjectId>();

            // Dict de datos de polylines
            Dictionary<ObjectId, (Point3d start, Point3d end, double length)> polyData =
                new Dictionary<ObjectId, (Point3d, Point3d, double)>();
            // Iteramos polys
            foreach (ObjectId polyId in psrInvCabIds)
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

            // Detectamos CT conectados
            foreach (ObjectId ctId in psrCtBlockIds)
            {
                // Obtenemos blockRef
                BlockReference ctBr = tr.GetObject(ctId, OpenMode.ForRead) as BlockReference;
                // Validamos
                if (ctBr == null) continue;

                Extents3d ctExt = ctBr.GeometricExtents;
                // Validamos conexion
                foreach (var kvp in polyData)
                {
                    ObjectId polyId = kvp.Key;
                    Point3d pStart = kvp.Value.start;
                    Point3d pEnd = kvp.Value.end;
                    // Validamos pto dentro de CT
                    if (PointInsideExtents(ctExt, pStart, tolerance) ||
                        PointInsideExtents(ctExt, pEnd, tolerance))
                    {
                        // Almacenamos
                        cablesConnectedToCt.Add(polyId);
                    }
                }
            }

            // Detectamos Inversores conectados
            foreach (ObjectId invId in psrInvBlockIds)
            {
                // Obtenemos blockRef
                BlockReference br = tr.GetObject(invId, OpenMode.ForRead) as BlockReference;
                // Validamos
                if (br == null) continue;

                // Obtenemos basepoint
                Point3d invPt = br.Position;

                List<ObjectId> connectedPolys = new List<ObjectId>();
                // Iteramos dict de polys
                foreach (var kvp in polyData)
                {
                    // Obtenemos info
                    ObjectId polyId = kvp.Key;
                    Point3d pStart = kvp.Value.start;
                    Point3d pEnd = kvp.Value.end;

                    // Comprobamos conexion por base point
                    if (invPt.DistanceTo(pStart) <= tolerance ||
                        invPt.DistanceTo(pEnd) <= tolerance
                    )
                    {
                        // Almacenamos
                        connectedPolys.Add(polyId);
                        cablesConnectedToInv.Add(polyId);
                    }
                }

                // Validacion por Inversor
                if (connectedPolys.Count == 0)
                {
                    result[invId] = noCableValue;
                    continue;
                }
                if (connectedPolys.Count > 1)
                {
                    result[invId] = multipleCableValue;
                    continue;
                }

                // Solo 1 cable
                ObjectId cableId = connectedPolys[0];
                // Validamos si el cable también conecta a CT
                if (!cablesConnectedToCt.Contains(cableId)) continue;

                // Cable Ct-Inv invalido
                Polyline cable = tr.GetObject(cableId, OpenMode.ForRead) as Polyline;
                if (cable == null)
                {
                    result[invId] = noCableValue;
                    continue;
                }

                // Obtenemos info
                double cableLength = Math.Round(cable.Length, 2);
                double cableLengthCorrected = cableLength * cableLengthCorrectionFactor;
                double cableLengthCorrectedTotal = cableLengthCorrected + cableLengthFixedAllowance;
                double totalInstalledCableLength = cableLengthCorrectedTotal * cableNumberOfConductors;

                // Almacenamos
                result[invId] = new EntityExcelRow
                {
                    CableHandle = cable.Handle.ToString(),
                    CableLayer = cable.Layer,
                    CableLength = cableLength,
                    CableLengthCorrectionFactor = cableLengthCorrectionFactor,
                    CableExtraLength = cableLengthCorrected,
                    CableLengthFixedAllowance = cableLengthFixedAllowance,
                    CableLengthCorrectedTotal = cableLengthCorrectedTotal,
                    NumberOfConductors = cableNumberOfConductors,
                    TotalInstalledCableLength = totalInstalledCableLength
                };
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
