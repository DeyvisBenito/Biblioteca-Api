using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BibliotecaAPI.Swagger
{
    public class AgruparControllersConvencions : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            //Obteniendo la version por el namespace
            var nameSpace = controller.ControllerType.Namespace;
            var version = nameSpace!.Split(".").Last().ToLower();
            controller.ApiExplorer.GroupName = version;
        }
    }
}
