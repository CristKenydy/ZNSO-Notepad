using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ZNSO_Notepad.Services
{
    [XmlRoot("UIElementData")]
    public class UIElementData
    {
        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("X")]
        public double X { get; set; }

        [XmlAttribute("Y")]
        public double Y { get; set; }

        [XmlAttribute("Width")]
        public double Width { get; set; }

        [XmlAttribute("Height")]
        public double Height { get; set; }

        [XmlElement("Content")]
        public string Content { get; set; }

        [XmlAttribute("ContentFormat")]
        public string ContentFormat { get; set; }

        [XmlAttribute("Header")]
        public string Header { get; set; }
    }
}
