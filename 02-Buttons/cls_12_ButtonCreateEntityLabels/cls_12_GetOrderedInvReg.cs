using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal static class cls_12_GetOrderedInvReg
    {
        public static List<Region> GetOrderedInverterRegions(
            List<Entity> contInvEntities,
            Dictionary<Handle, Handle> dictPolyToRegionContInv,
            List<Region> validRegionContInv,
            StringBuilder infoRegiones
        )
        {
            List<Region> orderedInvRegions = new List<Region>();
            // Iteramos por los Inversores
            foreach (Entity invEntity in contInvEntities)
            {
                // Obtenemos el handle
                Handle invHandle = invEntity.Handle;
                // Buscamos en el dict la relacion con la region
                if (dictPolyToRegionContInv.TryGetValue(invHandle, out Handle regionHandle))
                {
                    // Buscar la region correspondiente
                    Region invRegion =
                        validRegionContInv.FirstOrDefault(r => r.Handle == regionHandle);
                    // Validamos
                    if (invRegion != null)
                    {
                        // Almacenamos
                        orderedInvRegions.Add(invRegion);
                    }
                }
                // En caso contrario
                else
                {
                    // Mostramos
                    infoRegiones.AppendLine(
                        $"⚠ Inverter entity {invEntity.Handle} " +
                        $"has no matching region."
                    );
                }
            }
            // return
            return orderedInvRegions;
        }
    }





    
}
