using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.GetEntities;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_GetDictLabelByEnt
    {
        private static void ShowEntLabelByRegionSummary(
            Dictionary<Region, string> entLabelByRegion,
            HashSet<ObjectId> unusedLabelIds
        )
        {
            // -----------------------------
            // Info
            // -----------------------------

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("📌 Entity Labels By Region Summary");
            sb.AppendLine();

            int index = 1;

            // Iteramos
            foreach (var kvp in entLabelByRegion)
            {
                Region region = kvp.Key;
                string label = kvp.Value;

                sb.AppendLine($"🔹 Region {index++}");
                sb.AppendLine($"   - Region Handle: {region.Handle}");
                sb.AppendLine($"   - Label: {label}");
                sb.AppendLine();
            }

            // -----------------------------
            // Unused Labels
            // -----------------------------

            sb.AppendLine("📌 Unused Label Ids");

            if (unusedLabelIds != null && unusedLabelIds.Count > 0)
            {
                foreach (ObjectId id in unusedLabelIds)
                {
                    sb.AppendLine($"   - {id.Handle}");
                }
            }
            else
            {
                sb.AppendLine("   - NONE");
            }

            // -----------------------------
            // Mostrar
            // -----------------------------

            ShowStringBuilder.ShowInfo(
                "📌 Entity Labels By Region Summary",
                sb.ToString()
            );
        }

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
                List<Entity> labelEntities = cls_00_GetEntityListByRegion.GetEntityListByRegionByPoint(
                    tr, region, psrEntLabIds
                );
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

                        string text = null;
                        // Validamos
                        if (labelEnt is MText mtext)
                        {
                            // Obtenemos el valor
                            text = mtext.Contents;
                        }
                        else if (labelEnt is DBText dbtext)
                        {
                            // Obtenemos el valor
                            text = dbtext.TextString;
                        }
                        // Validamos
                        if (!string.IsNullOrWhiteSpace(text))
                            entLabelText = text.Trim();
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

            bool showInfo = false;
            // Debug
            if (showInfo)
                ShowEntLabelByRegionSummary(entLabelByRegion, unusedLabelIds);

            // return
            return entLabelByRegion;
        }


    }
}
