using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace VisFileManager
{
    public static class Bootstraper
    {
        public static Guid ApplicationInstanceId { get; set; }
        public static CompositionContainer Container { get; private set; }

        public static EyetrackerManager EyetrackerManager
        {
            get
            {
                return Container.GetExportedValue<EyetrackerManager>();
            }
        }

        public static void InitializeContainer(object initPoint)
        {
            var catalog = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
            Container = new CompositionContainer(catalog);
            
            Container.SatisfyImportsOnce(initPoint);
        }

        public static void Dispose()
        {
            Container.Dispose();
        }
    }

}
