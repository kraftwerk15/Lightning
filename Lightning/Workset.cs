using Autodesk.Revit.DB;
using RevitServices.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spring
{
    public static class Workset
    {
        public static bool SetWorksetName(int WorksetId, string Name)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if(doc.IsWorkshared)
            {
                try
                {
                    WorksetId id = null;
                    FilteredWorksetCollector worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset);
                    foreach (Autodesk.Revit.DB.Workset w in worksets)
                    {
                        if(w.Id.IntegerValue == WorksetId)
                        {
                            id = w.Id;
                            break;
                        }
                    }
                    if(id != null)
                    {
                        using (Autodesk.Revit.DB.Transaction t = new Autodesk.Revit.DB.Transaction(doc, "Dynamo_SetProjectNumber"))
                        {
                            t.Start();
                            WorksetTable.RenameWorkset(doc, id, Name);
                            t.Commit();
                            return true;
                        }
                    }
                    else
                    {
                        throw new Exception("Workset is not available in this document.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            else
            {
                throw new Exception("The current file is not workshared.");
            }
        }
    }
}
