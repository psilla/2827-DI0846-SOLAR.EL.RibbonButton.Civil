using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictMeasureCablesN2
    {
        private static void ShowMeasureCablesN2Summary(
            Dictionary<ObjectId, object> result,
            HashSet<ObjectId> cablesConnectedToInv,
            HashSet<ObjectId> cablesConnectedToCt
        )
        {
            // -----------------------------
            // Info
            // -----------------------------

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("📌 Measure Cables N2 Summary");
            sb.AppendLine();

            // Iteramos
            foreach (var kvp in result)
            {
                ObjectId invId = kvp.Key;
                object value = kvp.Value;

                sb.AppendLine($"🔹 Inverter: {invId.Handle}");

                // Caso EntityExcelRow
                if (value is EntityExcelRow row)
                {
                    sb.AppendLine($"   - Cable Handle: {row.CableHandle}");
                    sb.AppendLine($"   - Cable Layer: {row.CableLayer}");

                    sb.AppendLine($"   - Cable Length: {row.CableLength}");
                    sb.AppendLine($"   - Correction Factor: {row.CableLengthCorrectionFactor}");

                    sb.AppendLine($"   - Extra Length: {row.CableExtraLength}");
                    sb.AppendLine($"   - Fixed Allowance: {row.CableLengthFixedAllowance}");

                    sb.AppendLine($"   - Corrected Total: {row.CableLengthCorrectedTotal}");

                    sb.AppendLine($"   - Number Of Conductors: {row.NumberOfConductors}");
                    sb.AppendLine($"   - Total Installed Length: {row.TotalInstalledCableLength}");
                }
                // Caso string
                else
                {
                    sb.AppendLine($"   - Result: {value}");
                }

                sb.AppendLine();
            }

            // -----------------------------
            // Cables conectados a inversores
            // -----------------------------

            sb.AppendLine("📌 Cables Connected To Inverters");

            if (cablesConnectedToInv != null && cablesConnectedToInv.Count > 0)
            {
                foreach (ObjectId id in cablesConnectedToInv)
                {
                    sb.AppendLine($"   - {id.Handle}");
                }
            }
            else
            {
                sb.AppendLine("   - NONE");
            }

            sb.AppendLine();

            // -----------------------------
            // Cables conectados a CT
            // -----------------------------

            sb.AppendLine("📌 Cables Connected To CT");

            if (cablesConnectedToCt != null && cablesConnectedToCt.Count > 0)
            {
                foreach (ObjectId id in cablesConnectedToCt)
                {
                    sb.AppendLine($"   - {id.Handle}");
                }
            }
            else
            {
                sb.AppendLine("   - NONE");
            }

            // -----------------------------
            // Mostrar
            // -----------------------------

            ShowStringBuilder.ShowInfo(
                "📌 Measure Cables N2 Summary",
                sb.ToString()
            );
        }

        private static void ShowMeasureCablesN2Debug(
            Transaction tr,
            Dictionary<ObjectId, object> result
        )
        {
            StringBuilder sb = new StringBuilder();

            // -----------------------------
            // Resultado por inversor
            // -----------------------------

            sb.AppendLine("📌 RESULT BY INVERTER");
            sb.AppendLine();

            foreach (var kvp in result)
            {
                ObjectId invId = kvp.Key;

                BlockReference inv =
                    tr.GetObject(
                        invId,
                        OpenMode.ForRead
                    ) as BlockReference;

                string invHandle =
                    inv != null
                        ? inv.Handle.ToString()
                        : "NULL";

                sb.AppendLine(
                    $"🔹 Inverter: {invHandle}"
                );

                if (kvp.Value is EntityExcelRow row)
                {
                    sb.AppendLine(
                        $"   CableHandle: {row.CableHandle}"
                    );

                    sb.AppendLine(
                        $"   CableLayer: {row.CableLayer}"
                    );
                }
                else
                {
                    sb.AppendLine(
                        $"   Result: {kvp.Value}"
                    );
                }

                sb.AppendLine();
            }

            // -----------------------------
            // Mostrar
            // -----------------------------

            ShowStringBuilder.ShowInfo(
                "📌 Measure Cables N2 Summary",
                sb.ToString()
            );
        }

        private static Dictionary<ObjectId, (Point3d start, Point3d end, double length)> GetPolylineData(
            Transaction tr,
            HashSet<ObjectId> polyIds
        )
        {
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
                    poly.GetPoint3dAt(0), poly.GetPoint3dAt(poly.NumberOfVertices - 1), poly.Length
                );
            }

            // return
            return polyData;
        }

        private static HashSet<ObjectId> GetCablesConnectedToCt(
            Transaction tr,
            HashSet<ObjectId> psrCtBlockIds,
            Dictionary<ObjectId, (Point3d start, Point3d end, double length)> polyData,
            double tolerance = 1e-6
        )
        {
            HashSet<ObjectId> cablesConnectedToCt = new HashSet<ObjectId>();
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
                    // Obtenemos info
                    ObjectId cableId = kvp.Key;
                    Point3d pStart = kvp.Value.start;
                    Point3d pEnd = kvp.Value.end;

                    // Validamos pto dentro de CT
                    if (PointInsideExtents(ctExt, pStart, tolerance) ||
                        PointInsideExtents(ctExt, pEnd, tolerance))
                    {
                        // Almacenamos
                        cablesConnectedToCt.Add(cableId);
                    }
                }
            }

            // return
            return cablesConnectedToCt;
        }

        private static bool PointInsideCtPolyline(
             Point3d pt,
             Polyline poly
         )
        {
            // Validamos
            if (poly == null || !poly.Closed) return false;

            DBObjectCollection curves = new DBObjectCollection();
            // Añadimos
            curves.Add(poly);

            DBObjectCollection regions = Region.CreateFromCurves(curves);

            // Validamos
            if (regions.Count == 0) return false;

            using (Region region = regions[0] as Region)
            {
                // Validamos
                if (region == null) return false;

                // return
                return cls_00_GetElemByRegionByBrep.PointByRegionByBrep(
                    pt, region
                );
            }
        }

        private static Dictionary<ObjectId, string> GetCablesConnectedToCtByInnerPolyLayer(
            Transaction tr,
            HashSet<ObjectId> psrCtBlockIds,
            Dictionary<ObjectId, (Point3d start, Point3d end, double length)> polyData,
            List<string> validPolyLayers,
            double tolerance = 1e-6
        )
        {
            Dictionary<ObjectId, string> cableLayerByCableId = new Dictionary<ObjectId, string>();
            // Iteramos CTs
            foreach (ObjectId ctId in psrCtBlockIds)
            {
                // -----------------------------
                // Obtener BlockRef
                // -----------------------------

                BlockReference ctBr = tr.GetObject(ctId, OpenMode.ForRead) as BlockReference;
                // Validamos
                if (ctBr == null) continue;

                // -----------------------------
                // Obtener polys internas validas
                // -----------------------------

                List<Polyline> innerPolys = cls_00_GetNestedPolysInBlockRef.GetNestedPolysInBlockRef(
                    ctBr, tr, validPolyLayers
                );
                // Validamos
                if (innerPolys.Count == 0) continue;

                // -----------------------------
                // Iteramos cables
                // -----------------------------

                foreach (var kvp in polyData)
                {
                    // -----------------------------
                    // Obtener info
                    // -----------------------------

                    ObjectId cableId = kvp.Key;
                    Point3d pStart = kvp.Value.start;
                    Point3d pEnd = kvp.Value.end;

                    // -----------------------------
                    // Iteramos polys internas
                    // -----------------------------

                    bool connected = false;
                    // Iteramos 
                    foreach (Polyline innerPoly in innerPolys)
                    {
                        // Transformar poly al espacio mundo
                        Polyline polyWorld = innerPoly.GetTransformedCopy(
                            ctBr.BlockTransform
                        ) as Polyline;
                        // Validamos
                        if (polyWorld == null) continue;

                        // Verificar conexion
                        if (
                            PointInsideCtPolyline(pStart, polyWorld) || 
                            PointInsideCtPolyline(pEnd, polyWorld
                        )
                        )
                        {
                            // Almacenamos
                            cableLayerByCableId[cableId] = innerPoly.Layer;
                            // Aplicamos
                            connected = true;
                            break;
                        }
                    }

                    // Ya conectado
                    if (connected) continue;
                }
            }

            // return
            return cableLayerByCableId;
        }

        private static List<ObjectId> GetConnectedPolysToInverter(
            Transaction tr,
            ObjectId invId,
            Dictionary<ObjectId, (Point3d start, Point3d end, double length)> polyData,
            HashSet<ObjectId> cablesConnectedToInv,
            double tolerance = 1e-6
        )
        {
            List<ObjectId> connectedPolys = new List<ObjectId>();

            // Obtenemos inversor
            BlockReference br = tr.GetObject(invId, OpenMode.ForRead) as BlockReference;
            // Validamos
            if (br == null) return connectedPolys;

            Point3d invPt = br.Position;
            // Iteramos polys
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

            // return
            return connectedPolys;
        }

        private static bool TryGetSingleValidCable(
            Transaction tr,
            ObjectId invId,
            List<ObjectId> connectedPolys,
            HashSet<ObjectId> cablesConnectedToCt,
            Dictionary<ObjectId, object> result,
            string noCableValue,
            string multipleCableValue,
            out Polyline cable
        )
        {
            cable = null;

            // No cables
            if (connectedPolys.Count == 0)
            {
                result[invId] = noCableValue;
                return false;
            }

            // Multiple cables
            if (connectedPolys.Count > 1)
            {
                result[invId] = multipleCableValue;
                return false;
            }

            // Solo 1
            ObjectId cableId = connectedPolys[0];

            // Validamos conexion CT
            if (!cablesConnectedToCt.Contains(cableId))
            {
                result[invId] = noCableValue;
                return false;
            }

            // Obtenemos poly
            cable = tr.GetObject(cableId, OpenMode.ForRead) as Polyline;

            // Validamos
            if (cable == null)
            {
                result[invId] = noCableValue;
                return false;
            }

            // return
            return true;
        }

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

            // -----------------------------
            // Obtener datos polys
            // -----------------------------

            Dictionary<ObjectId, (Point3d start, Point3d end, double length)> polyData =
                GetPolylineData(tr, psrInvCabIds);

            // -----------------------------
            // Obtener cables conectados CT
            // -----------------------------

            cablesConnectedToCt = GetCablesConnectedToCt(
                tr, psrCtBlockIds, polyData, tolerance
            );

            // -----------------------------
            // Detectar Inversores conectados
            // -----------------------------

            foreach (ObjectId invId in psrInvBlockIds)
            {
                // -----------------------------
                // Obtener polys conectadas
                // -----------------------------

                List<ObjectId> connectedPolys = GetConnectedPolysToInverter(
                    tr, invId, polyData, cablesConnectedToInv, tolerance
                );

                // -----------------------------
                // Obtener cable valido
                // -----------------------------

                if (!TryGetSingleValidCable(
                    tr, invId, connectedPolys, cablesConnectedToCt, result,
                    noCableValue, multipleCableValue, out Polyline cable
                )) continue;

                // -----------------------------
                // Obtener info
                // -----------------------------

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

            bool showInfo = false;
            // Debug
            if (showInfo)
                ShowMeasureCablesN2Summary(
                    result, cablesConnectedToInv, cablesConnectedToCt
                );

            // return
            return result;
        }

        public static Dictionary<ObjectId, object> GetDictMeasureCablesN2(
            Transaction tr,
            HashSet<ObjectId> psrInvBlockIds,
            HashSet<ObjectId> psrCtBlockIds,
            HashSet<ObjectId> psrInvCabIds,
            string noCableValue,
            string multipleCableValue,
            List<string> validPolyLayers,
            out HashSet<ObjectId> cablesConnectedToInv,
            out HashSet<ObjectId> cablesConnectedToCt,
            double tolerance = 1e-6
        )
        {
            Dictionary<ObjectId, object> result = new Dictionary<ObjectId, object>();
            cablesConnectedToInv = new HashSet<ObjectId>();

            // -----------------------------
            // Obtener datos polys
            // -----------------------------

            Dictionary<ObjectId, (Point3d start, Point3d end, double length)> polyData =
                GetPolylineData(tr, psrInvCabIds);

            // -----------------------------
            // Obtener cables conectados CT
            // -----------------------------

            Dictionary<ObjectId, string> cableLayerByCableId = GetCablesConnectedToCtByInnerPolyLayer(
                tr, psrCtBlockIds, polyData, validPolyLayers, tolerance
            );
            // Obtener Ids
            cablesConnectedToCt = cableLayerByCableId.Keys.ToHashSet();

            // -----------------------------
            // Detectar Inversores conectados
            // -----------------------------

            foreach (ObjectId invId in psrInvBlockIds)
            {
                // -----------------------------
                // Obtener polys conectadas
                // -----------------------------

                List<ObjectId> connectedPolys = GetConnectedPolysToInverter(
                    tr, invId, polyData, cablesConnectedToInv, tolerance
                );

                // -----------------------------
                // Obtener cable valido
                // -----------------------------

                if (!TryGetSingleValidCable(
                    tr, invId, connectedPolys, cablesConnectedToCt, result,
                    noCableValue, multipleCableValue, out Polyline cable
                )) continue;

                // -----------------------------
                // Obtener layer CT interna
                // -----------------------------

                string polyInnerLayer = cableLayerByCableId[cable.ObjectId];

                // Almacenamos
                result[invId] = new EntityExcelRow
                {
                    CableHandle = cable.Handle.ToString(),
                    CableLayer = polyInnerLayer
                };
            }

            bool showInfo = false;
            // Debug
            if (showInfo)
                ShowMeasureCablesN2Debug(tr, result);

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
