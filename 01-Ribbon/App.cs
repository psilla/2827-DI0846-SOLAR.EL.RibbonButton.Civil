using Autodesk.AutoCAD.Runtime;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Windows.Forms;
using SOLAR.EL.RibbonButton.Civil.Properties;
using SOLAR.EL.RibbonButton.Autocad.Buttons;

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

                // CREATE ENTITY LABELS 

                Autodesk.Windows.RibbonButton button1 = CreateRibbonButton(
                    name: "Create Entity Labels",
                    text: "Create Entity Labels",
                    image: Resources.trackLabels,
                    commandParameter: "CreateEntityLabels",
                    tooltipTitle: "Create Entity Labels",
                    tooltipContent: "Creates labels for Trackers inside detected regions in active document."
                );

                // Añadimos button
                rps1.Items.Add(button1);

                // Separador visual
                rps1.Items.Add(new Autodesk.Windows.RibbonSeparator());

                // CREATE ENTITY LABELS BACK

                Autodesk.Windows.RibbonButton button2 = CreateRibbonButton(
                    name: "Create Entity Labels Back",
                    text: "Create Entity Labels Back",
                    image: Resources.trackLabels,
                    commandParameter: "CreateEntityLabelsBack",
                    tooltipTitle: "Create Entity Labels Back",
                    tooltipContent: "Creates labels for Trackers inside detected regions in selected files."
                );

                // Añadimos button
                rps1.Items.Add(button2);

                // Separador visual
                rps1.Items.Add(new Autodesk.Windows.RibbonSeparator());

                // EXPORT LABELS TO EXCEL 

                Autodesk.Windows.RibbonButton button3 = CreateRibbonButton(
                    name: "Export Labels To Excel",
                    text: "Export Labels To Excel",
                    image: Resources.excelFile,
                    commandParameter: "ExportLabelsToExcel",
                    tooltipTitle: "Export Labels To Excel",
                    tooltipContent: "Exports labels of Trackers to Excel."
                );

                // Añadimos button
                rps1.Items.Add(button3);

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
                        case "CreateEntityLabels":
                            // Instanciamos la clase
                            cls_12_ButtonCreateEntityLabels.CreateEntityLabels();
                            break;

                        case "CreateEntityLabelsBack":
                            // Instanciamos la clase
                            cls_12_ButtonCreateEntityLabelsBack.CreateEntityLabelsBack();
                            break;

                        case "ExportLabelsToExcel":
                            // Instanciamos la clase
                            cls_13_ButtonExportLabelsToExcel.ExportLabelsToExcel();
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

















    }
}