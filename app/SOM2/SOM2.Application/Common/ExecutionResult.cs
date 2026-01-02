using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Application.Common
{
    public class ExecutionResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = "";
    }
}
