using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace SOLAR.EL.RibbonButton.Autocad.Settings
{
    public class CableCtInfo
    {
        public double CableLength { get; set; }
        public List<ObjectId> RelatedLabels { get; set; } = new List<ObjectId>();
    }
    public class EntityExcelRow
    {
        // Str
        public string StringHandle { get; set; }
        public string StringLayer { get; set; }
        public string StringLabel { get; set; }
        // Inv
        public string InverterHandle { get; set; }
        public string InverterLayer { get; set; }
        public string InverterLabel { get; set; }
        // CT
        public string CtLabelFrom { get; set; }
        public string CtLabelTo { get; set; }
        // Cable
        public string CableHandle { get; set; }
        public string CableLayer { get; set; }
        public object CableLength { get; set; }
    }
    public class SolarSettings
    {
        // Etiquetas
        public string PolyCtTag { get; set; } = "Centros de Transformacion (Polylines)";
        public string PolyInvTag { get; set; } = "Inversores (Polylines)";
        public string PolyStringTag { get; set; } = "Strings (Polylines)";
        public string BlockRefCtTag { get; set; } = "Centros de Transformacion (BlockRef)";
        public string BlockRefInvTag { get; set; } = "Inversores (BlockRef)";
        public string BlockRefTrackTag { get; set; } = "Trackers (BlockRef)";
        public string LabelCtTag { get; set; } = "Centros de Transformacion (Tags)";
        public string LabelInvTag { get; set; } = "Inversores (Tags)";
        public string LabelStringTag { get; set; } = "Strings (Tags)";
        public string CableCtToEstTag { get; set; } = "Cables CT-Subestación (Polylines)";
        public string CableInvToCtTag { get; set; } = "Cables Inversor-CT (Polylines)";
        public string CableStringToInvTag { get; set; } = "Cables String-Inversor (Polylines)";
        
        // Capas
        public string PolyCtLayer { get; set; } = "_SP-E-CONTORNO-CT";
        public string PolyInvLayer { get; set; } = "_SP-E-CONTORNO-INVERSOR";
        public string PolyStringLayer { get; set; } = "_SP-S-STRING";
        public string BlockRefCtLayer { get; set; } = "_SP-EQ-CT";
        public string BlockRefInvLayer { get; set; } = "_SP-E-EQ-INVERSOR-16SP STRINGS";
        public string BlockRefTrackLayer { get; set; } = "_SP-S-TRACKER";
        public string LabelCtLayer { get; set; } = "_SP-E-EQ-CT-TEXTOS";
        public string LabelInvLayer { get; set; } = "_SP-E-INVERSOR-TXT";
        public string LabelStringLayer { get; set; } = "_SP-E-STRINGS-TXT";
        public string CableCtToEstLayer { get; set; } = "_SP-E-CABLE-MT-1";
        public string CableInvToCtLayer { get; set; } = "_SP-E-CABLE-N2";
        public string CableStringToInvLayer { get; set; } = "_SP-E-CABLE-N1-";

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

        // Separadores válidos
        public char[] ValidSeparators { get; set; } = { '.', '-', '_', ',', ';' };

        public static SolarSettings GetDefaultSolarSettings()
        {
            return new SolarSettings();
        }
    }
}
