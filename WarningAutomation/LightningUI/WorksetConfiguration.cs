using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightningUI.Utilities;

namespace WorksetConfirguration
{
    public static class WorksetConfiguration
    {
        [NodeName("Workset Configuration")]
        [NodeCategory("Action")]
        public class WorksetConfigureUI : CustomGenericEnumerationDropDown
        {
            public WorksetConfigureUI() : base("WorksetConfigure", typeof(Autodesk.Revit.DB.WorksetConfiguration)) { }
        }
    }
}
