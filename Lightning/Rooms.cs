using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spring
{
    public static class Rooms
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Unplaced Rooms", "Unplaced Room IDs", "Placed Rooms", "Placed Rooms IDs" })]
        public static Dictionary<string, object> PlacedUnplaced()
        {
            List<dynamic> unplacedelem = new List<dynamic>();
            List<dynamic> unplacedid = new List<dynamic>();
            List<dynamic> placedelem = new List<dynamic>();
            List<dynamic> placedID = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            IList<Autodesk.Revit.DB.Element> coll = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).ToElements();

            foreach(var i in coll)
            {
                if(i.Location == null)
                {
                    unplacedelem.Add(i.ToDSType(true));
                    unplacedid.Add(i.Id);
                }
                else
                {
                    placedelem.Add(i.ToDSType(true));
                    placedID.Add(i.Id);
                }
            }
            return new Dictionary<string, object>
            {
                {"Unplaced Rooms",unplacedelem },
                {"Unplaced Room IDs",unplacedid },
                {"Placed Rooms",placedelem },
                {"Placed Rooms IDs", placedID }
            };
        }

 //       collector = FilteredElementCollector(doc)

 //   collector.OfCategory(BuiltInCategory.OST_Rooms)

 //   famtypeitr = collector.GetElementIdIterator()

 //   famtypeitr.Reset()


	//for item in famtypeitr:
	//	elmID = item
 //       eleminst = doc.GetElement(elmID)


 //       room = eleminst
 //       level = ''
	//	for p in room.Parameters:
	//		if p.Definition.Name == 'Level':			
	//			level = p.AsValueString()
	//			if (level is None):
	//				level = p.AsString()
	//			if (level is None):
	//				level = ''
    }
}
