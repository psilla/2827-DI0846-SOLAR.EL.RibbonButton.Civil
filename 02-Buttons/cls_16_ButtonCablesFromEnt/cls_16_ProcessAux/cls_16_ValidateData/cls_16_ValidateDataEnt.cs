using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_ValidateDataEnt
    {
        public static HashSet<ObjectId> GetInvalidLabelRegionIds(
            List<Region> invalidRegions
        )
        {
            // return
            return new HashSet<ObjectId>(
                invalidRegions.Select(r => r.ObjectId)
            );
        }

        public static HashSet<ObjectId> GetInvalidCableEntityIds(
            List<Region> invalidCableRegions,
            HashSet<ObjectId> unusedCableIds
        )
        {
            HashSet<ObjectId> ids = new HashSet<ObjectId>();
            // Regiones sin cable
            foreach (Region region in invalidCableRegions)
                ids.Add(region.ObjectId);
            // Cables no contenidos en ninguna region
            foreach (ObjectId id in unusedCableIds)
                ids.Add(id);
            // return
            return ids;
        }

        public static void ShowCableValidationMessageStr(
            int regionsWithNoCable,
            int regionsWithMultipleCables,
            int unusedCableCount
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
            if (unusedCableCount > 0)
            {
                // Mostramos
                sb.AppendLine($"• Cables not contained inside any String region: {unusedCableCount}");
            }
            // Mensaje
            MessageBox.Show(
                sb.ToString(), "Cable Validation Errors",
                MessageBoxButtons.OK, MessageBoxIcon.Warning
            );
        }

        public static void ShowCableValidationMessageInv(
            int invertersWithNoCable,
            int invertersWithMultipleCables,
            int unusedCableCount
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
            if (unusedCableCount > 0)
            {
                // Mostramos
                sb.AppendLine($"• Cables not connected to any inverter: {unusedCableCount}");
            }
            // Mensaje
            MessageBox.Show(
                sb.ToString(), "Cable Validation Errors",
                MessageBoxButtons.OK, MessageBoxIcon.Warning
            );
        }

       


    }
}
