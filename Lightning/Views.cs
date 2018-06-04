using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.Transaction;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Autodesk.DesignScript.Runtime;
using System.Collections;
using Dynamo.Graph.Nodes;

namespace Spring
{
    public static class Views
    {
        /// <summary>
        /// Given a ViewFamilyType name as a string, attempts to find the ViewFamilyType in the Active Document.
        /// </summary>
        /// <param name="ViewFamilyType">The View Type as a string.</param>
        /// <returns>The ViewFamilyType as an element.</returns>
        /// <search>find, view, family, type, name, title</search>
        [NodeCategory("Query")]
        public static Revit.Elements.Element ViewFamilyType(string ViewFamilyType)
        {
            List<Autodesk.Revit.DB.Element> collector = new List<Autodesk.Revit.DB.Element>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = new FilteredElementCollector(doc);

            e.OfClass(typeof(ViewFamilyType));
            var elements = e.ToElements();

            Revit.Elements.Element hold = elements.Where(x => x.Name == ViewFamilyType).FirstOrDefault().ToDSType(true);
            if (hold == null)
            {
                return null;
            }
            else
                return hold;
        }

        /// <summary>
        /// This node will take a given list of ViewFamilyTypes and find the Views that those ViewFamilyTypes are applied.
        /// </summary>
        /// <param name="ViewFamilyType">The View Family Type.</param>
        /// <param name="Views">The Views to compare against.</param>
        /// <returns>A nested list of matching ViewFamilyTypes and Views.</returns>
        /// <search>compare, contrast, views, family, families, types, difference</search>
        [NodeCategory("Action")]
        [MultiReturn(new[] { "View Family Type Found", "View Family Type Not Found" })]
        public static Dictionary<string, List<List<Revit.Elements.Element>>> CompareViewFamilyTypes(List<Revit.Elements.Element> ViewFamilyType, List<Revit.Elements.Element> Views)
        {
            List<List<Revit.Elements.Element>> found = new List<List<Revit.Elements.Element>>();
            List<List<Revit.Elements.Element>> not = new List<List<Revit.Elements.Element>>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            foreach (Revit.Elements.Element k in Views)
            {
                if(k != null)
                {
                    List<Revit.Elements.Element> nest = new List<Revit.Elements.Element>();
                    nest.Clear();
                    ElementId searcher = Spring.Elements.UnwrapElement(k);
                    View finalView = doc.GetElement(searcher) as View;
                    ElementId viewID = finalView.GetTypeId();
                    if (viewID != null && searcher != null)
                    {
                        foreach (Revit.Elements.Element l in ViewFamilyType)
                        {
                            ElementId finder = Spring.Elements.UnwrapElement(l);
                            ViewFamilyType elements = doc.GetElement(finder) as ViewFamilyType;
                            Revit.Elements.Element oView = finalView.ToDSType(false);
                            Revit.Elements.Element eMent = elements.ToDSType(false);
                            nest.Add(oView);
                            nest.Add(eMent);
                            if (viewID == finder)
                            {
                                found.Add(nest);
                            }
                            else
                            {
                                not.Add(nest);
                            };
                        }
                    }

                }
            }
            return new Dictionary<string, List<List<Revit.Elements.Element>>>()
            {
                { "View Family Type Found", found },
                { "View Family Type Not Found", not }
            };

        }

        /// <summary>
        /// Given a Plan View, find the Associated Level or Generated Level of the View.
        /// </summary>
        /// <param name="View">Expecting a Plan View.</param>
        /// <returns>A Level to use.</returns>
        /// <search>associated, level, view, datum, host</search>
        [NodeCategory("Query")]
        public static Revit.Elements.Element AssociatedLevel(Revit.Elements.Element View)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            ElementId searcher = Spring.Elements.UnwrapElement(View);
            View finalView = doc.GetElement(searcher) as View;
            Revit.Elements.Element level = finalView.GenLevel.ToDSType(true);
            return level;
        }

        /// <summary>
        /// This node attempts to set the starting view and the preview image of the project to the project. The graph should save the project after this node.
        /// </summary>
        /// <param name="View">The View to set the Starting View.</param>
        /// <returns>IfSuccessful</returns>
        /// <search>start, starting, view, views, project, startup, standard</search>
        [NodeCategory("Action")]
        public static object SetStartingView(Revit.Elements.Element View)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc.IsFamilyDocument)
            {
                return new Exception("This node cannot be used on a Family Document. It may only be used on Projects or Project Templates.");
            }
            ElementId searcher = Spring.Elements.UnwrapElement(View);
            View finalView = doc.GetElement(searcher) as View;
            StartingViewSettings startingViewSettings = StartingViewSettings.GetStartingViewSettings(doc);

            if (finalView.Id == startingViewSettings.ViewId)
                return "Starting View is the same as input.";
            else
            {
                try
                {
                    if(startingViewSettings.ViewId.Equals(ElementId.InvalidElementId))
                    {
                        throw new Exception("The Document's existing Starting View is invalid.");
                    }
                    else if(finalView.ViewType != ViewType.ThreeD 
                        && finalView.ViewType != ViewType.FloorPlan 
                        && finalView.ViewType != ViewType.AreaPlan 
                        && finalView.ViewType != ViewType.CeilingPlan
                        && finalView.ViewType != ViewType.Detail
                        && finalView.ViewType != ViewType.DraftingView
                        && finalView.ViewType != ViewType.Elevation
                        && finalView.ViewType != ViewType.EngineeringPlan
                        && finalView.ViewType != ViewType.Section
                        && finalView.ViewType != ViewType.Schedule
                        )
                    {
                        throw new Exception("The View passed in to the node is not a valid type of view to set as a Starting View.");
                    }
                    else if(finalView.IsTemplate)
                    {
                        throw new Exception("The View passed in to the node is a View Template. This is not a valid view.");
                    }
                    else
                    {
                        // If valid, then set the viewId
                        startingViewSettings.ViewId = searcher;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Get the Starting View for this Project.
        /// </summary>
        /// <returns>The Starting View.</returns>
        /// <search>project, start, startup, starting, view, existing, standard</search>
        [NodeCategory("Query")]
        public static Revit.Elements.Element GetStartingView()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            StartingViewSettings startingViewSettings = StartingViewSettings.GetStartingViewSettings(doc);
            View finalView = doc.GetElement(startingViewSettings.ViewId) as View;
            Revit.Elements.Element view = finalView.ToDSType(true);
            return view;
        }

        /// <summary>
        /// Get a View in the Project by a specific name.
        /// </summary>
        /// <param name="ViewName"></param>
        /// <returns>The View, if found.</returns>
        /// <search>get, find, view, floor plan, section, elevation, views, 3D, title, name</search>
        [NodeCategory("Query")]
        public static Revit.Elements.Element GetbyName(string ViewName)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector views = new FilteredElementCollector(doc).OfClass(typeof(View));
            Revit.Elements.Element hold = views.Where(x => x.Name == ViewName).FirstOrDefault().ToDSType(true);
            return hold;
        }
    }
}
