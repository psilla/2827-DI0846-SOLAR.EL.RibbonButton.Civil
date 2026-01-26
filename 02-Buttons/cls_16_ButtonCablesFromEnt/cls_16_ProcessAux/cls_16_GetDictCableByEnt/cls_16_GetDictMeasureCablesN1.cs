using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictMeasureCablesN1
    {
        public static Dictionary<Region, object> GetDictMeasureCablesN1(
            Transaction tr,
            Database db,
            List<Region> validRegionString,
            Dictionary<Handle, Handle> dictPolyToRegionString,
            HashSet<ObjectId> psrStringCabIds,
            string noCableValue, 
            string multipleCableValue,
            double cableLengthCorrectionFactor,
            double cableLengthFixedAllowance,
            int cableNumberOfConductors,
            out HashSet<ObjectId> usedCableIds
        )
        {
            Dictionary<Region, object> result = new Dictionary<Region, object>();
            usedCableIds = new HashSet<ObjectId>();

            // Invertimos: Region → Poly original
            Dictionary<Handle, Handle> regionToPoly =
                dictPolyToRegionString.ToDictionary(
                    kvp => kvp.Value,
                    kvp => kvp.Key
                );

            // Iteramos regiones
            foreach (Region region in validRegionString)
            {
                List<Polyline> cablesAssigned = new List<Polyline>();
                // Validamos poly
                if (!regionToPoly.TryGetValue(region.Handle, out Handle polyHandle))
                {
                    // Valor por defecto
                    result[region] = noCableValue;
                    // Obviamos
                    continue;
                }

                // Obtenemos poly original desde region
                Polyline polyFromRegion = 
                    tr.GetObject(db.GetObjectId(false, polyHandle, 0),OpenMode.ForRead) as Polyline;
                // Validamos
                if (polyFromRegion == null)
                {
                    // Valor por defecto
                    result[region] = noCableValue;
                    // Obviamos
                    continue;
                }

                // Iteramos cables
                foreach (ObjectId cabId in psrStringCabIds)
                {
                    // Obtenemos poly
                    Polyline cablePoly = tr.GetObject(cabId, OpenMode.ForRead) as Polyline;
                    // Validamos
                    if (cablePoly == null) continue;

                    // Obtenemos intersecciones
                    Point3dCollection inters = new Point3dCollection();
                    polyFromRegion.IntersectWith(
                        cablePoly, Intersect.OnBothOperands, inters, IntPtr.Zero, IntPtr.Zero
                    );

                    // Validamos si intersecta en 1 solo punto a la region
                    if (inters.Count == 1)
                    {
                        // Almacenamos
                        cablesAssigned.Add(cablePoly);
                        usedCableIds.Add(cablePoly.ObjectId);
                    }
                }

                // Evaluamos resultado
                if (cablesAssigned.Count == 0)
                {
                    result[region] = noCableValue;
                }
                else if (cablesAssigned.Count > 1)
                {
                    result[region] = multipleCableValue;
                }
                // Exactamente 1 poly
                else
                {
                    // Obtenemos cable
                    Polyline cable = cablesAssigned[0];
                    double cableLength = Math.Round(cable.Length, 2);
                    double cableLengthCorrected = cableLength * cableLengthCorrectionFactor;
                    double cableLengthCorrectedTotal = cableLengthCorrected + cableLengthFixedAllowance;
                    double totalInstalledCableLength = cableLengthCorrectedTotal * cableNumberOfConductors;
                    // Almacenamos
                    result[region] = new EntityExcelRow
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
            }
            // return
            return result;
        }

        public static Dictionary<Region, object> GetDictMeasureCablesN1BIS(
            Transaction tr,
            Database db,
            List<Region> validRegionString,
            Dictionary<Handle, Handle> dictPolyToRegionString,
            HashSet<ObjectId> psrStringCabIds,
            string noCableValue,
            string multipleCableValue,
            double cableLengthCorrectionFactor,
            double cableLengthFixedAllowance,
            int cableNumberOfConductors,
            out HashSet<ObjectId> usedCableIds
        )
        {
            Dictionary<Region, object> result = new Dictionary<Region, object>();
            usedCableIds = new HashSet<ObjectId>();

            // Invertimos: Region → Poly original
            Dictionary<Handle, Handle> regionToPoly =
                dictPolyToRegionString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            // Cacheamos TODOS los cables una sola vez
            List<Polyline> cablePolys = psrStringCabIds
                .Select(id => tr.GetObject(id, OpenMode.ForRead) as Polyline)
                .Where(p => p != null)
                .ToList();

            // Iteramos regiones
            foreach (Region region in validRegionString)
            {
                // Validamos poly asociada a la región
                if (!regionToPoly.TryGetValue(region.Handle, out Handle polyHandle))
                {
                    result[region] = noCableValue;
                    continue;
                }

                Polyline polyFromRegion =
                    tr.GetObject(db.GetObjectId(false, polyHandle, 0), OpenMode.ForRead) as Polyline;

                if (polyFromRegion == null)
                {
                    result[region] = noCableValue;
                    continue;
                }

                Extents3d regionExt = polyFromRegion.GeometricExtents;

                Polyline foundCable = null;
                int cableCount = 0;

                // Iteramos cables (ya cacheados)
                foreach (Polyline cablePoly in cablePolys)
                {
                    // Filtro rápido por BoundingBox
                    if (!ExtentsIntersect(regionExt, cablePoly.GeometricExtents)) continue;

                    Point3dCollection inters = new Point3dCollection();
                    polyFromRegion.IntersectWith(
                        cablePoly,
                        Intersect.OnBothOperands,
                        inters,
                        IntPtr.Zero,
                        IntPtr.Zero
                    );

                    // Solo nos interesan intersecciones en 1 punto
                    if (inters.Count == 1)
                    {
                        cableCount++;
                        usedCableIds.Add(cablePoly.ObjectId);

                        if (cableCount == 1)
                        {
                            foundCable = cablePoly;
                        }
                        else
                        {
                            // Ya sabemos que hay múltiples
                            break;
                        }
                    }
                }

                // Evaluamos resultado
                if (cableCount == 0)
                {
                    result[region] = noCableValue;
                }
                else if (cableCount > 1)
                {
                    result[region] = multipleCableValue;
                }
                else
                {
                    // Exactamente 1 cable
                    double cableLength = Math.Round(foundCable.Length, 2);
                    double cableLengthCorrected = cableLength * cableLengthCorrectionFactor;
                    double cableLengthCorrectedTotal = cableLengthCorrected + cableLengthFixedAllowance;
                    double totalInstalledCableLength = cableLengthCorrectedTotal * cableNumberOfConductors;

                    result[region] = new EntityExcelRow
                    {
                        CableHandle = foundCable.Handle.ToString(),
                        CableLayer = foundCable.Layer,
                        CableLength = cableLength,
                        CableLengthCorrectionFactor = cableLengthCorrectionFactor,
                        CableExtraLength = cableLengthCorrected,
                        CableLengthFixedAllowance = cableLengthFixedAllowance,
                        CableLengthCorrectedTotal = cableLengthCorrectedTotal,
                        NumberOfConductors = cableNumberOfConductors,
                        TotalInstalledCableLength = totalInstalledCableLength
                    };
                }
            }

            return result;
        }

        private static bool ExtentsIntersect(Extents3d a, Extents3d b)
        {
            return
                a.MinPoint.X <= b.MaxPoint.X && a.MaxPoint.X >= b.MinPoint.X &&
                a.MinPoint.Y <= b.MaxPoint.Y && a.MaxPoint.Y >= b.MinPoint.Y &&
                a.MinPoint.Z <= b.MaxPoint.Z && a.MaxPoint.Z >= b.MinPoint.Z;
        }





    }
}
