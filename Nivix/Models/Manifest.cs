using System.Collections.Generic;
using System.Xml.Linq;
using Bazam.Modules;
using Melek.Models;
using System.Linq;

namespace Nivix.Models
{
    public class Manifest
    {
        public List<Package> Packages { get; set; }

        public static Manifest FromXML(XDocument xml)
        {
            Manifest retVal = new Manifest();

            retVal.Packages = (
                from p in xml.Root.Elements("package")
                select new Package() {
                    CardsReleased = XMLPal.GetDate(p.Attribute("cardsReleased")),
                    DataUpdated = XMLPal.GetDate(p.Attribute("dataUpdated")),
                    ID = XMLPal.GetString(p.Attribute("id")),
                    Name = XMLPal.GetString(p.Attribute("name"))
                }
            ).ToList();

            return retVal;
        }

        public XDocument ToXML()
        {
            List<XElement> packageElements = new List<XElement>();
            foreach (Package p in Packages) {
                packageElements.Add(
                    new XElement(
                        "package",
                        new XAttribute("id", p.ID),
                        new XAttribute("name", p.Name),
                        new XAttribute("cardsReleased", p.CardsReleased),
                        new XAttribute("dataUpdated", p.DataUpdated)
                    )
                );
            }
            XDocument doc = new XDocument(new XElement("packages", packageElements));
            return doc;
        }
    }
}