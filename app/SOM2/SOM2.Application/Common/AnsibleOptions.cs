using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Application.Common
{
    public class AnsibleOptions
    {
        public string Mode { get; set; } = "Wsl";
        public string BasePath { get; set; } = ""; 
    }
}
