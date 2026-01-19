using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetCabLengthSummary
    {
        public static Dictionary<string, double> BuildCableSummaryString(
            List<EntityExcelRow> rows,
            int groupLevels,
            char[] separators
        )
        {
            // return
            return rows
                .Select(r => new
                {
                    Key = GetGroupKey(r.StringLabel, groupLevels, separators),
                    Length = GetCableLengthAsDouble(r)
                })
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .GroupBy(x => x.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Length)
                );
        }

        public static Dictionary<string, double> BuildCableSummaryInv(
            List<EntityExcelRow> rows,
            int groupLevels,
            char[] separators
        )
        {
            // return
            return rows
                .Select(r => new
                {
                    Key = GetGroupKey(r.InverterLabel, groupLevels, separators),
                    Length = GetCableLengthAsDouble(r)
                })
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .GroupBy(x => x.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Length)
                );
        }

        public static string GetGroupKey(
            string entityLabel,
            int numLevels,
            char[] separators
        )
        {
            // Validamos
            if (string.IsNullOrWhiteSpace(entityLabel)) return null;

            string[] parts = entityLabel
                .Split(separators, StringSplitOptions.RemoveEmptyEntries);
            // Validamos
            if (parts.Length < numLevels) return null;
            // return
            return string.Join("-", parts.Take(numLevels));
        }

        public static double GetCableLengthAsDouble(EntityExcelRow row)
        {
            // Validamos
            if (row == null || row.CableLength == null) return 0.0;

            // Caso 1: ya es double
            if (row.CableLength is double d) return d;

            // Caso 2: viene como string
            if (row.CableLength is string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return 0.0;

                // Invariant (punto decimal)
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double v1)) return v1;

                // Cultura actual (coma decimal)
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out double v2)) return v2;
            }

            // Por defecto
            return 0.0;
        }
    }
}
