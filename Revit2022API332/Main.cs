using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit2022API332
{
    [Transaction(TransactionMode.Manual)]

    public class Main : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document document = uidoc.Document;

            double lengthValue = 0;
            Parameter lenghtParametr = null;

            IList<Reference> selectedElementsReference = uidoc.Selection.PickObjects(ObjectType.Element, "Выберите трубопровод");
            var pipeList = new List<Pipe>();

            foreach (var selectedElementRef in selectedElementsReference)
            {
                Element element = document.GetElement(selectedElementRef);
                if (element is Pipe)
                {
                    Pipe elemetPipe = (Pipe)element;
                    pipeList.Add(elemetPipe);

                    var selectedElement = document.GetElement(selectedElementRef);
                    lenghtParametr = selectedElement.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                    lengthValue += UnitUtils.ConvertFromInternalUnits(lenghtParametr.AsDouble(), UnitTypeId.Meters);
                }
            }

            TaskDialog.Show("Volume selected PIPES: ", lengthValue.ToString());
            return Result.Succeeded;
        }
    }
}
