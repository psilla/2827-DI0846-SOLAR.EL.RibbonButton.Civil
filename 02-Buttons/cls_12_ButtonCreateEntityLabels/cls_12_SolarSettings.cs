namespace SOLAR.EL.RibbonButton.Civil.Buttons
{
    public class SolarSettings
    {
        // Etiquetas
        public string ContGenTag { get; set; } = "Centros de Transformacion";
        public string ContInvTag { get; set; } = "Inversores";
        public string TrackTag { get; set; } = "Trackers";

        // Capas
        public string ContGenLayer { get; set; } = "_SP-E-CONTORNO-CT";
        public string ContInvLayer { get; set; } = "_SP-E-CONTORNO-INVERSOR";
        public string TrackLayer { get; set; } = "_SP-S-TRACKER";
        public string StringLayer { get; set; } = "_SP-S-STRING";
        public string LabelsLayer { get; set; } = "_SP-E-STRINGS-TXT";

        // Tipologías Tracker
        public string TipTrack { get; set; } = "Tracker";
        public string TipEstFija { get; set; } = "Estructura Fija";

        // Estilo label
        public string LabelStyle { get; set; } = "TYPSA_Arial_1.8mm";

        // Propiedades Prefijos
        public string ContGenProp { get; set; } = "Prefijo Centro Transformacion";
        public string ContInvProp { get; set; } = "Prefijo Inversor";
        public string TrackProp { get; set; } = "Prefijo Tracker";
        public string StringProp { get; set; } = "Prefijo String";

        public static SolarSettings GetDefaultSolarSettings()
        {
            return new SolarSettings();
        }


    }
}
