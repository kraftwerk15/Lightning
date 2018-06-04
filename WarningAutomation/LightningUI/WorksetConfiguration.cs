using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using LightningUI.Utilities;
using Newtonsoft.Json;
using CoreNodeModels;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using Autodesk.Revit.DB;

namespace WorksetConfigure
{
    /// <summary>
    /// Dropdown for Workset Configuration.
    /// </summary>
    /// <returns>A list of the Workset Configurations of a project.</returns>
    /// <search>workset, configure, confirguration, list, editable, all, last, viewed, user, specify</search>
    [NodeName("Workset Configuration")]
    [NodeCategory("Lightning.Revit.Worksets")]
    [IsDesignScriptCompatible]
    public class WorksetConfigurationUI : DSDropDownBase
    {
        
        public WorksetConfigurationUI() : base("WorksetConfiguration") {}

        [JsonConstructor]
        public WorksetConfigurationUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base("WorksetConfiguration", inPorts, outPorts) {}
        
        public Type EnumerationType = null;

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            // The Items collection contains the elements
            // that appear in the list. For this example, we
            // clear the list before adding new items, but you
            // can also use the PopulateItems method to add items
            // to the list.
            //if (EnumerationType == null) return SelectionState.Restore;
            
            Items.Clear();

            // Create a number of DynamoDropDownItem objects 
            // to store the items that we want to appear in our list.

            //var newItems = new List<DynamoDropDownItem>()
            //{
            //     new DynamoDropDownItem("Johnson", 0),
            //     new DynamoDropDownItem("Cersei", 1),
            //     new DynamoDropDownItem("Hodor",2)
            //};
            var strings = Enum.GetNames(typeof(SimpleWorksetConfiguration));
            Console.WriteLine(strings);
            for (var i=0; i < strings.Length; i++)
            {
                Items.Add(new DynamoDropDownItem(strings[i], strings[i]));
            }
            //foreach (var name in strings)
            //{
            //    Items.Add(new DynamoDropDownItem(name);
            //}

            //Items.AddRange(newItems);
            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            // Set the selected index to something other
            // than -1, the default, so that your list
            // has a pre-selection.

            SelectedIndex = 0;
            return SelectionState.Done;
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // Build an AST node for the type of object contained in your Items collection.
            var stringNode = AstFactory.BuildStringNode(Items[SelectedIndex].Item.ToString());
            var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), stringNode);
            return new List<AssociativeNode> { assign };
        }
    }
}
