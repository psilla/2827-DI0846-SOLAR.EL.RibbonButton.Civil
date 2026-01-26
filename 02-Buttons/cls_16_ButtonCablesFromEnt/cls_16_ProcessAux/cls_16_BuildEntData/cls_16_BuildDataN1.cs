using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_BuildDataN1
    {
        public static Dictionary<string, object> BuildDataN1(
            List<Region> validRegionString,
            Dictionary<Region, string> labelByString,
            Dictionary<Region, object> cableByString,
            Dictionary<Handle, Handle> dictPolyToRegionString
        )
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            // Invertimos: RegionHandle -> PolyHandle (String)
            Dictionary<Handle, Handle> regionToPoly =
                dictPolyToRegionString.ToDictionary(
                    kvp => kvp.Value, 
                    kvp => kvp.Key    
                );
            // Iteramos
            foreach (Region region in validRegionString)
            {
                // Obtenemos Handle
                string regionHandleAsStr = region.Handle.ToString();

                // Obtener StringId (poly original)
                string stringHandleAsStr = regionToPoly.TryGetValue(region.Handle, out Handle polyHandle)
                    ? polyHandle.ToString()
                    : null;

                string cableId = null;
                string cableLayer = null;
                object cableLength = null;
                double cableLengthCorrectionFactor = 0;
                object cableLengthCorrected = null;
                double cableLengthFixedAllowance = 0;
                object cableLengthCorrectedTotal = null;
                int numberOfConductors = 0;
                object totalInstalledCableLength = null;
                // Info Cable
                if (cableByString.TryGetValue(region, out object cableValue))
                {
                    // Validamos
                    if (cableValue is EntityExcelRow cableRow)
                    {
                        // Obtenemos data
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
                    else if (cableValue is string s)
                    {
                        cableLength = s; 
                    }
                }

                // Almacenamos
                result[regionHandleAsStr] = new EntityExcelRow
                {
                    StringHandle = stringHandleAsStr,
                    StringLabel = labelByString.ContainsKey(region)
                    ? labelByString[region]
                    : null,
                    CableHandle = cableId,
                    CableLayer = cableLayer,
                    CableLength = cableLength,
                    CableLengthCorrectionFactor = cableLengthCorrectionFactor,
                    CableExtraLength = cableLengthCorrected,
                    CableLengthFixedAllowance = cableLengthFixedAllowance,
                    CableLengthCorrectedTotal = cableLengthCorrectedTotal,
                    NumberOfConductors = numberOfConductors,
                    TotalInstalledCableLength = totalInstalledCableLength
                };
            }
            // return
            return result;
        }

    }
}
