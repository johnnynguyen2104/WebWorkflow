using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo_WorkFlow.Data
{
    public class WorkflowData
    {
        public string CurrentFlowUrl { get; }

        public string FlowId { get; }

        public dynamic Inputs { get; set; }
    }
}
