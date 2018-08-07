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
        /// <returns name="View Family Type Found">A list of View Family Types that were found.</returns>
        /// <returns name="View Family Type Not Found">A list of View Family Types that were not found.</returns>
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
                if (k != null)
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
                    if (startingViewSettings.ViewId.Equals(ElementId.InvalidElementId))
                    {
                        throw new Exception("The Document's existing Starting View is invalid.");
                    }
                    else if (finalView.ViewType != ViewType.ThreeD
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
                    else if (finalView.IsTemplate)
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

        /// <summary>
        /// Collect all views in the project. Omits View Templates.
        /// </summary>
        /// <returns name="Area View">Area View</returns>
        /// <returns name="Reflected Ceiling Plan">Reflected Ceiling Plan</returns>
        /// <returns name="Column Schedule">Column Schedule</returns>
        /// <returns name="Cost Report">Cost Report</returns>
        /// <returns name="Detail View">Detail View</returns>
        /// <returns name="Drafting View">Drafting View</returns>
        /// <returns name="Drawing Sheet">Drawing Sheet</returns>
        /// <returns name="Elevations">Elevations</returns>
        /// <returns name="Engineering Plan">Engineering Plan</returns>
        /// <returns name="Floor Plan">Floor Plan</returns>
        /// <returns name="Internal">Internal</returns>
        /// <returns name="Legend">Legend</returns>
        /// <returns name="Load Report">Load Report</returns>
        /// <returns name="Panel Schedule">Panel Schedule</returns>
        /// <returns name="Pressure Loss Report">Pressure Loss Report</returns>
        /// <returns name="Rendering">Rendering</returns>
        /// <returns name="Report">Report</returns>
        /// <returns name="Schedule">Schedule</returns>
        /// <returns name="Section">Section</returns>
        /// <returns name="3D">3D</returns>
        /// <returns name="Walkthrough">Walkthrough</returns>
        /// <returns name="Undefined">Undefined</returns>
        /// <search>view, collect, collector, get, views, find, report, schedule, floor, ceiling, sheet</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Area View",
            "Reflected Ceiling Plan",
            "Column Schedule",
            "Cost Report" ,
            "Detail View" ,
            "Drafting View" ,
            "Drawing Sheet" ,
            "Elevations" ,
            "Engineering Plan" ,
            "Floor Plan" ,
            "Internal",
            "Legend",
            "Load Report",
            "Panel Schedule",
            "Pressure Loss Report",
            "Rendering",
            "Report",
            "Schedule",
            "Section",
            "3D",
            "Walkthrough",
            "Undefined"
        })]
        public static Dictionary<string, object> CollectViews()
        {

            List<dynamic> area = new List<dynamic>();
            List<dynamic> ceiling = new List<dynamic>();
            List<dynamic> column = new List<dynamic>();
            List<dynamic> cost = new List<dynamic>();
            List<dynamic> detail = new List<dynamic>();
            List<dynamic> draft = new List<dynamic>();
            List<dynamic> drawing = new List<dynamic>();
            List<dynamic> elevation = new List<dynamic>();
            List<dynamic> engineering = new List<dynamic>();
            List<dynamic> floorplan = new List<dynamic>();
            List<dynamic> intern = new List<dynamic>();
            List<dynamic> legend = new List<dynamic>();
            List<dynamic> loads = new List<dynamic>();
            List<dynamic> panels = new List<dynamic>();
            List<dynamic> pressure = new List<dynamic>();
            List<dynamic> rendering = new List<dynamic>();
            List<dynamic> report = new List<dynamic>();
            List<dynamic> schedule = new List<dynamic>();
            List<dynamic> section = new List<dynamic>();
            List<dynamic> threeD = new List<dynamic>();
            List<dynamic> walkhrough = new List<dynamic>();
            List<dynamic> undefined = new List<dynamic>();
            List<dynamic> errors = new List<dynamic>();
            var document = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector viewCollector = new FilteredElementCollector(document);
            viewCollector.OfClass(typeof(View));

            foreach (Autodesk.Revit.DB.Element viewElement in viewCollector)
            {
                try
                {
                    View view = viewElement as View;
                    // Get the view type of the given view, and format the prompt string
                    if (view.IsTemplate)
                    {
                        continue;
                    }
                    switch (view.ViewType)
                    {
                        case ViewType.AreaPlan:
                            area.Add(view.ToDSType(true));
                            break;
                        case ViewType.CeilingPlan:
                            ceiling.Add(view.ToDSType(true));
                            break;
                        case ViewType.ColumnSchedule:
                            column.Add(view.ToDSType(true));
                            break;
                        case ViewType.CostReport:
                            cost.Add(view.ToDSType(true));
                            break;
                        case ViewType.Detail:
                            detail.Add(view.ToDSType(true));
                            break;
                        case ViewType.DraftingView:
                            draft.Add(view.ToDSType(true));
                            break;
                        case ViewType.DrawingSheet:
                            drawing.Add(view.ToDSType(true));
                            break;
                        case ViewType.Elevation:
                            elevation.Add(view.ToDSType(true));
                            break;
                        case ViewType.EngineeringPlan:
                            engineering.Add(view.ToDSType(true));
                            break;
                        case ViewType.FloorPlan:
                            floorplan.Add(view.ToDSType(true));
                            break;
                        case ViewType.Internal:
                            intern.Add(view.ToDSType(true));
                            break;
                        case ViewType.Legend:
                            legend.Add(view.ToDSType(true));
                            break;
                        case ViewType.LoadsReport:
                            loads.Add(view.ToDSType(true));
                            break;
                        case ViewType.PanelSchedule:
                            panels.Add(view.ToDSType(true));
                            break;
                        case ViewType.PresureLossReport:
                            pressure.Add(view.ToDSType(true));
                            break;
                        case ViewType.Rendering:
                            rendering.Add(view.ToDSType(true));
                            break;
                        case ViewType.Report:
                            report.Add(view.ToDSType(true));
                            break;
                        case ViewType.Schedule:
                            schedule.Add(view.ToDSType(true));
                            break;
                        case ViewType.Section:
                            section.Add(view.ToDSType(true));
                            break;
                        case ViewType.ThreeD:
                            threeD.Add(view.ToDSType(true));
                            break;
                        case ViewType.Undefined:
                            undefined.Add(view.ToDSType(true));
                            break;
                        case ViewType.Walkthrough:
                            walkhrough.Add(view.ToDSType(true));
                            break;
                        default:
                            break;
                    }
                }
                catch(NullReferenceException ex)
                {
                    errors.Add(ex.ToString());
                }

            }
            return new Dictionary<string, object>
            {
                {"Area View", area },
                {"Reflected Ceiling Plan", ceiling },
                {"Column Schedule", column},
                {"Cost Report", cost},
                {"Detail View", detail },
                {"Drafting View", draft },
                {"Drawing Sheet", drawing },
                {"Elevations", elevation },
                {"Engineering Plan", engineering },
                {"Floor Plan", floorplan },
                {"Internal", intern },
                {"Legend", legend },
                {"Load Report", loads },
                {"Panel Schedule", panels },
                {"Pressure Loss Report", pressure },
                {"Rendering", rendering },
                {"Report", report },
                {"Schedule", schedule },
                {"Section", section },
                {"3D", threeD },
                {"Walkthrough", walkhrough },
                {"Undefined", undefined },
                {"Errors", errors }
            };
        }

        /// <summary>
        /// Find all of the View Templates in this Project.
        /// </summary>
        /// <returns>Collect the View Templates in the Project.</returns>
        /// <search>view, template, collector</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "View Templates"})]
        public static Dictionary<string, object> CollectViewTemplates()
        {
            List<dynamic> template = new List<dynamic>();

            var document = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector viewCollector = new FilteredElementCollector(document);
            viewCollector.OfClass(typeof(View));
            foreach (Autodesk.Revit.DB.Element viewElement in viewCollector)
            {
                View view = (View)viewElement;
                // Get the view type of the given view, and format the prompt string
                if (!view.IsTemplate)
                {
                    template.Add(view);
                }
            }
            return new Dictionary<string, object>
            {
                {"View Templates", template }
            };
        }

        /// <summary>
        /// Is a View Template applied to this view?
        /// </summary>
        /// <param name="Views">The Views to find the View Templates.</param>
        /// <returns>View Template applied?</returns>
        /// <search>view, template</search>
        [NodeCategory("Query")]
        public static List<dynamic> ViewTemplateApplied(Revit.Elements.Element[] Views)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            List<dynamic> template = new List<dynamic>();
            foreach (var viewElement in Views)
            {
                ElementId searcher = Spring.Elements.UnwrapElement(viewElement);
                View finalView = doc.GetElement(searcher) as View;
                if(finalView.ViewTemplateId != null)
                {
                    template.Add(true);
                }
                else
                {
                    template.Add(false);
                }
            }
            return template;
        }
    }
}
