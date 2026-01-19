using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictCableByString
    {
        public static Dictionary<Region, object> GetDictCableByString(
            Transaction tr,
            Database db,
            List<Region> validRegionString,
            Dictionary<Handle, Handle> dictPolyToRegionString,
            HashSet<ObjectId> psrStringCabIds,
            string noCableValue, 
            string multipleCableValue,
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
                    // Almacenamos
                    result[region] = new EntityExcelRow
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



    }
}
