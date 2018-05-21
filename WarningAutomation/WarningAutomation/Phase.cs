using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace Spring
{
    public static class Phase
    {
        /// <summary>
        /// Base method for getting the phase status
        /// </summary>
        /// <param name="element">Expecting an ElementId.</param>
        /// <param name="phaseID">Expecting an ElementId.</param>
        /// <returns>The Phase Status as a string. Changing from enum.</returns>
        [IsVisibleInDynamoLibrary(false)]
        internal static string GetPhaseStatus(ElementId element, ElementId phaseID)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Element elemId = doc.GetElement(element) as Autodesk.Revit.DB.Element;
            ElementOnPhaseStatus elemStat = elemId.GetPhaseStatus(phaseID);
            string phaseStatus = elemStat.ToString();
            return phaseStatus;
        }
    }
}
