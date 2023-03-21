using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisFileManager.ElevatedOperations
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                return;
            }

            Guid.TryParse(args[0], out Guid operationId);
            Guid.TryParse(args[1], out Guid applicationInstanceId);

            var manager = new OperationsManager(operationId, applicationInstanceId);
            manager.Start();
        }
    }
}
