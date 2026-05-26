using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using SOLAR.EL.RibbonButton.Autocad.Settings;
using TYPSA.SharedLib.Autocad.Buttons;
using TYPSA.SharedLib.Autocad.ObjectsByTypeByLayer;
using TYPSA.SharedLib.Autocad.ProjectUnits;
using TYPSA.SharedLib.ExcelAutocad;
using TYPSA.SharedLib.UserForms;

namespace SOLAR.EL.RibbonButton.Autocad.Buttons
{
    internal class cls_16_ButtonMeasureCables
    {
        private static class CableFormKeys
        {
            public const string Factor = "Enter the Length Correction Factor:";
            public const string Allowance = "Enter the Fixed Length Allowance (in Project Units)";
            public const string Conductors = "Enter the Number of Conductors";

            // Generador de keys
            public static string GetFactorKey(string cable) => $"{cable} - {Factor}";

            public static string GetAllowanceKey(string cable) => $"{cable} - {Allowance}";

            public static string GetConductorsKey(string cable) => $"{cable} - {Conductors}";

            // Detectores
            public static bool IsFactor(string key) => key.Contains(Factor);

            public static bool IsAllowance(string key) => key.Contains(Allowance);

            public static bool IsConductors(string key) => key.Contains(Conductors);

            // Obtener base (N1, N2, MV)
            public static string GetBaseKey(string key)
            {
                return key.Split('-')[0].Trim();
            }
        }

        private static class CablesFromEntityOptions
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

            string projectCode = cls_00_GetUserData.GetProjectCodeFromDialog();
            // Validamos
            if (projectCode == null) return;

            string excelDirectory = cls_00_SelectExcelDirectory.SelectExcelDirectory();
            // Validamos
            if (string.IsNullOrEmpty(excelDirectory)) return;

            string excelPath = cls_00_SelectExcelFile.SelectExcelFile(excelDirectory);
            // Validamos
            if (string.IsNullOrEmpty(excelPath)) return;

            // -------------------------------
            // Form Opciones
            // -------------------------------

            List<string> selectedOptions = cls_00_InstaForm_CheckedListBox.CheckListBoxFormSearchOut(
                "Select the measurements to be performed:", CablesFromEntityOptions.AllOptions,
                CablesFromEntityOptions.DefaultSelectedOptions.ToList()
            );
            // Validamos
            if (selectedOptions == null || selectedOptions.Count == 0)
            {
                MessageBox.Show(
                    "No options were selected. The process has been cancelled.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information
                );
                return;
            }

            // -------------------------------
            // Diccionario combinado
            // -------------------------------

            Dictionary<string, string> combinedDict = new Dictionary<string, string>();

            void AddCable(string key, string factor, string allowance, string conductors)
            {
                combinedDict[CableFormKeys.GetFactorKey(key)] = factor;
                combinedDict[CableFormKeys.GetAllowanceKey(key)] = allowance;
                combinedDict[CableFormKeys.GetConductorsKey(key)] = conductors;
            }

            // Añadimos valores por defecto
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarStrings))
                AddCable("N1", "1", "5", "2");

            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarInversores))
                AddCable("N2", "1", "5", "3");

            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarCT))
                AddCable("MV", "1", "5", "3");

            // -------------------------------
            // Form
            // -------------------------------

            Dictionary<string, string> result = cls_00_InstaForm_TextBox.TextBoxFormOut_NextToLabel_String(
                "Define Cable parameters:", combinedDict
            );
            // Validamos
            if (result == null)
            {
                MessageBox.Show(
                    "Input cancelled. The process has been cancelled.", "Cancelled",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
                return;
            }

            // -------------------------------
            // Parsear
            // -------------------------------

            Dictionary<string, double> dictFactor = new Dictionary<string, double>();
            Dictionary<string, double> dictAllowance = new Dictionary<string, double>();
            Dictionary<string, int> dictConductors = new Dictionary<string, int>();

            foreach (var kvp in result)
            {
                string key = kvp.Key;
                string value = kvp.Value;

                string baseKey = CableFormKeys.GetBaseKey(key);

                if (CableFormKeys.IsFactor(key))
                {
                    if (!double.TryParse(value, out double val))
                    {
                        MessageBox.Show($"Invalid factor for {baseKey}");
                        return;
                    }
                    dictFactor[baseKey] = val;
                }
                else if (CableFormKeys.IsAllowance(key))
                {
                    if (!double.TryParse(value, out double val))
                    {
                        MessageBox.Show($"Invalid allowance for {baseKey}");
                        return;
                    }
                    dictAllowance[baseKey] = val;
                }
                else if (CableFormKeys.IsConductors(key))
                {
                    if (!int.TryParse(value, out int val))
                    {
                        MessageBox.Show($"Invalid number of conductors for {baseKey}");
                        return;
                    }
                    dictConductors[baseKey] = val;
                }
            }

            // -----------------------------
            // Obtener settings
            // -----------------------------

            SolarSettings solarSet = SolarSettings.GetDefaultSolarSettings();
            AutocadSettings autoSettings = AutocadSettings.GetDefaultSettings();

            // -------------------------------
            // Ejecutar
            // -------------------------------

            // Obtenemos las unidades del proyecto
            string projectUnits = cls_00_ProjectUnits.GetProjectUnits();
            // Validamos N1
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarStrings))
            {
                cls_16_ButtonMeasureCablesN1.ButtonMeasureCablesN1(
                    projectCode, excelPath, projectUnits, 
                    dictFactor["N1"], dictAllowance["N1"], dictConductors["N1"], solarSet, autoSettings
                );
            }

            // Validamos N2
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarInversores))
            {
                cls_16_ButtonMeasureCablesN2.ButtonMeasureCablesN2(
                    projectCode, excelPath, projectUnits, 
                    dictFactor["N2"], dictAllowance["N2"], dictConductors["N2"], solarSet, autoSettings
                );
            }

            // Validamos MV
            if (selectedOptions.Contains(CablesFromEntityOptions.ProcesarCT))
            {
                cls_16_ButtonMeasureCablesMV.ButtonMeasureCablesMV(
                    projectCode, excelPath, projectUnits, 
                    dictFactor["MV"], dictAllowance["MV"], dictConductors["MV"], solarSet
                );
            }

            // -------------------------------
            // Tiempos
            // -------------------------------

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

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
