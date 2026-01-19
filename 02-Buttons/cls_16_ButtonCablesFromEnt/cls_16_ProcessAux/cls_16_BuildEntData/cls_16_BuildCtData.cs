using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using SOLAR.EL.RibbonButton.Autocad.Settings;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_16_BuildCtData
    {
        public static Dictionary<string, object> BuildCtData(
            Transaction tr,
            Dictionary<ObjectId, CableCtInfo> cableRelationDict,
            string defaultValue
        )
        {
            Dictionary<string, object> excelData = new Dictionary<string, object>();
            int rowIndex = 1;

            foreach (var kvp in cableRelationDict)
            {
                ObjectId cableId = kvp.Key;
                CableCtInfo info = kvp.Value;

                List<string> labels = new List<string>();

                foreach (ObjectId labelId in info.RelatedLabels)
                {
                    string txt = GetLabelText(tr, labelId);
                    if (!string.IsNullOrWhiteSpace(txt))
                        labels.Add(txt);
                }

                // Ordenamos numéricamente
                labels = labels
                    .OrderBy(l => ExtractNumericSuffix(l))
                    .ToList();

                string labelFrom;
                string labelTo;

                if (labels.Count == 1)
                {
                    labelFrom = labels[0];
                    labelTo = defaultValue;
                }
                else if (labels.Count >= 2)
                {
                    labelFrom = labels[0];
                    labelTo = labels[1];
                }
                else
                {
                    labelFrom = string.Empty;
                    labelTo = string.Empty;
                }

                EntityExcelRow row = new EntityExcelRow
                {
                    CableHandle = cableId.Handle.ToString(),
                    CtLabelFrom = labelFrom,
                    CtLabelTo = labelTo,
                    CableLength = Math.Round(info.CableLength, 2)
                };

                excelData.Add($"Cable_{rowIndex}", row);
                rowIndex++;
            }

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


    }
}
