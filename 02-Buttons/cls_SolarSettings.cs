using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace SOLAR.EL.RibbonButton.Autocad.Settings
{
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
        public double CableLengthCorrectionFactor { get; set; }
        public object CableExtraLength { get; set; }
        public double CableLengthFixedAllowance { get; set; }
        public object CableLengthCorrectedTotal { get; set; }
        public int NumberOfConductors { get; set; }
        public object TotalInstalledCableLength { get; set; }
        public List<ObjectId> RelatedLabels { get; set; } = new List<ObjectId>();
    }

    public class SolarSettings
    {
        // Etiquetas
        public string PolyCtTag { get; set; } = "Centros de Transformacion (Polylines)";
        public string PolyInvTag { get; set; } = "Inversores (Polylines)";
        public string PolyStringTag { get; set; } = "Strings (Polylines)";
        public string BlockRefEstTag { get; set; } = "Subestación (BlockRef)";
        public string BlockRefCtTag { get; set; } = "Centros de Transformacion (BlockRef)";
        public string BlockRefInvTag { get; set; } = "Inversores (BlockRef)";
        public string BlockRefTrackTag { get; set; } = "Trackers (BlockRef)";
        public string LabelCtTag { get; set; } = "Centros de Transformacion (Tags)";
        public string LabelInvTag { get; set; } = "Inversores (Tags)";
        public string LabelStringTag { get; set; } = "Strings (Tags)";
        public string CableMVTag { get; set; } = "Cables CT-Subestación (Polylines)";
        public string CableN2Tag { get; set; } = "Cables Inversor-CT (Polylines)";
        public string CableN1Tag { get; set; } = "Cables String-Inversor (Polylines)";
        
        // Capas
        // Polys
        public string PolyCtLayer { get; set; } = "02.PV.EL_Grouping_CT";
        public string PolyInvLayer { get; set; } = "02.PV.EL_Grouping_INV";
        public string PolyStringLayer { get; set; } = "02.PV.EL_Grouping_String";
        public string CableCtToEstLayer { get; set; } = "04.PV.EL_Cable_MT_circuit_";
        public string CableInvToCtLayer { get; set; } = "02.PV.EL_Cable_N2";
        public string CableStringToInvLayer { get; set; } = "02.PV.EL_Cable_N1+";
        // BlockRef
        public string BlockRefEstLayer { get; set; } = "01.PV.EQ_Substation";
        public string BlockRefCtLayer { get; set; } = "01.PV.EQ_Transformation-centre";
        public string BlockRefInvLayer { get; set; } = "01.PV.EQ_INV_";
        public string BlockRefTrackLayer { get; set; } = "01.PV.ST_Tracker";
        // Tags
        public string LabelCtLayer { get; set; } = "01.PV.EQ_Transformation-centre_Text";
        public string LabelInvLayer { get; set; } = "01.PV.EQ_Power-inverter_Text";
        public string LabelStringLayer { get; set; } = "01.PV.EQ_String_Text";

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

        // Hojas Excel
        public string SheetNameStr { get; set; } = "Cables-N1";
        public string SheetNameInv { get; set; } = "Cables-N2";
        public string SheetNameCt { get; set; } = "Cables-MV";
        public string TableNameStr { get; set; } = "String";
        public string TableNameInv { get; set; } = "Inverter";
        public string TableNameCt { get; set; } = "CT";
        public string SummaryNameTrack { get; set; } = "Summary by Tracker";
        public string SummaryNameInv { get; set; } = "Summary by Inverter";
        public string SummaryNameCt { get; set; } = "Summary by CT";
        public string SummaryNameTrackKey { get; set; } = "summaryByTracker";
        public string SummaryNameInvKey { get; set; } = "summaryByInverter";
        public string SummaryNameCtKey { get; set; } = "summaryByCT";

        // Default values
        public string EntNoLabelValue { get; set; } = "Entidad sin etiqueta";
        public string EntMultiLabelValue { get; set; } = "Entidad con más de 1 etiqueta";
        public string EntNoCableValue { get; set; } = "Entidad sin cable conectado";
        public string EntMultiCableValue { get; set; } = "Entidad con más de 1 cable conectado";

        public static SolarSettings GetDefaultSolarSettings()
        {
            return new SolarSettings();
        }
    }
}
