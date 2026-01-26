using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using TYPSA.SharedLib.Autocad.GetEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictLabelByEnt
    {
        public static Dictionary<Region, string> GetDictLabelByEnt(
            Transaction tr,
            List<Region> validRegion,
            HashSet<ObjectId> psrEntLabIds,
            string defaultEntLabel,
            string multipleEntLabel,
            out HashSet<ObjectId> unusedLabelIds
        )
        {
            Dictionary<Region, string> entLabelByRegion = new Dictionary<Region, string>();
            // Inicialmente todas las etiquetas sin usar
            unusedLabelIds = new HashSet<ObjectId>(psrEntLabIds);

            // Iteramos por las regiones
            foreach (Region region in validRegion)
            {
                string entLabelText = defaultEntLabel;
                // Obtener etiqueta
                List<Entity> labelEntities = cls_00_GetEntityListByRegion.
                    GetEntityListByRegionByPoint(tr, region, psrEntLabIds);
                // Validamos
                if (labelEntities != null && labelEntities.Count > 0)
                {
                    // Marcamos etiquetas como usadas
                    foreach (Entity ent in labelEntities)
                        unusedLabelIds.Remove(ent.ObjectId);
                    // Caso1: 1 etiqueta
                    if (labelEntities.Count == 1)
                    {
                        // Obtenemos la etiqueta 
                        Entity labelEnt = labelEntities.First();
                        // Validamos
                        if (labelEnt is MText mtext)
                        {
                            // Obtenemos el valor
                            string text = mtext.Contents.Trim();
                            // Validamos
                            if (!string.IsNullOrWhiteSpace(text))
                                entLabelText = text;
                        }
                    }
                    // Caso2: X etiquetas
                    else
                    {
                        entLabelText = multipleEntLabel;
                    }
                }
                // Almacenamos
                entLabelByRegion[region] = entLabelText;
            }
            // return
            return entLabelByRegion;
        }

    }
}
