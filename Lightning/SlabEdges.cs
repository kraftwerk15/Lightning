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
        /// <param name="Floor">Curves</param>
        /// <param name="SlabEdgeType">Slab Edge Type</param>
        /// <returns>Created Slab Edges.</returns>
        /// <search>create, slab, edge, reference, line, edge, curve</search>
        [NodeName("SlabEdgeByCurve")]
        [NodeCategory("Create")]
        public static List<SlabEdge> ByReferenceArray(Revit.Elements.Element Floor, Revit.Elements.Element SlabEdgeType)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            List<SlabEdge> slab = new List<SlabEdge>();
            ElementId id = Elements.UnwrapElement(SlabEdgeType);
            SlabEdgeType unwrapSlabEdge = doc.GetElement(id) as SlabEdgeType;
            Console.WriteLine(id);
            ElementId floorId = Elements.UnwrapElement(Floor);
            Autodesk.Revit.DB.Floor elem = doc.GetElement(floorId) as Autodesk.Revit.DB.Floor;
            Options geomOptions = new Options();
            GeometryElement solid = elem.get_Geometry(geomOptions);
            
            try
            {
                foreach(GeometryElement k in solid)
                {

                    ReferenceArray referenceArray = new ReferenceArray();
                    referenceArray.Clear();
                    //IEnumerable<ReferenceArray> reference = Line as IEnumerable<ReferenceArray>;
                    //foreach (Autodesk.DesignScript.Geometry.Curve host in k)
                    //{
                    //    //var curve = host.ToRevitType();
                    //    //var host = curve.ToProtoType();
                    //    //Reference j = host as Reference;
                    //    Curve curve = host.ToRevitType();
                    //    Reference refc = curve.Reference;
                    //    referenceArray.Append(refc);
                    //}
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

        //public static List<SlabEdge> ByCurveArray(List<Revit.Elements.Element> floor, Revit.Elements.Element SlabEdgeType)
        //{
        //    var doc = DocumentManager.Instance.CurrentDBDocument;
        //    List<SlabEdge> slab = new List<SlabEdge>();
        //    ElementId id = Elements.UnwrapElement(SlabEdgeType);
        //    SlabEdgeType unwrapSlabEdge = doc.GetElement(id) as SlabEdgeType;
        //    Console.WriteLine(id);
        //    foreach (Revit.Elements.Element liFloor in floor)
        //    {
        //        ElementId floorId = Elements.UnwrapElement(liFloor);
        //        Autodesk.Revit.DB.Floor elem = doc.GetElement(floorId) as Autodesk.Revit.DB.Floor;


        //        try
        //        {
        //            Options geomOptions = new Options();
        //            GeometryElement faceGeom = elem.get_Geometry(geomOptions);

        //            List<Face> topFaces = new List<Face>();

        //            foreach (Autodesk.Revit.DB.Floor small in Floor)
        //            {
        //                GeoElement geo = small.get_Geometry(geomOptions);
        //                GeometryObjectArray objects = geo.Objects;
        //                foreach (GeometryObject obj in objects)
        //                {
        //                    Solid solid = obj as Solid;
        //                    if (solid != null)
        //                    {
        //                        PlanarFace f = GetTopFace(solid);
        //                        if (null == f)
        //                        {
        //                            Debug.WriteLine(
        //                              Util.ElementDescription(small)
        //                              + " has no top face.");
        //                            ++nNullFaces;
        //                        }
        //                        topFaces.Add(f);
        //                    }
        //                }
        //            }

        //            foreach (GeometryObject geomObj in faceGeom)
        //            {
        //                Solid geomSolid = geomObj as Solid;
        //                if (null != geomSolid)
        //                {
        //                    foreach (Face geomFace in geomSolid.Faces)
        //                    {
        //                        face = geomFace;
        //                        break;
        //                    }
        //                    break;
        //                }
        //            }



        //            foreach (List<Autodesk.DesignScript.Geometry.Geometry> k in Line)
        //            {

        //            }
        //        }
        //        }
        //}

        /// <summary>
        /// Find Slabe Edge Types by Name.
        /// </summary>
        /// <param name="TypeName">The Type Name to find.</param>
        /// <returns>Slab Edge type if found</returns>
        /// <search>slab, edge, type, name, floor</search>
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

        //PlanarFace GetTopFace(Solid solid)
        //{
        //    PlanarFace topFace = null;
        //    FaceArray faces = solid.Faces;
        //    foreach (Face f in faces)
        //    {
        //        PlanarFace pf = f as PlanarFace;
        //        if (null != pf
        //          && (Math.Abs(pf.FaceNormal.X - 0) < _eps
        //          && Math.Abs(pf.Normal.Y - 0) < _eps))
        //        {
        //            if ((null == topFace)
        //              || (topFace.Origin.Z < pf.Origin.Z))
        //            {
        //                topFace = pf;
        //            }
        //        }
        //    }
        //    return topFace;
        //}
    }
}
