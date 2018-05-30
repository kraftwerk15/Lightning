using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;

namespace Spring
{
    public static class FamilyType
    {
        /// <summary>
        /// Given a Family Name and a Family Type as strings, this node will find the Family Type if it exists in the project for your use.
        /// </summary>
        /// <param name="FamilyName">The family name of the family type we are trying to find.</param>
        /// <param name="FamilyType">The family type from the family.</param>
        /// <returns>The FamilyType as the family type.</returns>
        public static Revit.Elements.Element GetFamilyType(string FamilyName, string FamilyType)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            FamilySymbol symbol = Elements.GetSymbol(doc, FamilyName, FamilyType);
            FamilySymbol familySymbol = symbol as FamilySymbol;
            return symbol.ToDSType(false);
        }
    }
}
