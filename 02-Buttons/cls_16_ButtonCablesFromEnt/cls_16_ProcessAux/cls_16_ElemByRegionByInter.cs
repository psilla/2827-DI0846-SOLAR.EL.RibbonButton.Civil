using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using TYPSA.SharedLib.Autocad.EntitiesInsertionPoint;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_ElemByRegionByInter
    {
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


    }
}
