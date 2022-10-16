using Autodesk.Revit.ApplicationServices;
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

namespace Revit2022API334
{
    [Transaction(TransactionMode.Manual)]

    public class Main : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            string nameParameter = "Description";

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document document = uidoc.Document;

            var categorySet = new CategorySet();
            categorySet.Insert(Category.GetCategory(document, BuiltInCategory.OST_PipeCurves));

            using (Transaction ts = new Transaction(document, "Add parameter"))
            {
                ts.Start();
                CreateSharedParameter(uiapp.Application, document, nameParameter, categorySet, BuiltInParameterGroup.PG_GEOMETRY, true);
                ts.Commit();
            }

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
                    Parameter innerDiametr = selectedElement.get_Parameter(BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM);
                    Parameter outerDiametr = selectedElement.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);

                    string descriptionValue =
                        "Труба " +
                        Convert.ToString(UnitUtils.ConvertFromInternalUnits(outerDiametr.AsDouble(), UnitTypeId.Millimeters)) +
                        " / " +
                        Convert.ToString(UnitUtils.ConvertFromInternalUnits(innerDiametr.AsDouble(), UnitTypeId.Millimeters));

                    using (Transaction ts = new Transaction(document, "Set parametrs"))
                    {
                        ts.Start();
                        Parameter description = elemetPipe.LookupParameter(nameParameter);
                        description.Set(descriptionValue);
                        ts.Commit();
                    }
                }
            }

            TaskDialog.Show("Операция выполнена", "Добавлен новый параметр Description");

            return Result.Succeeded;

        }

        private void CreateSharedParameter(
            Application application,
            Document document,
            string parametrName,
            CategorySet categorySet,
            BuiltInParameterGroup builtInParameterGroup,
            bool isInstance)
        {
            DefinitionFile definitionFile = application.OpenSharedParameterFile();

            if (definitionFile == null)
            {
                TaskDialog.Show("Ошибка", "Не найден файл общих параметров");
                return;
            }

            Definition definition = definitionFile.Groups
                .SelectMany(group => group.Definitions)
                .FirstOrDefault(def => def.Name.Equals(parametrName));

            if (definition == null)
            {
                TaskDialog.Show("Ошибка", "Не найден указанный параметр");
                return;
            }

            Binding binding = application.Create.NewTypeBinding(categorySet);

            if (isInstance)
            {
                binding = application.Create.NewInstanceBinding(categorySet);
            }

            BindingMap map = document.ParameterBindings;
            map.Insert(definition, binding, builtInParameterGroup);
        }
    }
}
