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

namespace Revit2022API333
{
    [Transaction(TransactionMode.Manual)]

    public class Main : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string nameParameter = "Length10";

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


            double lengthValue = 0;
            Parameter lenghtParametr = null;

            foreach (var selectedElementRef in selectedElementsReference)
            {
                Element element = document.GetElement(selectedElementRef);
                if (element is Pipe)
                {
                    Pipe elemetPipe = (Pipe)element;
                    pipeList.Add(elemetPipe);

                    var selectedElement = document.GetElement(selectedElementRef);
                    lenghtParametr = selectedElement.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                    lengthValue = UnitUtils.ConvertFromInternalUnits(lenghtParametr.AsDouble(), UnitTypeId.Feet);
                    double lengthValue10 = lengthValue * 1.1;

                    using (Transaction ts = new Transaction(document, "Set parametrs"))
                    {
                        ts.Start();
                        Parameter length10Parameter = elemetPipe.LookupParameter(nameParameter);
                        length10Parameter.Set(lengthValue10);
                        ts.Commit();
                    }
                }
            }

            TaskDialog.Show("Операция выполнена", "Добавлен новый параметр  Длина + 10%");

            return Result.Succeeded;

        }

        private void CreateSharedParameter(Application application,
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
