using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_BuildDataN2
    {
        private static void ShowBuildDataN2Summary(
            Dictionary<string, object> result
        )
        {
            // -----------------------------
            // Info
            // -----------------------------

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("📌 Summary");
            sb.AppendLine();

            // Iteramos
            foreach (var kvp in result)
            {
                // Validamos
                if (!(kvp.Value is EntityExcelRow row)) continue;

                sb.AppendLine($"🔹 {kvp.Key}");

                sb.AppendLine($"   - Inverter Handle: {row.InverterHandle}");
                sb.AppendLine($"   - Inverter Label: {row.InverterLabel}");

                sb.AppendLine($"   - Cable Handle: {row.CableHandle}");
                sb.AppendLine($"   - Cable Layer: {row.CableLayer}");

                sb.AppendLine($"   - Cable Length: {row.CableLength}");
                sb.AppendLine($"   - Cable Length Correction Factor: {row.CableLengthCorrectionFactor}");

                sb.AppendLine($"   - Cable Extra Length: {row.CableExtraLength}");
                sb.AppendLine($"   - Cable Fixed Allowance: {row.CableLengthFixedAllowance}");

                sb.AppendLine($"   - Cable Corrected Total: {row.CableLengthCorrectedTotal}");

                sb.AppendLine($"   - Number Of Conductors: {row.NumberOfConductors}");
                sb.AppendLine($"   - Total Installed Cable Length: {row.TotalInstalledCableLength}");

                sb.AppendLine();
            }

            // -----------------------------
            // Mostrar
            // -----------------------------

            ShowStringBuilder.ShowInfo(
                "📌 BuildDataN2 Summary",
                sb.ToString()
            );
        }

        public static Dictionary<string, object> BuildDataN2(
            Dictionary<Region, List<(ObjectId InverterId, string InverterLabel, object CableInfo)>> inverterDataByRegion
        )
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            int rowIndex = 1;
            // Iteramos
            foreach (var kvp in inverterDataByRegion)
            {
                var inverterList = kvp.Value;
                // Iteramos
                foreach (var inv in inverterList)
                {
                    string cableId = null;
                    string cableLayer = null;
                    object cableLength = null;
                    double cableLengthCorrectionFactor = 0;
                    object cableLengthCorrected = null;
                    double cableLengthFixedAllowance = 0;
                    object cableLengthCorrectedTotal = null;
                    int numberOfConductors = 0;
                    object totalInstalledCableLength = null;
                    // Validamos
                    if (inv.CableInfo is EntityExcelRow cableRow)
                    {
                        cableId = cableRow.CableHandle;
                        cableLayer = cableRow.CableLayer;
                        cableLength = cableRow.CableLength;
                        cableLengthCorrectionFactor = cableRow.CableLengthCorrectionFactor;
                        cableLengthCorrected = cableRow.CableExtraLength;
                        cableLengthFixedAllowance = cableRow.CableLengthFixedAllowance;
                        cableLengthCorrectedTotal = cableRow.CableLengthCorrectedTotal;
                        numberOfConductors = cableRow.NumberOfConductors;
                        totalInstalledCableLength = cableRow.TotalInstalledCableLength;
                    }
                    // En caso contrario
                    else if (inv.CableInfo is string s)
                    {
                        cableLength = s; 
                    }
                    // Almacenamos
                    result.Add(
                        $"ROW_{rowIndex++}",
                        new EntityExcelRow
                        {
                            InverterHandle = inv.InverterId.Handle.ToString(),
                            InverterLabel = inv.InverterLabel,
                            CableHandle = cableId,
                            CableLayer = cableLayer,
                            CableLength = cableLength,
                            CableLengthCorrectionFactor = cableLengthCorrectionFactor,
                            CableExtraLength = cableLengthCorrected,
                            CableLengthFixedAllowance = cableLengthFixedAllowance,
                            CableLengthCorrectedTotal = cableLengthCorrectedTotal,
                            NumberOfConductors = numberOfConductors,
                            TotalInstalledCableLength = totalInstalledCableLength
                        }
                    );
                }
            }

            bool showInfo = false;
            // Debug
            if (showInfo)
                ShowBuildDataN2Summary(result);

            // return
            return result;
        }

    }
}
