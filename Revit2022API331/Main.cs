using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

namespace Revit2022API331
{
    [Transaction(TransactionMode.Manual)]

    public class Main : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document document = uidoc.Document;

            double volumeValue = 0;
            Parameter volumeParametr = null;

            IList<Reference> selectedElementsReference = uidoc.Selection.PickObjects(ObjectType.Face, "Выберите стены по граням");
            var wallList = new List<Wall>();

            foreach (var selectedElementRef in selectedElementsReference)
            {

                Element element = document.GetElement(selectedElementRef);
                if (element is Wall)
                {
                    Wall elemetWall = (Wall)element;
                    wallList.Add(elemetWall);

                    var selectedElement = document.GetElement(selectedElementRef);
                    volumeParametr = selectedElement.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                    volumeValue += UnitUtils.ConvertFromInternalUnits(volumeParametr.AsDouble(), UnitTypeId.CubicMeters);
                }
            }
            TaskDialog.Show("Volume selected walls: ", volumeValue.ToString());
            return Result.Succeeded;
        }
    }
}
