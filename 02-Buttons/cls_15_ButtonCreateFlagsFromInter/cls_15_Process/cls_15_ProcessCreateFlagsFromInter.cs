using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;
using TYPSA.SharedLib.Autocad.DrawEntities;
using TYPSA.SharedLib.Autocad.GetEntities;

namespace SOLAR.EL.RibbonButton.Autocad.Process
{
    internal class cls_15_ProcessCreateFlagsFromInter
    {
        public static int? ProcessCreateFlagsFromInter(
            Database db,
            Transaction tr,
            BlockTableRecord btr
        )
        {
            // Contador inicial
            int totalLabelsCreated = 0;

            // Capas por defecto
            HashSet<string> layersByDefault = new HashSet<string>
                {
                    "E-TRAY", "C-TRAY", "E-MESSENGER-WIRE",
                    "E-CAB", "E-DC-UGC", "E-DC-UGD"
                };

            List<Line> clonedEntities;
            Dictionary<ObjectId, Entity> originalEntities;
            HashSet<string> layersByUserSet;
            Dictionary<ObjectId, List<Line>> dictPolyIdExplodedLines;
            string layersFormMess = "Select the layers which contain the cable trenches.";
            // Obtenemos el graph
            cls_00_GraphClass.Graph graph = cls_00_GetGraphFromLayers.GetGraphFromLayers(
                db, tr, btr, layersByDefault, layersFormMess, out clonedEntities, 
                out originalEntities, out layersByUserSet, out dictPolyIdExplodedLines);
            // Validamos
            if (graph == null) return null;

            // Incluimos las intersecciones de esas entidades en el grafo
            cls_00_GraphTools.InjectEntityIntersectionsIntoGraph(clonedEntities, graph);

            // Iteramos por los ptos
            foreach (var kvp in graph.AdjacencyList)
            {
                // Obtenemos info
                cls_00_NodeClass.NodePoint np = kvp.Key;
                Point3d pt = np.Point;
                // Validamos 4 direcciones
                if (cls_00_IsIntersectCross.IsIntersectionCross(graph, np))
                {
                    // Leader 1: coordenadas
                    string coordText = $"X = {pt.X:F3}\nY = {pt.Y:F3}";
                    // Crear leader
                    cls_00_DrawMleader.DrawMLeaderOnPoint(
                        pt, coordText, tr, btr,
                        textHeight: 5.0, colorIndex: 1, layer: "0",
                        textStyle: "Standard", offsetX: 8.0, offsetY: 2.0
                    );
                    // Actualizamos contador
                    totalLabelsCreated++;

                    // Obtener capas que intersectan este punto
                    List<string> intersectingLayers = new List<string>();
                    foreach (var ent in originalEntities.Values)
                    {
                        // Revisar si el punto está dentro del bounding box de la entidad
                        Extents3d ext = ent.GeometricExtents;
                        if (ext.MinPoint.X - 0.001 <= pt.X && ext.MaxPoint.X + 0.001 >= pt.X &&
                            ext.MinPoint.Y - 0.001 <= pt.Y && ext.MaxPoint.Y + 0.001 >= pt.Y)
                        {
                            // Almacenamos
                            intersectingLayers.Add(ent.Layer);
                        }
                    }

                    // Leader 2: capas
                    if (intersectingLayers.Count > 0)
                    {
                        string layerText = "Layers:\n" + string.Join("\n", intersectingLayers.Distinct());
                        // Crear leader
                        cls_00_DrawMleader.DrawMLeaderOnPoint(
                            pt, layerText, tr, btr,
                            textHeight: 5, colorIndex: 2, layer: "0",
                            textStyle: "Standard", offsetX: 8.0, offsetY: -25.0
                        );
                        // Actualizamos contador
                        totalLabelsCreated++;
                    }
                }
            }

            // return
            return totalLabelsCreated;
        }

    }
}
