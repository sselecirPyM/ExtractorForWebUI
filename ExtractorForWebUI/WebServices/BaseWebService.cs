using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.WebServices;

public abstract class BaseWebService
{
    public abstract void Process(WebServiceContext context);
}
