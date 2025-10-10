using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.GetEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_12_GetInvInRegGen
    {
        public static List<Entity> GetInvInRegGen(
            Transaction tr,
            Region regionContGen,
            HashSet<ObjectId> contInvIds,
            StringBuilder infoRegiones
        )
        {
            // Mostramos info
            infoRegiones.AppendLine($"═══════════════════════════════════");
            infoRegiones.AppendLine($" ContGen Region Handle: {regionContGen.Handle}");
            infoRegiones.AppendLine($"═══════════════════════════════════");

            // Diccionario de Inversores por region
            Dictionary<string, List<DBObject>> dictContInvByRegion =
                cls_00_GetEntityByRegion.GetEntityByRegionByPoint(
                    tr, regionContGen, contInvIds
                );
            // Validamos
            if (dictContInvByRegion == null || dictContInvByRegion.Count == 0)
            {
                // Mensaje
                infoRegiones.AppendLine("⚠ dictContInvByRegion is empty for this General Contour.");
                // Finalizamos
                return null;
            }

            // Obtenemos lista de Inversores por region 
            List<DBObject> contInvList =
                dictContInvByRegion.SelectMany(kv => kv.Value).ToList();
            // Validamos
            if (contInvList.Count == 0)
            {
                // Mensaje
                infoRegiones.AppendLine("⚠ contInvList is empty for this General Contour.");
                // Finalizamos
                return null;
            }

            // Convertimos a entidades
            List<Entity> contInvEntities = contInvList.OfType<Entity>().ToList();
            // Validamos
            if (contInvEntities.Count == 0)
            {
                // Mensaje
                infoRegiones.AppendLine(
                    "⚠ contInvEntities is empty (no valid entities) " +
                    "for this General Contour."
                );
                // Finalizamos
                return null;
            }

            // return
            return contInvEntities;
        }


    }
}
