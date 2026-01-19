using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_BuildStringData
    {
        public static Dictionary<string, object> BuildStringData(
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
                    kvp => kvp.Value, // region handle
                    kvp => kvp.Key    // poly handle (string)
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
                // Info Cable
                if (cableByString.TryGetValue(region, out object cableValue))
                {
                    // Validamos
                    if (cableValue is EntityExcelRow cableRow)
                    {
                        cableId = cableRow.CableHandle;
                        cableLayer = cableRow.CableLayer;
                        cableLength = cableRow.CableLength; 
                    }
                    else if (cableValue is string s)
                    {
                        cableLength = s; 
                    }
                }

                // Almacenamos
                result[regionHandleAsStr] = new EntityExcelRow
                {
                    // Asignamos Id 
                    StringHandle = stringHandleAsStr,
                    // Asignamos label
                    StringLabel = labelByString.ContainsKey(region)
                    ? labelByString[region]
                    : null,
                    // Asignamos info cable
                    CableHandle = cableId,
                    CableLayer = cableLayer,
                    CableLength = cableLength
                };
            }
            // return
            return result;
        }

    }
}
