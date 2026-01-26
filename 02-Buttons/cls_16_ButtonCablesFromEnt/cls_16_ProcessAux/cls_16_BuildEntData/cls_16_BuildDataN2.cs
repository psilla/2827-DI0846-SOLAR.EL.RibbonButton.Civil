using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_BuildDataN2
    {
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
            // return
            return result;
        }

    }
}
