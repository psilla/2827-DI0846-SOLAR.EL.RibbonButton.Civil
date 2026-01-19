using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_13_ProcessExportLabelsToExcel
    {
        public static List<List<string>> ProcessExportLabelsToExcel(
            List<DBObject> mTextObjects
        )
        {
            List<List<string>> dataToExcel = new List<List<string>>();
            // Iteramos
            foreach (DBObject dbObj in mTextObjects)
            {
                // Obtenemos el texto
                MText mText = dbObj as MText;
                // Validamos
                if (mText == null) continue;

                // Obtenemos valor del texto
                string value = mText.Contents;
                // Extraemos campos
                List<string> splitValues =
                    cls_00_MTextObjectsByLayer.SplitLabelValueByCond(value);
                // Validamos
                if (splitValues == null || splitValues.Count == 0) continue;

                // Extraemos coordenadas
                double x = mText.Location.X;
                double y = mText.Location.Y;

                // Nueva lista con el orden deseado
                List<string> row = new List<string>
                    {
                        x.ToString("F3"),
                        y.ToString("F3"),
                        value
                    };
                // Agregar los campos del split
                row.AddRange(splitValues);
                // Agregar a la lista final
                dataToExcel.Add(row);
            }
            // return
            return dataToExcel;
        }


    }
}
