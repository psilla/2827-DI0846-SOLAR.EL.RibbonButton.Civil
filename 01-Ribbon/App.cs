using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using SOLAR.EL.RibbonButton.Autocad.Buttons;
using _0000_XX0000_SOLAR.EL.RibbonButton.Civil.Properties;

namespace SOLAR.EL.RibbonButton.Autocad
{
    public class App : IExtensionApplication
    {
        public void Initialize()
        {

            LoadRibbon();
        }

        public void Terminate()
        {

        }

        private void LoadRibbon()
        {
            Autodesk.Windows.RibbonControl ribbonControl = Autodesk.Windows.ComponentManager.Ribbon;
            if (ribbonControl != null)
            {
                // Crear Ribbon
                Autodesk.Windows.RibbonTab rtab = new Autodesk.Windows.RibbonTab();
                rtab.Title = "SOLAR.EL";
                rtab.Id = "TESTRIBBON_TAB_ID";
                ribbonControl.Tabs.Add(rtab);

                // Crear panel
                Autodesk.Windows.RibbonPanelSource rps1 = new Autodesk.Windows.RibbonPanelSource();
                rps1.Title = "SOLAR UTILS"; // Título 
                Autodesk.Windows.RibbonPanel rp1 = new Autodesk.Windows.RibbonPanel();
                rp1.Source = rps1;
                rtab.Panels.Add(rp1);

                /////////////// ADDING BUTTONS //////////////////////

                // CREATE STRING LABELS 
                // Document
                Autodesk.Windows.RibbonButton button1 = CreateRibbonButton(
                    name: "Create String Labels Doc",
                    text: "Create String Labels",
                    image: Resources.trackLabels,
                    commandParameter: "CreateStringLabels",
                    tooltipTitle: "Create String Labels Doc",
                    tooltipContent: "Creates String labels in active document."
                );
                // Background
                Autodesk.Windows.RibbonButton button2 = CreateRibbonButton(
                    name: "Create String Labels Back",
                    text: "Create String Labels",
                    image: Resources.trackLabels,
                    commandParameter: "CreateStringLabelsBack",
                    tooltipTitle: "Create String Labels Back",
                    tooltipContent: "Creates String labels in selected files."
                );

                // Crear dropdown
                RibbonSplitButton entLabelsDropdown = CreateRibbonSplitButton(
                    "stringLabelsDropdown",
                    "String Labels",
                    Resources.trackLabels,
                    new List<Autodesk.Windows.RibbonButton> { button1, button2 }
                );

                // Añadimos button
                rps1.Items.Add(entLabelsDropdown);

                // Separador visual
                rps1.Items.Add(new Autodesk.Windows.RibbonSeparator());

                //// EXPORT STRING LABELS TO EXCEL 
                //// Document
                //Autodesk.Windows.RibbonButton button3 = CreateRibbonButton(
                //    name: "Export String Labels To Excel Doc",
                //    text: "Export String Labels To Excel",
                //    image: Resources.labExport,
                //    commandParameter: "ExportStringLabelsToExcel",
                //    tooltipTitle: "Export String Labels To Excel",
                //    tooltipContent: "Export to Excel String labels in active document."
                //);
                //// Background
                //Autodesk.Windows.RibbonButton button4 = CreateRibbonButton(
                //    name: "Export String Labels To Excel Back",
                //    text: "Export String Labels To Excel",
                //    image: Resources.labExport,
                //    commandParameter: "ExportStringLabelsToExcelBack",
                //    tooltipTitle: "Export String Labels To Excel Back",
                //    tooltipContent: "Export to Excel String labels in selected files."
                //);

                //// Crear dropdown
                //RibbonSplitButton entLabelsExcelDropdown = CreateRibbonSplitButton(
                //    "stringLabelsExcelDropdown",
                //    "String Labels Excel",
                //    Resources.labExport,
                //    new List<Autodesk.Windows.RibbonButton> { button3, button4 }
                //);

                //// Añadimos button
                //rps1.Items.Add(entLabelsExcelDropdown);

                //// Separador visual
                //rps1.Items.Add(new Autodesk.Windows.RibbonSeparator());

                //// CREATE STRING LABELS FROM EXCEL
                //// Document
                //Autodesk.Windows.RibbonButton button5 = CreateRibbonButton(
                //    name: "Create String Labels from Excel Doc",
                //    text: "Create String Labels from Excel",
                //    image: Resources.labImport,
                //    commandParameter: "CreateStringLabelsFromExcelDoc",
                //    tooltipTitle: "Create String Labels from Excel Doc",
                //    tooltipContent: "Creates from Excel String labels in active document."
                //);

                //// Añadimos button
                //rps1.Items.Add(button5);

                //// Separador visual
                //rps1.Items.Add(new Autodesk.Windows.RibbonSeparator());

                //// CREATE FLAGS FROM INTER
                //// Document
                //Autodesk.Windows.RibbonButton button6 = CreateRibbonButton(
                //    name: "Create Flags From Inter",
                //    text: "Create Flags From Inter",
                //    image: Resources.flagInter,
                //    commandParameter: "CreateFlagsFromInter",
                //    tooltipTitle: "Create Flags From Inter",
                //    tooltipContent: "Create Flags From Intersections between Cable Trenches."
                //);

                //// Añadimos button
                //rps1.Items.Add(button6);

                //// Separador visual
                //rps1.Items.Add(new Autodesk.Windows.RibbonSeparator());

                // CABLES FROM ENTITIES 
                Autodesk.Windows.RibbonButton button7 = CreateRibbonButton(
                    name: "Cables From Entity",
                    text: "Cables From Entity",
                    image: Resources.cableLength,
                    commandParameter: "CablesFromEntity",
                    tooltipTitle: "Cables From Entity",
                    tooltipContent: "Cables From Entity."
                );

                // Añadimos button
                rps1.Items.Add(button7);

                // Separador visual
                rps1.Items.Add(new Autodesk.Windows.RibbonSeparator());

                // Definimos el tab activo
                rtab.IsActive = true;
            }
        }

