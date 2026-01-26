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
    internal class cls_16_ButtonMeasureCables
    {
        public  static class CablesFromEntityOptions
        {
            // Opciones
            public const string ProcesarStrings = "Cables-N1";
            public const string ProcesarInversores = "Cables-N2";
            public const string ProcesarCT = "Cables-MV";
            // Lista completa para el formulario
            public static readonly List<string> AllOptions = new List<string>
            {
                ProcesarStrings, ProcesarInversores, ProcesarCT
            };

            // Opciones marcadas por defecto
            public static readonly HashSet<string> DefaultSelectedOptions = new HashSet<string>
            {
                ProcesarStrings, ProcesarInversores, ProcesarCT
            };
        }

        [CommandMethod("CablesFromEntity")]
        public static void ButtonMeasureCables()
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
            List<string> selectedOptions = InstanciarFormularios.CheckListBoxFormSelectedItemsOut(
                "Select the measurements to be performed:",
                CablesFromEntityOptions.AllOptions,
                CablesFromEntityOptions.DefaultSelectedOptions
            );
            // Validamos
            if (selectedOptions == null || selectedOptions.Count == 0)
            {
                // Mensaje
                MessageBox.Show(
                    "No options were selected. The process has been cancelled.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information
                );
                // Finalizamos
                return;
            }

            Dictionary<string, double> dictCableLengthCorrectionFactor = new Dictionary<string, double>();
            Dictionary<string, double> dictCableLengthFixedAllowance = new Dictionary<string, double>();
            Dictionary<string, int> dictNumberOfConductors = new Dictionary<string, int>();
            // Coeficientes para longitudes
            // N1
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarStrings))
            {
                dictCableLengthCorrectionFactor["N1"] = 1;
                dictCableLengthFixedAllowance["N1"] = 5;
                dictNumberOfConductors["N1"] = 2;
            }
            // N2
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarInversores))
            {
                dictCableLengthCorrectionFactor["N2"] = 1;
                dictCableLengthFixedAllowance["N2"] = 5;
                dictNumberOfConductors["N2"] = 3;
            }
            // MV
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarCT))
            {
                dictCableLengthCorrectionFactor["MV"] = 1;
                dictCableLengthFixedAllowance["MV"] = 5;
                dictNumberOfConductors["MV"] = 3;
            }
            // Validamos
            if (dictCableLengthCorrectionFactor.Count > 0)
            {
                // Form
                dictCableLengthCorrectionFactor = InstanciarFormularios.TextBoxFormOut_NextToLabel_Double(
                    "Enter the length correction factor for each measurement:",
                    dictCableLengthCorrectionFactor
                );
                // Validamos
                if (dictCableLengthCorrectionFactor == null)
                {
                    // Mensaje
                    MessageBox.Show(
                        "No valid correction factors were provided. The process has been cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return;
                }
            }

            // Validamos
            if (dictCableLengthFixedAllowance.Count > 0)
            {
                // Form
                dictCableLengthFixedAllowance = InstanciarFormularios.TextBoxFormOut_NextToLabel_Double(
                    "Enter the fixed length allowance (in project units) for each measurement:",
                    dictCableLengthFixedAllowance
                );
                // Validamos
                if (dictCableLengthFixedAllowance == null)
                {
                    // Mensaje
                    MessageBox.Show(
                        "No valid length allowances were provided. The process has been cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return;
                }
            }

            // Validamos
            if (dictNumberOfConductors.Count > 0)
            {
                // Form
                dictNumberOfConductors = InstanciarFormularios.TextBoxFormOut_NextToLabel_Integer(
                    "Enter the number of conductors for each measurement:",
                    dictNumberOfConductors
                );
                // Validamos
                if (dictNumberOfConductors == null)
                {
                    // Mensaje
                    MessageBox.Show(
                        "No valid number of conductors was provided. The process has been cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                    // Finalizamos
                    return;
                }
            }

            // Obtenemos las unidades del proyecto
            string projectUnits = cls_00_ProjectUnits.GetProjectUnits();

            // N1
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarStrings))
            {
                double cableLengthCorrectionFactor = dictCableLengthCorrectionFactor["N1"];
                double cableLengthFixedAllowance = dictCableLengthFixedAllowance["N1"];
                int cableNumberOfConductors = dictNumberOfConductors["N1"];
                cls_16_ButtonMeasureCablesN1.ButtonMeasureCablesN1(
                    projectCode, excelPath, projectUnits, cableLengthCorrectionFactor, 
                    cableLengthFixedAllowance, cableNumberOfConductors
                );
            }
            // N2
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarInversores))
            {
                double cableLengthCorrectionFactor = dictCableLengthCorrectionFactor["N2"];
                double cableLengthFixedAllowance = dictCableLengthFixedAllowance["N2"];
                int cableNumberOfConductors = dictNumberOfConductors["N2"];
                cls_16_ButtonMeasureCablesN2.ButtonMeasureCablesN2(
                    projectCode, excelPath, projectUnits, cableLengthCorrectionFactor, 
                    cableLengthFixedAllowance, cableNumberOfConductors
                );
            }
            // MV
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarCT))
            {
                double cableLengthCorrectionFactor = dictCableLengthCorrectionFactor["MV"];
                double cableLengthFixedAllowance = dictCableLengthFixedAllowance["MV"];
                int cableNumberOfConductors = dictNumberOfConductors["MV"];
                cls_16_ButtonMeasureCablesMV.ButtonMeasureCablesMV(
                    projectCode, excelPath, projectUnits, cableLengthCorrectionFactor, 
                    cableLengthFixedAllowance, cableNumberOfConductors
                );
            }

            // Mostrar tiempos
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            // Mensaje
            MessageBox.Show(
                $"Process completed successfully.\n\n" +
                $"Duration: {duration:hh\\:mm\\:ss}\n" +
                $"Start time: {startTime:HH:mm:ss}\n" +
                $"End time: {endTime:HH:mm:ss}",
                "Process completed",
                MessageBoxButtons.OK, MessageBoxIcon.Information
            );
        }
    }
}
