using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using TYPSA.SharedLib.Autocad.EntitiesInsertionPoint;
using TYPSA.SharedLib.UserForms;
using TYPSA.SharedLib.Autocad.GetEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_ElemByRegionByInter
    {
        public static void ShowAssignEntitiesByInterSummary(
            Dictionary<Region, List<DBObject>> regionData,
            HashSet<ObjectId> alreadyAssigned,
            int addedByIntersection
        )
        {
            // -----------------------------
            // Info
            // -----------------------------

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("📌 Assign Entities By Intersection Summary");
            sb.AppendLine();

            int regionIndex = 1;

            // Iteramos regiones
            foreach (var kvp in regionData)
            {
                Region region = kvp.Key;
                List<DBObject> entities = kvp.Value;

                sb.AppendLine($"🔹 Region {regionIndex++}");
                sb.AppendLine($"   - Region Handle: {region.Handle}");
                sb.AppendLine($"   - Entities Count: {(entities != null ? entities.Count : 0)}");

                // Entidades
                if (entities != null && entities.Count > 0)
                {
                    foreach (DBObject obj in entities)
                    {
                        sb.AppendLine($"      • {obj.ObjectId.Handle}");
                    }
                }

                sb.AppendLine();
            }

            // -----------------------------
            // Assigned entities
            // -----------------------------

            sb.AppendLine("📌 Assigned Entity Ids");

            if (alreadyAssigned != null && alreadyAssigned.Count > 0)
            {
                foreach (ObjectId id in alreadyAssigned)
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
            // Total
            // -----------------------------

            sb.AppendLine($"📌 Added By Intersection: {addedByIntersection}");

            // -----------------------------
            // Mostrar
            // -----------------------------

            ShowStringBuilder.ShowInfo(
                "📌 Assign Entities By Intersection Summary",
                sb.ToString()
            );
        }

        public static int AssignEntitiesByInter(
            Transaction tr,
            IEnumerable<ObjectId> allEntities,
            HashSet<ObjectId> alreadyAssigned,
            IEnumerable<Region> allRegions,
            Dictionary<Region, List<DBObject>> regionData,
            double radTolerance = 20,
            bool boolByGeometryExt = true
        )
        {
            int addedByIntersection = 0;
            // Iteramos
            foreach (ObjectId objId in allEntities.Except(alreadyAssigned))
            {
                // Aplicamos
                EntityByRegionByInter(
                    tr, objId, allRegions, regionData, alreadyAssigned,
                    ref addedByIntersection, radTolerance, boolByGeometryExt
                );
            }
            // return
            return addedByIntersection;
        }

        public static void EntityByRegionByInter(
            Transaction tr,
            ObjectId dbObjId,
            IEnumerable<Region> allRegions,
            Dictionary<Region, List<DBObject>> regionData,
            HashSet<ObjectId> entityInRegion,
            ref int entityAddedInSecondIteration,
            double radTolerance = 20,
            bool boolByGeometryExt = true
        )
        {
            // Obtener el objeto DBObject desde el ObjectId
            DBObject dbObj = tr.GetObject(dbObjId, OpenMode.ForRead);
            // Validamos
            if (!(dbObj is Entity ent)) return;

            // Obtener el punto de inserción de la entidad (bloque, texto, mtext)
            Point3d punto = cls_00_GetEntityInsertionPoint.GetEntityInsertionPoint(ent);

            // Lista de regiones donde se detecta intersección
            List<Region> intersectedRegions = new List<Region>();

            // Verificamos en qué región encaja
            foreach (var region in allRegions)
            {
                double radio = radTolerance;
                // En caso de analizar la geometria
                if (boolByGeometryExt)
                {
                    // try
                    try
                    {
                        // Obtenemos geometría
                        Extents3d ext = ent.GeometricExtents;
                        // Sacamos altura
                        double altura = ext.MaxPoint.Y - ext.MinPoint.Y;
                        // Si la altura es razonable, la usamos como radio del circulo
                        if (altura > 1e-3)
                            radio = altura / 2;
                    }
                    // catch
                    catch { }
                }

                // Creamos un circulo centrado en el Base Point de la entidad
                using (Circle circulo = new Circle(punto, Vector3d.ZAxis, radio))
                {
                    // Lista de ptos de interseccion
                    Point3dCollection ptosInter = new Point3dCollection();

                    // Verificamos la intersección del círculo con la región
                    region.IntersectWith(
                        circulo, Intersect.OnBothOperands, ptosInter, IntPtr.Zero, IntPtr.Zero
                    );

                    // Si tiene al menos una intersección con esta región, la añadimos a la lista
                    if (ptosInter.Count > 0)
                    {
                        intersectedRegions.Add(region);
                    }
                }
            }

            // Caso1: Interseca con una region
            if (intersectedRegions.Count == 1)
            {
                // Obtenemos la region
                Region assignedRegion = intersectedRegions[0];
                // Validamos
                if (!regionData.TryGetValue(assignedRegion, out List<DBObject> list))
                {
                    list = new List<DBObject>();
                    regionData[assignedRegion] = list;
                }

                // Añadimos
                list.Add(ent);
                entityInRegion.Add(ent.ObjectId);
                entityAddedInSecondIteration++;
                // return
                return;
            }

            // Caso2: Interseca con varias regiones
            if (intersectedRegions.Count > 1)
            {
                // Evaluar en cuántas regiones el disconnect puede ser aceptado
                List<Region> validRegions = new List<Region>();
                // Iteramos por las regiones
                foreach (Region region in intersectedRegions)
                {
                    // Validamos
                    if (!regionData.TryGetValue(region, out List<DBObject> list) || list.Count == 0)
                    {
                        validRegions.Add(region);
                    }
                }

                // Si hay solo una región válida, asignar ahí
                if (validRegions.Count == 1)
                {
                    // Obtenemos la region
                    Region assignedRegion = validRegions[0];
                    // Validamos
                    if (!regionData.TryGetValue(assignedRegion, out List<DBObject> list))
                    {
                        list = new List<DBObject>();
                        regionData[assignedRegion] = list;
                    }

                    // Añadimos a lista de entidades ya asignadas
                    list.Add(ent);
                    entityInRegion.Add(ent.ObjectId);
                    entityAddedInSecondIteration++;
                }
            }
        }

        public static int AssignEntitiesByInterIter(
            Transaction tr,
            IEnumerable<ObjectId> allEntities,
            HashSet<ObjectId> alreadyAssigned,
            IEnumerable<Region> allRegions,
            Dictionary<Region, List<DBObject>> regionData,
            double radTolerance = 20,
            bool boolByGeometryExt = true,
            double toleranceStep = 1
        )
        {
            // -----------------------------
            // Contador
            // -----------------------------

            int addedByIntersection = 0;

            // -----------------------------
            // Diccionario de ambiguos
            // -----------------------------

            Dictionary<ObjectId, List<Region>> possibleRegions =
                new Dictionary<ObjectId, List<Region>>();

            // -----------------------------
            // Primera pasada
            // Resolver directos
            // -----------------------------

            foreach (ObjectId objId in allEntities.Except(alreadyAssigned))
            {
                // Obtener regiones posibles
                //List<Region> intersectedRegions = GetIntersectedRegionsIter(
                //    tr, objId, allRegions, radTolerance, boolByGeometryExt, toleranceStep
                //);
                List<Region> intersectedRegions = GetIntersectedRegionsIterByBrep(
                    tr, objId, allRegions, radTolerance, boolByGeometryExt, toleranceStep
                );

                // -----------------------------
                // Caso 1: Unica interseccion
                // -----------------------------

                if (intersectedRegions.Count == 1)
                {
                    // Obtenemos la region
                    Region assignedRegion = intersectedRegions[0];
                    // Obtenemos la entidad
                    DBObject dbObj = tr.GetObject(objId, OpenMode.ForRead);
                    // Validamos
                    if (!(dbObj is Entity ent)) continue;

                    // Crear lista si no existe
                    if (
                        !regionData.TryGetValue(assignedRegion, out List<DBObject> list
                    ))
                    {
                        list = new List<DBObject>();
                        regionData[assignedRegion] = list;
                    }

                    // Añadir
                    list.Add(ent);
                    alreadyAssigned.Add(objId);
                    addedByIntersection++;
                }

                // -----------------------------
                // Caso 2: Varias intersecciones
                // -----------------------------

                else if (intersectedRegions.Count > 1)
                {
                    // Almacenamos
                    possibleRegions[objId] = intersectedRegions;
                }
            }

            // -----------------------------
            // Resolver ambiguos iterativamente
            // -----------------------------

            bool changes = true;
            // Iteramos
            while (changes)
            {
                changes = false;
                // Iteramos
                foreach (var kvp in possibleRegions.ToList())
                {
                    // Obtenemos entidad
                    ObjectId objId = kvp.Key;
                    // Validamos si ha sido asignado
                    if (alreadyAssigned.Contains(objId)) continue;

                    // -----------------------------
                    // Regiones libres
                    // -----------------------------

                    List<Region> validRegions =
                        kvp.Value
                        .Where(r =>
                        {
                            return
                            !regionData.TryGetValue(
                                r, out List<DBObject> list
                            )
                            ||
                            list.Count == 0;
                        })
                        .ToList();

                    // -----------------------------
                    // Solo una valida
                    // -----------------------------

                    if (validRegions.Count == 1)
                    {
                        Region assignedRegion = validRegions[0];
                        DBObject dbObj = tr.GetObject(objId, OpenMode.ForRead);
                        // Validamos
                        if (!(dbObj is Entity ent)) continue;

                        // Crear lista si no existe
                        if (!regionData.TryGetValue(
                            assignedRegion, out List<DBObject> list
                        ))
                        {
                            list = new List<DBObject>();
                            regionData[assignedRegion] = list;
                        }

                        // Añadir
                        list.Add(ent);
                        alreadyAssigned.Add(objId);
                        addedByIntersection++;
                        changes = true;
                    }
                }
            }

            // -----------------------------
            // Finalizar
            // -----------------------------

            return addedByIntersection;
        }

        

        public static List<Region> GetIntersectedRegionsIter(
            Transaction tr,
            ObjectId dbObjId,
            IEnumerable<Region> allRegions,
            double radTolerance = 20,
            bool boolByGeometryExt = true,
            double maxExtraTolerance = 20,
            double toleranceStep = 1
        )
        {
            List<Region> intersectedRegions = new List<Region>();

            // -----------------------------
            // Obtener entidad
            // -----------------------------

            DBObject dbObj = tr.GetObject(dbObjId, OpenMode.ForRead);
            // Validamos
            if (!(dbObj is Entity ent)) return intersectedRegions;

            // -----------------------------
            // Punto insercion
            // -----------------------------

            Point3d punto = cls_00_GetEntityInsertionPoint.GetEntityInsertionPoint(ent);

            // -----------------------------
            // Radio base
            // -----------------------------

            double baseRadius = radTolerance;

            // -----------------------------
            // Obtener radio geometría
            // -----------------------------

            if (boolByGeometryExt)
            {
                // try
                try
                {
                    // Obtenemos geometría
                    Extents3d ext = ent.GeometricExtents;
                    // Sacamos altura
                    double altura = ext.MaxPoint.Y - ext.MinPoint.Y;
                    // Si la altura es razonable, la usamos como radio del circulo
                    if (altura > 1e-3)
                        baseRadius = altura / 2;
                }
                // catch
                catch { }
            }

            // -----------------------------
            // Expandir tolerancia
            // -----------------------------

            for (
                double extraTol = 0;
                extraTol <= maxExtraTolerance;
                extraTol += toleranceStep
            )
            {
                intersectedRegions.Clear();
                // Nuevo radio
                double currentRadius = baseRadius + extraTol;

                // -----------------------------
                // Buscar regiones
                // -----------------------------

                foreach (Region region in allRegions)
                {
                    using (Circle circulo = new Circle(
                        punto, Vector3d.ZAxis, currentRadius
                    ))
                    {
                        // Lista de ptos de interseccion
                        Point3dCollection ptosInter = new Point3dCollection();
                        // Intersectamos
                        region.IntersectWith(
                            circulo, Intersect.OnBothOperands, ptosInter, IntPtr.Zero, IntPtr.Zero
                        );
                        // Validamos
                        if (ptosInter.Count > 0)
                        {
                            intersectedRegions.Add(region);
                        }
                    }
                }

                // -----------------------------
                // Si tiene intersecciones, paramos
                // -----------------------------

                if (intersectedRegions.Count > 0) break;
            }

            // return
            return intersectedRegions;
        }

        

        public static List<Region> GetIntersectedRegionsIterByBrep(
            Transaction tr,
            ObjectId dbObjId,
            IEnumerable<Region> allRegions,
            double radTolerance = 20,
            bool boolByGeometryExt = true,
            double maxExtraTolerance = 20,
            double toleranceStep = 1
        )
        {
            List<Region> intersectedRegions = new List<Region>();

            // -----------------------------
            // Obtener entidad
            // -----------------------------

            DBObject dbObj = tr.GetObject(dbObjId, OpenMode.ForRead);
            // Validamos
            if (!(dbObj is Entity ent)) return intersectedRegions;

            // -----------------------------
            // Punto insercion
            // -----------------------------

            Point3d punto = cls_00_GetEntityInsertionPoint.GetEntityInsertionPoint(ent);

            // -----------------------------
            // Verificar Punto dentro de region
            // -----------------------------

            foreach (Region region in allRegions)
            {
                // Validamos
                if (cls_00_GetElemByRegionByBrep.PointByRegionByBrep(punto, region)
                )
                {
                    // Añadimos
                    intersectedRegions.Add(region);
                }
            }

            // -----------------------------
            // Validacion
            // -----------------------------

            if (intersectedRegions.Count > 0)
            {
                return intersectedRegions;
            }

            // -----------------------------
            // Radio base
            // -----------------------------

            double baseRadius = radTolerance;

            // -----------------------------
            // Obtener radio geometría
            // -----------------------------

            if (boolByGeometryExt)
            {
                // try
                try
                {
                    // Obtenemos geometría
                    Extents3d ext = ent.GeometricExtents;
                    // Sacamos altura
                    double altura = ext.MaxPoint.Y - ext.MinPoint.Y;
                    // Si la altura es razonable, la usamos como radio del circulo
                    if (altura > 1e-3)
                        baseRadius = altura / 2;
                }
                // catch
                catch { }
            }

            // -----------------------------
            // Expandir tolerancia
            // -----------------------------

            for (
                double extraTol = 0;
                extraTol <= maxExtraTolerance;
                extraTol += toleranceStep
            )
            {
                intersectedRegions.Clear();
                // Nuevo radio
                double currentRadius = baseRadius + extraTol;

                // -----------------------------
                // Buscar regiones
                // -----------------------------

                foreach (Region region in allRegions)
                {
                    using (Circle circulo = new Circle(
                        punto, Vector3d.ZAxis, currentRadius
                    ))
                    {
                        // Lista de ptos de interseccion
                        Point3dCollection ptosInter = new Point3dCollection();
                        // Intersectamos
                        region.IntersectWith(
                            circulo, Intersect.OnBothOperands, ptosInter, IntPtr.Zero, IntPtr.Zero
                        );
                        // Validamos
                        if (ptosInter.Count > 0)
                        {
                            intersectedRegions.Add(region);
                        }
                    }
                }

                // -----------------------------
                // Si tiene intersecciones, paramos
                // -----------------------------

                if (intersectedRegions.Count > 0) break;
            }

            // return
            return intersectedRegions;
        }

        public static List<Region> GetIntersectedRegions(
            Transaction tr,
            ObjectId dbObjId,
            IEnumerable<Region> allRegions,
            double radTolerance = 20,
            bool boolByGeometryExt = true
        )
        {
            List<Region> intersectedRegions = new List<Region>();

            // -----------------------------
            // Obtener entidad
            // -----------------------------

            DBObject dbObj = tr.GetObject(dbObjId, OpenMode.ForRead);
            // Validamos
            if (!(dbObj is Entity ent)) return intersectedRegions;

            // -----------------------------
            // Punto insercion
            // -----------------------------

            Point3d punto = cls_00_GetEntityInsertionPoint.GetEntityInsertionPoint(ent);

            // -----------------------------
            // Radio base
            // -----------------------------

            double baseRadius = radTolerance;

            // -----------------------------
            // Buscar regiones
            // -----------------------------

            foreach (Region region in allRegions)
            {
                // -----------------------------
                // Obtener radio geometría
                // -----------------------------

                if (boolByGeometryExt)
                {
                    // try
                    try
                    {
                        // Obtenemos geometría
                        Extents3d ext = ent.GeometricExtents;
                        // Sacamos altura
                        double altura = ext.MaxPoint.Y - ext.MinPoint.Y;
                        // Si la altura es razonable, la usamos como radio del circulo
                        if (altura > 1e-3)
                            baseRadius = altura / 2;
                    }
                    // catch
                    catch { }
                }

                // -----------------------------
                // Intersección
                // -----------------------------

                // Creamos un circulo centrado en el Base Point de la entidad
                using (Circle circulo = new Circle(punto, Vector3d.ZAxis, baseRadius))
                {
                    // Lista de ptos de interseccion
                    Point3dCollection ptosInter = new Point3dCollection();
                    // Intersectamos
                    region.IntersectWith(
                        circulo, Intersect.OnBothOperands, ptosInter, IntPtr.Zero, IntPtr.Zero
                    );
                    // Validamos
                    if (ptosInter.Count > 0)
                    {
                        intersectedRegions.Add(region);
                    }
                }
            }

            // return
            return intersectedRegions;
        }





    }
}