        // Define a command handler class implementing the ICommand interface for ribbon button actions.
        public class MyRibbonCommandHandler : System.Windows.Input.ICommand
        {
            // Determines whether the command can be executed. Always returns true in this case.
            public bool CanExecute(object parameter)
            {
                return true;
            }

            // Event that must be declared when implementing ICommand, 
            // but it's not used here. It's for handling changes in command execution state.
            public event EventHandler CanExecuteChanged;

            // Executes the actual command logic based on the parameter passed, typically a ribbon button.
            public void Execute(object parameter)
            {
                // Check if the parameter is a RibbonButton from Autodesk's UI components.
                if (parameter is Autodesk.Windows.RibbonButton ribbonButton)
                {
                    // Retrieve the command parameter from the ribbon button, expected to be a string.
                    string command = ribbonButton.CommandParameter as string;
                    switch (command)
                    {
                        case "CreateStringLabels":
                            // Instanciamos la clase
                            cls_12_ButtonCreateEntityLabels.ButtonCreateEntityLabels();
                            break;

                        case "CreateStringLabelsBack":
                            // Instanciamos la clase
                            cls_12_ButtonCreateEntityLabelsBack.ButtonCreateEntityLabelsBack();
                            break;

                        case "ExportStringLabelsToExcel":
                            // Instanciamos la clase
                            cls_13_ButtonExportLabelsToExcel.ButtonExportLabelsToExcel();
                            break;

                        case "ExportStringLabelsToExcelBack":
                            // Instanciamos la clase
                            cls_13_ButtonExportLabelsToExcelBack.ButtonExportLabelsToExcelBack();
                            break;

                        case "CreateLabelsFromExcel":
                            // Instanciamos la clase
                            cls_14_ButtonCreateLabelsFromExcel.ButtonCreateLabelsFromExcel();
                            break;

                        case "CreateFlagsFromInter":
                            // Instanciamos la clase
                            cls_15_ButtonCreateFlagsFromInter.ButtonCreateFlagsFromInter();
                            break;

                        case "CablesFromEntity":
                            // Instanciamos la clase
                            cls_16_ButtonCablesFromEnt.ButtonCablesFromEntity();
                            break;

                        // Default case for unhandled commands. No action is taken.
                        default:
                            break;
                    }
                }
            }
        }
        private BitmapImage GetImageSource(Image img)
        {
            try
            {
                if (img == null)
                {
                    throw new ArgumentNullException(nameof(img), "La imagen no puede ser null.");
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    Bitmap bitmap = new Bitmap(img);
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;

                    BitmapImage bmpImg = new BitmapImage();
                    bmpImg.BeginInit();
                    bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                    bmpImg.StreamSource = ms;
                    bmpImg.EndInit();
                    bmpImg.Freeze(); // Evita problemas de hilos en WPF y AutoCAD

                    return bmpImg;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"❌ ERROR en GetImageSource: {ex.Message}",
                    "Error de Conversión de Imagen"
                );
                return null;
            }
        }

        private Autodesk.Windows.RibbonButton CreateRibbonButton(
            string name,
            string text,
            Image image,
            string commandParameter,
            string tooltipTitle,
            string tooltipContent
        )
        {
            ImageSource imageSource = GetImageSource(image);

            Autodesk.Windows.RibbonButton button = new Autodesk.Windows.RibbonButton
            {
                Name = name,
                ShowText = true,
                Text = text,
                ShowImage = true,
                LargeImage = imageSource,
                Size = Autodesk.Windows.RibbonItemSize.Large,
                CommandHandler = new MyRibbonCommandHandler(),
                CommandParameter = commandParameter,
                // Tooltip
                ToolTip = new Autodesk.Windows.RibbonToolTip
                {
                    Title = tooltipTitle,
                    Content = tooltipContent,
                    IsHelpEnabled = false
                }
            };

            return button;
        }

        private Autodesk.Windows.RibbonSplitButton CreateRibbonSplitButton(
            string name,
            string text,
            Image image,
            List<Autodesk.Windows.RibbonButton> buttons
        )
        {
            ImageSource imageSource = GetImageSource(image);

            Autodesk.Windows.RibbonSplitButton splitButton =
                new Autodesk.Windows.RibbonSplitButton
                {
                    Name = name,
                    Text = text,
                    ShowImage = true,
                    ShowText = true,
                    Size = Autodesk.Windows.RibbonItemSize.Large,
                    LargeImage = imageSource
                };

            // Iteramos
            foreach (var button in buttons)
            {
                splitButton.Items.Add(button);
            }

            // return
            return splitButton;
        }

















    }
}