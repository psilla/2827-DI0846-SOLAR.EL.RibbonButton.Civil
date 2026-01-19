using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.DeleteEntities;
using TYPSA.SharedLib.Autocad.IsolateEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_ValidateDataEnt
    {
        public static void ValidateRegEnt(
            Editor ed,
            List<Region> validRegionEntity,
            List<Region> invalidRegions
        )
        {
            // Obtenemos Ids
            HashSet<ObjectId> invalidRegionIds = new HashSet<ObjectId>(
                invalidRegions.Select(r => r.ObjectId)
            );

            // Obtenemos regiones a borrar
            List<Region> regionsToDelete = validRegionEntity
                .Where(r => !invalidRegionIds.Contains(r.ObjectId))
                .ToList();

            // Borramos regiones
            foreach (Region region in regionsToDelete)
            {
                // Validamos
                if (region == null) continue;
                // Borramos
                cls_00_DeleteEntity.DeleteEntity(region);
            }

            // Aislamos
            cls_00_IsolateEntities.IsolateObjects(ed, invalidRegionIds);
        }

        public static void ValidatCableStr(
            Editor ed,
            int regionsWithNoCable,
            int regionsWithMultipleCables,
            HashSet<ObjectId> unusedCableIds,
            List<Region> invalidCableRegions,
            List<Region> validRegionEntity
        )
        {
            StringBuilder sb = new StringBuilder();
            // Mostramos
            sb.AppendLine("⚠ Cable validation errors detected:\n");
            // Validamos
            if (regionsWithNoCable > 0)
            {
                // Mostramos
                sb.AppendLine($"• Strings with NO cable inside the region: {regionsWithNoCable}");
            }
            // Validamos
            if (regionsWithMultipleCables > 0)
            {
                // Mostramos
                sb.AppendLine($"• Strings with MULTIPLE cables inside the same region: {regionsWithMultipleCables}");
            }
            // Validamos
            if (unusedCableIds.Count > 0)
            {
                // Mostramos
                sb.AppendLine($"• Cables not contained inside any String region: {unusedCableIds.Count}");
            }
            // Mostramos
            sb.AppendLine("\nThe problematic regions and cables will be isolated in AutoCAD.");
            // Mensaje
            MessageBox.Show(
                sb.ToString(), "Cable Validation Errors",
                MessageBoxButtons.OK, MessageBoxIcon.Warning
            );

            HashSet<ObjectId> idsToIsolate = new HashSet<ObjectId>();
            // Regiones sin cable
            foreach (Region region in invalidCableRegions)
            {
                // Almacenamos
                idsToIsolate.Add(region.ObjectId);
            }

            // Cables no contenidos en ninguna region
            foreach (ObjectId id in unusedCableIds)
            {
                // Almacenamos
                idsToIsolate.Add(id);
            }

            // Borrar regiones validas
            List<Region> regionsToDelete = validRegionEntity
                .Where(r => !invalidCableRegions.Contains(r))
                .ToList();
            // Iteramos
            foreach (Region region in regionsToDelete)
            {
                // Borramos
                cls_00_DeleteEntity.DeleteEntity(region);
            }

            // Aislamos
            cls_00_IsolateEntities.IsolateObjects(ed, idsToIsolate);
        }

        public static void ValidatCableInv(
            Editor ed,
            int invertersWithNoCable,
            int invertersWithMultipleCables,
            HashSet<ObjectId> unusedCableIds,
            List<ObjectId> invalidInverterIds,
            List<Region> validRegionEntity
        )
        {
            StringBuilder sb = new StringBuilder();
            // Mostramos
            sb.AppendLine("⚠ Cable validation errors detected:\n");
            // Validamos
            if (invertersWithNoCable > 0)
            {
                // Mostramos
                sb.AppendLine($"• Inverters with NO cable connected: {invertersWithNoCable}");
            }
            // Validamos
            if (invertersWithMultipleCables > 0)
            {
                // Mostramos
                sb.AppendLine($"• Inverters with MULTIPLE cables connected: {invertersWithMultipleCables}");
            }
            // Validamos
            if (unusedCableIds.Count > 0)
            {
                // Mostramos
                sb.AppendLine($"• Cables not connected to any inverter: {unusedCableIds.Count}");
            }
            // Mostramos
            sb.AppendLine("\nThe problematic inverters and cables will be isolated in AutoCAD.");
            // Mensaje
            MessageBox.Show(
                sb.ToString(), "Cable Validation Errors",
                MessageBoxButtons.OK, MessageBoxIcon.Warning
            );

            // Borramos regiones
            foreach (Region region in validRegionEntity)
            {
                // Validamos
                if (region == null) continue;
                // Borramos
                cls_00_DeleteEntity.DeleteEntity(region);
            }

            HashSet<ObjectId> idsToIsolate = new HashSet<ObjectId>();
            // Inversores inválidos
            foreach (ObjectId invId in invalidInverterIds)
            {
                // Almacenamos
                idsToIsolate.Add(invId);
            }

            // Cables no conectados
            foreach (ObjectId cabId in unusedCableIds)
            {
                // Almacenamos
                idsToIsolate.Add(cabId);
            }

            // Aislamos
            cls_00_IsolateEntities.IsolateObjects(ed, idsToIsolate);
        }

    }
}
