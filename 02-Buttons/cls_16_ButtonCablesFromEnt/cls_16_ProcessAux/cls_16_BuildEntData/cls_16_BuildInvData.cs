using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_BuildInvData
    {
        public static Dictionary<string, object>
        BuildInvData(
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
                    // Validamos
                    if (inv.CableInfo is EntityExcelRow cableRow)
                    {
                        cableId = cableRow.CableHandle;
                        cableLayer = cableRow.CableLayer;
                        cableLength = cableRow.CableLength; 
                    }
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
                            CableLength = cableLength
                        }
                    );
                }
            }
            // return
            return result;
        }

    }
}
