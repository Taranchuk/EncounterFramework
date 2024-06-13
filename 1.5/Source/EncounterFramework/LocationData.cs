using RimWorld.Planet;
using System.IO;
using System.Xml;
using Verse;

namespace EncounterFramework
{
    public class PawnAmountOption
    {
        public PawnKindDef kind;

        public IntRange amount;
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "kind", xmlRoot.Name);
            amount = ParseHelper.FromString<IntRange>(xmlRoot.FirstChild.Value);
        }
    }

    public class ThingAmountOption
    {
        public ThingDef thing;

        public IntRange amount;
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", xmlRoot.Name);
            amount = ParseHelper.FromString<IntRange>(xmlRoot.FirstChild.Value);
        }
    }

    public class LocationData
    {
        public LocationDef locationDef;
        public FileInfo file;
        public MapParent mapParent;
        public LocationData(LocationDef locationDef, FileInfo file, MapParent mapParent = null)
        {
            this.file = file;
            this.locationDef = locationDef;
            this.mapParent = mapParent;
        }
    }
}