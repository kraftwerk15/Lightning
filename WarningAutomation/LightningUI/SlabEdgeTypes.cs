using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes;
using Autodesk.Revit.DB;
using CoreNodeModels;
//using DSRevitNodesUI;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Dynamo.Utilities;
using LightningUI.Utilities;

namespace SlabEdgeTypes
{
    public class SlabEdges
    {
        /// <summary>
        /// Dropdown listing of available Slab Edge Types in the Current Document.
        /// </summary>
        [NodeName("Slab Edge Types")]
        [NodeCategory("Lightning.Revit.SlabEdges")]
        [IsDesignScriptCompatible]
        public class SlabEdgeTypes : CustomGenericEnumerationDropDown
        {
            public SlabEdgeTypes() : base("SlabEdgeType", typeof(Autodesk.Revit.DB.SlabEdgeType)) { }


        //    private const string noSlabEdge = "No Slab Edge types available in project.";
        //    private SlabEdgeTypes() : base("Slab Edge Type") { }
        //    protected override SelectionState PopulateItemsCore(string currentSelection)
        //    {
        //        Items.Clear();
        //        //find all sheets in the project
        //        var elements = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(SlabEdgeType)).ToElements();
        //        if (!elements.Any())
        //        {
        //            Items.Add(new DynamoDropDownItem("SlabEdge", null));
        //            SelectedIndex = 0;
        //            return SelectionState.Done;
        //        }
        //        Items = elements.Select(x => new DynamoDropDownItem((x.Name), x)).OrderBy(x => x.Name).ToObservableCollection();
        //        return SelectionState.Restore;
        //    }
        //    public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        //    {
        //        if (Items.Count == 0 || Items[0].Name == noSlabEdge || SelectedIndex == -1)
        //        {
        //            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
        //        }
        //        var node = AstFactory.BuildFunctionCall(
        //            "Revit.Elements.ElementSelector",
        //            "ByElementId",
        //            new List<AssociativeNode>
        //            {
        //            AstFactory.BuildIntNode(((SlabEdgeType)Items[SelectedIndex].Item).Id.IntegerValue)
        //            });
        //        return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        //    }
        }

    }
}
