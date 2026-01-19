using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TYPSA.SharedLib.Autocad.Buttons;
using TYPSA.SharedLib.Autocad.ProjectUnits;
using TYPSA.SharedLib.Excel;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_16_ButtonCablesFromEnt
    {
        public static class CablesFromEntityOptions
        {
            // Textos de opciones 
            public const string ProcesarStrings = "Procesar Strings";
            public const string ProcesarInversores = "Procesar Inversores";
            public const string ProcesarCT = "Procesar CT";

            // Lista completa para el formulario
            public static readonly List<string> AllOptions = new List<string>
            {
                ProcesarStrings,
                ProcesarInversores,
                ProcesarCT
            };

            // Opciones marcadas por defecto
            public static readonly HashSet<string> DefaultSelectedOptions =
                new HashSet<string>
                {
                    ProcesarStrings,
                    ProcesarInversores,
                    ProcesarCT
                };
        }

        [CommandMethod("CablesFromEntity")]
        public static void ButtonCablesFromEntity()
        {
            DateTime startTime = DateTime.Now;

            // Obtener codigo de proyecto
            string projectCode = cls_00_GetUserData.GetProjectCodeFromDialog();
            // Validamos
            if (projectCode == null) return;

            // Seleccionar el directorio del Excel
            string excelDirectory = cls_00_SelectExcelDirectory.SelectExcelDirectory();
            // Validamos
            if (string.IsNullOrEmpty(excelDirectory)) return;

            // Seleccionar el archivo de Excel
            string excelPath = cls_00_SelectExcelFile.SelectExcelFile(excelDirectory);
            // Validamos
            if (string.IsNullOrEmpty(excelPath)) return;

            // Form
            List<string> selectedOptions =
                InstanciarFormularios.CheckListBoxFormSelectedItemsOut(
                    "Seleccione las mediciones a realizar:",
                    CablesFromEntityOptions.AllOptions,
                    CablesFromEntityOptions.DefaultSelectedOptions
                );
            // Validamos
            if (selectedOptions == null || selectedOptions.Count == 0)
            {
                // Mensaje
                MessageBox.Show(
                    "No se seleccionó ninguna opción. Proceso cancelado.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                // Finalizamos
                return;
            }

            // Obtenemos las unidades del proyecto
            string projectUnits = cls_00_ProjectUnits.GetProjectUnits();

            // Strings
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarStrings))
            {
                cls_16_ButtonCablesFromString.ButtonCablesFromString(
                    projectCode, excelPath, projectUnits
                );
            }
            // Inverters
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarInversores))
            {
                cls_16_ButtonCablesFromInverter.ButtonCablesFromInverter(
                    projectCode, excelPath, projectUnits
                );
            }
            // CT
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarCT))
            {
                cls_16_ButtonCablesFromCT.ButtonCablesFromCT(
                    projectCode, excelPath, projectUnits
                );
            }

            // Mostrar tiempos
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            // Mensaje
            MessageBox.Show(
                $"Proceso completado correctamente.\n\n" +
                $"Duración: {duration:hh\\:mm\\:ss}\n" +
                $"Inicio: {startTime:HH:mm:ss}\n" +
                $"Fin: {endTime:HH:mm:ss}",
                "Proceso finalizado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}
