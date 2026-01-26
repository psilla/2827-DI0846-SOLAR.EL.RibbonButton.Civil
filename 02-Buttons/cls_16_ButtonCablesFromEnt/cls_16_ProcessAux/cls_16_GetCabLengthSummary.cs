using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetCabLengthSummary
    {
        public static Dictionary<string, (double cableLength, double cableLengthCorrected, double cableLengthCorrectedTotal, double totalInstalledCableLength)> BuildCableSummaryWithPhases(
            List<EntityExcelRow> rows,
            Func<EntityExcelRow, string> keySelector,
            int groupLevels,
            char[] separators
        )
        {
            // return
            return rows.Select(r => new
            {
                Key = GetGroupKey(keySelector(r), groupLevels, separators),
                CableLength = GetDoubleValueFromObj(r?.CableLength),
                CableExtraLength = GetDoubleValueFromObj(r?.CableExtraLength),
                CableLengthCorrectedTotal = GetDoubleValueFromObj(r?.CableLengthCorrectedTotal),
                TotalInstalledCableLength = GetDoubleValueFromObj(r?.TotalInstalledCableLength),
            })
            .Where(x => !string.IsNullOrEmpty(x.Key))
            .GroupBy(x => x.Key)
            .ToDictionary(
                g => g.Key,
                g => (
                    cableLength: g.Sum(x => x.CableLength),
                    cableLengthCorrected: g.Sum(x => x.CableExtraLength),
                    cableLengthCorrectedTotal: g.Sum(x => x.CableLengthCorrectedTotal),
                    totalInstalledCableLength: g.Sum(x => x.TotalInstalledCableLength)
                )
            );
        }

        public static Dictionary<string, (
            double cableLength,
            double cableLengthCorrected,
            double cableLengthCorrectedTotal,
            double totalInstalledCableLength)>
        BuildCableSummaryWithHierarchy(
            List<EntityExcelRow> rows,
            Func<EntityExcelRow, string> keySelector,
            int[] groupIndices,
            char[] separators
        )
        {
            return rows
                .Select(r => new
                {
                    Key = GetGroupKeyByIndices(
                        keySelector(r),
                        groupIndices,
                        separators
                    ),
                    CableLength = GetDoubleValueFromObj(r?.CableLength),
                    CableExtraLength = GetDoubleValueFromObj(r?.CableExtraLength),
                    CableLengthCorrectedTotal = GetDoubleValueFromObj(r?.CableLengthCorrectedTotal),
                    TotalInstalledCableLength = GetDoubleValueFromObj(r?.TotalInstalledCableLength),
                })
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .GroupBy(x => x.Key)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        g.Sum(x => x.CableLength),
                        g.Sum(x => x.CableExtraLength),
                        g.Sum(x => x.CableLengthCorrectedTotal),
                        g.Sum(x => x.TotalInstalledCableLength)
                    )
                );
        }

        private static string GetGroupKey(
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

        private static double GetDoubleValueFromObj(object value)
        {
            // Validamos
            if (value == null) return 0.0;

            // Caso 1: ya es double
            if (value is double d) return d;

            // Caso 2: viene como string
            if (value is string s)
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

        public static Dictionary<string, string> BuildFieldSummaryMappingDictionary(
            List<string> referenceFields,
            string[] defaultSummaries
        )
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            // Iteramos
            for (int i = 0; i < referenceFields.Count; i++)
            {
                string fieldValue = referenceFields[i];
                // Extraemos la clave alfabética
                string fieldKey = cls_00_MTextObjectsByLayer.GetAlphabeticFieldKey(fieldValue);
                // Validamos
                if (string.IsNullOrEmpty(fieldKey))
                {
                    // Mensaje
                    MessageBox.Show(
                        $"Invalid field format: '{fieldValue}'", "Invalid Label Format",
                        MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    // Finalizamos
                    return null;
                }
                // Validamos duplicidad
                if (result.ContainsKey(fieldKey))
                {
                    // Mensaje
                    MessageBox.Show(
                        $"Duplicate field key found: '{fieldKey}'", "Invalid Label Format",
                        MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    // Finalizamos
                    return null;
                }

                // Asignamos por defecto
                string summaryValue =
                    i < defaultSummaries.Length
                        ? defaultSummaries[i]
                        : string.Empty;
                // Almacenamos
                result.Add(fieldKey, summaryValue);
            }
            // Validamos
            if (result == null) return null;

            // Mapeamos el dict
            result = InstanciarFormularios.TextBoxFormOut_NextToLabel_String(
                "Assign the summary type for each label field:\n" +
                "Leave empty if the field should not be used for summaries.",
                result, textBoxWidth: 300
            );
            // Validamos
            if (result == null) return null;

            // return
            return result;
        }

        public static Dictionary<string, int> BuildSummaryIndexDictionary(
            List<string> referenceFields,
            Dictionary<string, string> fieldSummaryMapping
        )
        {
            Dictionary<string, int> summaryIndexDict =
                new Dictionary<string, int>();

            for (int i = 0; i < referenceFields.Count; i++)
            {
                string fieldValue = referenceFields[i];
                string fieldKey = cls_00_MTextObjectsByLayer.GetAlphabeticFieldKey(fieldValue);

                if (string.IsNullOrEmpty(fieldKey))
                    continue;

                if (!fieldSummaryMapping.TryGetValue(fieldKey, out string summaryName))
                    continue;

                if (string.IsNullOrEmpty(summaryName))
                    continue;

                // Index es 1-based para BuildCableSummaryWithPhases
                int groupLevel = i + 1;

                if (summaryIndexDict.ContainsKey(summaryName))
                {
                    MessageBox.Show(
                        $"Summary '{summaryName}' is assigned more than once.",
                        "Invalid Summary Mapping",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return null;
                }

                summaryIndexDict.Add(summaryName, groupLevel);
            }

            return summaryIndexDict;
        }

        private static string GetGroupKeyByIndices(
            string entityLabel,
            int[] indices,
            char[] separators
        )
        {
            if (string.IsNullOrWhiteSpace(entityLabel))
                return null;

            string[] parts = entityLabel
                .Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < indices.Max())
                return null;

            return string.Join(
                "-",
                indices.Select(i => parts[i - 1]) // índices 1-based
            );
        }

        private static Dictionary<string, int> BuildSummaryHierarchy(
            SolarSettings solarSet
        )
        {
            // return
            return new Dictionary<string, int>
            {
                { solarSet.SummaryNameCtKey, 1 },
                { solarSet.SummaryNameInvKey, 2 },
                { solarSet.SummaryNameTrackKey, 3 }
            };
        }


        public static Dictionary<string, int[]> BuildHierarchicalGroupIndices(
            Dictionary<string, int> summaryIndexDict,
            SolarSettings solarSet
        )
        {
            Dictionary<string, int[]> result = new Dictionary<string, int[]>();

            Dictionary<string, int> summaryHierarchy = BuildSummaryHierarchy(solarSet);
            // Iteramos
            foreach (var kvp in summaryIndexDict)
            {
                string summaryKey = kvp.Key;
                // Validamos
                if (!summaryHierarchy.ContainsKey(summaryKey)) continue;

                int hierarchyLevel = summaryHierarchy[summaryKey];

                int[] indices = summaryIndexDict
                    .Where(x =>
                        summaryHierarchy.ContainsKey(x.Key) &&
                        summaryHierarchy[x.Key] <= hierarchyLevel
                    )
                    .Select(x => x.Value)
                    .OrderBy(i => i)
                    .ToArray();
                // Añadimos
                result.Add(summaryKey, indices);
            }
            // return
            return result;
        }







    }
}
