using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.GetEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_GetTrackInRegInv
    {
        public static List<Entity> GetEntInRegByPoint(
            Transaction tr,
            Region entityRegion,
            HashSet<ObjectId> entitiesIds
        )
        {
            // Diccionario de Entidades por region
            Dictionary<string, List<DBObject>> dictEntByRegion =
                cls_00_GetEntityByRegion.GetEntityByRegionByPoint(
                    tr, entityRegion, entitiesIds
                );
            // Obtenemos lista de Objetos por region 
            List<DBObject> objList =
                dictEntByRegion.SelectMany(kv => kv.Value).ToList();
            // Convertimos a entidades
            List<Entity> entList = objList.OfType<Entity>().ToList();
            // return
            return entList;
        }

        public static List<Entity> GetEntInRegByPoints(
            Transaction tr,
            Region entityRegion,
            HashSet<ObjectId> entitiesIds
        )
        {
            // Diccionario de Entidades por región
            Dictionary<string, List<DBObject>> dictEntByRegion =
                cls_00_GetEntityByRegion.GetEntityByRegionByPoints(
                    tr, entityRegion, entitiesIds
                );
            // Obtenemos lista de Objetos por region 
            List<DBObject> objList =
                dictEntByRegion.SelectMany(kv => kv.Value).ToList();
            // Convertimos a entidades
            List<Entity> entList = objList.OfType<Entity>().ToList();
            // Return
            return entList;
        }





    }
}
