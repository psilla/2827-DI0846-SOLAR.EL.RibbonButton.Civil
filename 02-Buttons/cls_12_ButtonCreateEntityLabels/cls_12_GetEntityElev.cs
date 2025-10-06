using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Linq;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_GetEntityElev
    {
        public static bool AllEntHaveSameElev(
            Transaction tr,
            PromptSelectionResult psr,
            string entityTag,
            out double commonElevation,
            double tolerance = 1e-6
        )
        {
            commonElevation = 0.0;

            if (psr == null || psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                return false;

            double? refElevation = null;
            Dictionary<double, int> elevCounts = new Dictionary<double, int>();

            foreach (SelectedObject selObj in psr.Value)
            {
                if (selObj == null) continue;

                DBObject dbObj = tr.GetObject(selObj.ObjectId, OpenMode.ForRead);
                if (dbObj == null) continue;

                double elev;

                if (dbObj is Polyline pl)
                {
                    // Elevación de polilínea
                    elev = pl.Elevation;
                }
                else if (dbObj is BlockReference br)
                {
                    // Elevación de bloque (posición Z)
                    elev = br.Position.Z;
                }
                else
                {
                    // Si no es ninguno de los tipos que nos interesan, seguimos
                    continue;
                }

                // Contar entidades por elevación (redondeamos para agrupar)
                double roundedElev = Math.Round(elev, 3);
                if (!elevCounts.ContainsKey(roundedElev))
                    elevCounts[roundedElev] = 0;
                elevCounts[roundedElev]++;
                // Validamos
                if (refElevation == null)
                {
                    refElevation = elev;
                }
                else if (Math.Abs(elev - refElevation.Value) > tolerance)
                {
                    // Construir resumen de elevaciones detectadas
                    string elevSummary = string.Join(
                        Environment.NewLine,
                        elevCounts.Select(kv => $"   Z={kv.Key:F3} → {kv.Value} {entityTag}")
                    );
                    // Mensaje
                    MessageBox.Show(
                        $"⚠ {entityTag} are not at the same elevation.\n\n" +
                        $"Different elevations detected:\n{elevSummary}",
                        "Elevation Check",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return false;
                }
            }
            commonElevation = refElevation ?? 0.0;
            // return
            return true;
        }








    }
}
