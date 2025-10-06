using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Civil.GetEntityCoordinates;

namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    internal class cls_12_ProcessCentroTrans
    {
        public static int ProcessCentroTrans(
            Region regionContGen,
            Transaction tr,
            BlockTableRecord btr,
            HashSet<ObjectId> contInvIds,
            Dictionary<Handle, Handle> dictPolyToRegionContInv,
            List<Region> validRegionContInv,
            HashSet<ObjectId> trackersIds,
            string trackTag,
            string stringLayer,
            Dictionary<string, string> propPreDict,
            string contGenProp, int contGenIndex,
            string contInvProp,
            string trackProp,
            string stringProp,
            bool isHorizontal,
            string labelsLayer,
            string chosenStyle,
            AttachmentPoint chosenJustification,
            StringBuilder infoRegiones,
            bool dolarSelBool
        )
        {
            // Contador de etiquetas creadas en todo el Contorno General
            int labelsCreatedTotal = 0;

            // Obtenemos Inversores
            List<Entity> contInvEntities = cls_12_GetInvInRegGen.GetInvInRegGen(
                tr, regionContGen, contInvIds, infoRegiones
            );
            // Validamos
            if (contInvEntities == null || contInvEntities.Count == 0) return 0;

            // Ordenamos Inversores por centroide
            contInvEntities.Sort((a, b) =>
                cls_00_GetEntityCentroid.CompareEntitiesByPosition(a, b, 10.0));

            // Obtener regiones de inversores ya ordenadas
            List<Region> orderedInvRegions =
                cls_12_GetOrderedInvReg.GetOrderedInverterRegions(
                    contInvEntities, dictPolyToRegionContInv, validRegionContInv, infoRegiones
                );

            // Mostramos
            infoRegiones.AppendLine($"\t• Total Inverter Regions: {orderedInvRegions.Count}");

            // Contador de inversores dentro del Contorno General
            int invIndex = 1;
            // Contador de trackers dentro del Contorno General
            int trackIndex = 1;
            // Iteramos por las regiones de los Inversores
            foreach (Region invRegion in orderedInvRegions)
            {
                // Procesamos Inverter
                int labelsCreated = cls_12_ProcessInverter.ProcessInverter(
                    invRegion, tr, btr, trackersIds, trackTag, stringLayer,
                    propPreDict, contGenProp, contGenIndex,
                    contInvProp, invIndex,
                    trackProp, ref trackIndex,
                    stringProp, isHorizontal, labelsLayer,
                    chosenStyle, chosenJustification, infoRegiones,
                    dolarSelBool
                );
                // Actualizamos contador
                invIndex++;

                // Acumulamos etiquetas
                labelsCreatedTotal += labelsCreated;
            }
            // return
            return labelsCreatedTotal;
        }




    }
}
