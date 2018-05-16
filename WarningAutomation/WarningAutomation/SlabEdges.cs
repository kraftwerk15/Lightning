using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Spring
{

    public static class SlabEdges
    {
        /// <summary>
        /// Create a Slab Edge by a given curve and slab edge type.
        /// </summary>
        /// <param name="Line">Curves</param>
        /// <param name="slabEdgeType">Slab Edge Type</param>
        /// <returns>Created Slab Edges.</returns>
        [NodeName("SlabEdgeByCurve")]
        [NodeCategory("Create")]
        public static List<SlabEdge> CreateSlabEdge(List<List<Autodesk.DesignScript.Geometry.Geometry>> Line, Revit.Elements.Element slabEdgeType)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            List<SlabEdge> slab = new List<SlabEdge>();
            ElementId id = Elements.UnwrapElement(slabEdgeType);
            SlabEdgeType unwrapSlabEdge = doc.GetElement(id) as SlabEdgeType;
            Console.WriteLine(id);
            
            try
            {
                foreach(List<Autodesk.DesignScript.Geometry.Geometry> k in Line)
                {
                    ReferenceArray referenceArray = new ReferenceArray();
                    referenceArray.Clear();
                    //IEnumerable<ReferenceArray> reference = Line as IEnumerable<ReferenceArray>;
                    foreach (Autodesk.DesignScript.Geometry.Curve host in k)
                    {
                        //var curve = host.ToRevitType();
                        //var host = curve.ToProtoType();
                        //Reference j = host as Reference;
                        Curve curve = host.ToRevitType();
                        Reference refc = curve.Reference;
                        referenceArray.Append(refc);
                    }
                    TransactionManager.Instance.EnsureInTransaction(doc);
                    SlabEdge slabcreator = doc.Create.NewSlabEdge(unwrapSlabEdge, referenceArray);
                    slabcreator.ToDSType(true);
                    slab.Add(slabcreator);
                    TransactionManager.Instance.TransactionTaskDone();
                }
                return slab;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Find Slabe Edge Types by Name.
        /// </summary>
        /// <param name="TypeName">The Type Name to find.</param>
        /// <returns></returns>
        [NodeName("SlabEdgeTypeByName")]
        [NodeCategory("Query")]
        public static Revit.Elements.Element SlabEdgeTypeByName(string TypeName)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            try
            {
                SlabEdgeType hold = new FilteredElementCollector(doc).OfClass(typeof(SlabEdgeType)).OfType<SlabEdgeType>().FirstOrDefault(f => f.Name.Equals(TypeName));
                return hold.ToDSType(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
