using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_BuildDataMV
    {
        public static Dictionary<string, object> BuildDataMV(
            Transaction tr,
            Dictionary<ObjectId, EntityExcelRow> cableRelationDict,
            string defaultValue
        )
        {
            Dictionary<string, object> excelData = new Dictionary<string, object>();
            int rowIndex = 1;
            // Iteramos
            foreach (var kvp in cableRelationDict)
            {
                // Obtenemos valor
                EntityExcelRow row = kvp.Value;

                List<string> labels = new List<string>();
                // Iteramos
                foreach (ObjectId labelId in row.RelatedLabels)
                {
                    string txt = GetLabelText(tr, labelId);
                    // Validamos
                    if (!string.IsNullOrWhiteSpace(txt))
                        // Añadimos
                        labels.Add(txt);
                }

                // Ordenamos numericamente
                labels = labels.OrderBy(l => ExtractNumericSuffix(l)).ToList();

                // Casos
                if (labels.Count == 1)
                {
                    row.CtLabelFrom = labels[0];
                    row.CtLabelTo = defaultValue;
                }
                else if (labels.Count >= 2)
                {
                    row.CtLabelFrom = labels[0];
                    row.CtLabelTo = labels[1];
                }
                else
                {
                    row.CtLabelFrom = string.Empty;
                    row.CtLabelTo = string.Empty;
                }

                excelData.Add($"Cable_{rowIndex}", row);
                rowIndex++;
            }
            // return
            return excelData;
        }

        private static string GetLabelText(Transaction tr, ObjectId labelId)
        {
            Entity ent = tr.GetObject(labelId, OpenMode.ForRead) as Entity;

            if (ent is DBText dbText)
                return dbText.TextString;

            if (ent is MText mText)
                return mText.Text;

            return labelId.Handle.ToString();
        }

        private static int ExtractNumericSuffix(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return int.MaxValue;

            string digits = new string(label.Where(char.IsDigit).ToArray());

            if (int.TryParse(digits, out int value))
                return value;

            return int.MaxValue;
        }

        public static Dictionary<string, object> BuildCtDataByLayer(
            Transaction tr,
            Dictionary<ObjectId, EntityExcelRow> cableRelationDict,
            string defaultValue
        )
        {
            Dictionary<string, object> excelData = new Dictionary<string, object>();
            int rowIndex = 1;

            // 1Agrupamos por capa
            var cablesByLayer = cableRelationDict.Values
                .GroupBy(r => r.CableLayer);
            // Iteramos
            foreach (var layerGroup in cablesByLayer)
            {
                List<EntityExcelRow> layerCables = new List<EntityExcelRow>();
                // Construimos CtLabelFrom / CtLabelTo
                foreach (var row in layerGroup)
                {
                    List<string> labels = new List<string>();

                    foreach (ObjectId labelId in row.RelatedLabels)
                    {
                        string txt = GetLabelText(tr, labelId);
                        if (!string.IsNullOrWhiteSpace(txt))
                            labels.Add(txt);
                    }

                    labels = labels.OrderBy(l => ExtractNumericSuffix(l)).ToList();

                    if (labels.Count == 1)
                    {
                        row.CtLabelFrom = labels[0];
                        row.CtLabelTo = defaultValue;
                    }
                    else if (labels.Count >= 2)
                    {
                        row.CtLabelFrom = labels[0];
                        row.CtLabelTo = labels[1];
                    }
                    else
                    {
                        row.CtLabelFrom = string.Empty;
                        row.CtLabelTo = string.Empty;
                    }

                    layerCables.Add(row);
                }

                // Ordenamos por conectividad dentro de la capa
                List<EntityExcelRow> orderedLayerCables =
                    OrderAndOrientCablesByLayer(layerCables, defaultValue);

                // Añadimos al excelData en el orden correcto
                foreach (var row in orderedLayerCables)
                {
                    excelData.Add($"Cable_{rowIndex}", row);
                    rowIndex++;
                }
            }

            return excelData;
        }

        private static List<EntityExcelRow> OrderAndOrientCablesByLayer(
            List<EntityExcelRow> cables,
            string defaultValue
        )
        {
            // Filtramos cables con 2 extremos válidos
            var valid = cables
                .Where(c => !string.IsNullOrWhiteSpace(c.CtLabelFrom) &&
                            !string.IsNullOrWhiteSpace(c.CtLabelTo))
                .ToList();

            if (valid.Count <= 1) return cables;

            // Construimos "adyacencia" por etiqueta (grafo no dirigido)
            var adj = new Dictionary<string, List<EntityExcelRow>>();

            foreach (var c in valid)
            {
                string a = c.CtLabelFrom; // extremo A
                string b = c.CtLabelTo;   // extremo B

                if (!adj.ContainsKey(a)) adj[a] = new List<EntityExcelRow>();
                if (!adj.ContainsKey(b)) adj[b] = new List<EntityExcelRow>();

                adj[a].Add(c);
                adj[b].Add(c);
            }

            // Elegimos inicio: nodo con grado 1 que NO sea Subestación
            // (en una cadena, los extremos tienen grado 1)
            string startLabel = adj
                .Where(kvp => kvp.Key != defaultValue && kvp.Value.Count == 1)
                .Select(kvp => kvp.Key)
                .OrderBy(l => ExtractNumericSuffix(l)) // heurística si hay dos extremos
                .FirstOrDefault();

            if (string.IsNullOrEmpty(startLabel))
            {
                // fallback: devolvemos tal cual (no podemos orientar seguro)
                return cables;
            }

            var ordered = new List<EntityExcelRow>();
            var visitedCables = new HashSet<EntityExcelRow>();

            string current = startLabel;

            while (true)
            {
                // Elegimos el siguiente cable conectado a current que no hayamos usado
                var nextCable = adj[current].FirstOrDefault(c => !visitedCables.Contains(c));
                if (nextCable == null) break;

                visitedCables.Add(nextCable);

                // Orientamos el cable según el nodo actual
                string a = nextCable.CtLabelFrom; // extremo A
                string b = nextCable.CtLabelTo;   // extremo B

                if (a == current)
                {
                    // ya está orientado current -> other
                    current = b;
                }
                else if (b == current)
                {
                    // invertimos para que current quede como From
                    nextCable.CtLabelFrom = b;
                    nextCable.CtLabelTo = a;
                    current = a;
                }
                else
                {
                    // raro: no conecta realmente
                    break;
                }

                ordered.Add(nextCable);

                // fin si llegamos a Subestación
                if (current == defaultValue)
                    break;

                if (!adj.ContainsKey(current))
                    break;
            }

            // Añadimos los que no se pudieron ordenar/orientar (seguridad)
            foreach (var c in cables)
                if (!ordered.Contains(c))
                    ordered.Add(c);
            // return
            return ordered;
        }





    }
}
