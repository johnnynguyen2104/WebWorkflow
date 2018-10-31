using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Demo_WorkFlow.WorkFlow
{
    public class FlowBuilder
    {

        public delegate object ActionResultDelegate(object input, string flowId);

        public delegate void ExceptionDelegate(Exception input);

        public delegate void CompleteDelegate(string flowId, object extraData);

        public List<ActionResultDelegate> Flows { get { return flows; } }
        public ExceptionDelegate ErrorHandler { get { return errorHandler; } }
        public CompleteDelegate CompleteHandler { get { return completeHandler; } }

        private List<ActionResultDelegate> flows;
        private ExceptionDelegate errorHandler;
        private CompleteDelegate completeHandler;

        public FlowBuilder()
        {
            flows = new List<ActionResultDelegate>();
            errorHandler = (e) => { Console.WriteLine("Holy Cow !"); };
            completeHandler = (flowId, data) => { Console.WriteLine("Complete !"); };
        }


        /// <summary>
        /// Adding a flow to workflow. 
        /// </summary>
        /// <param name="middleware"> The middleware to be invoked. </param>
        /// <remarks>
        /// 
        /// </remarks>
        public FlowBuilder AddFlow(ActionResultDelegate flow)
        {
            flows.Add(flow);

            return this;
        }

        public FlowBuilder AddActionFlow(ActionResultDelegate flow)
        {
            //flows.Add(((a) => { Build(); return a; }));
            flows.Add(flow);


            return this;
        }

        public FlowBuilder OnComplete(CompleteDelegate complete)
        {
            completeHandler = complete;

            return this;
        }

        public FlowBuilder OnError(ExceptionDelegate error)
        {
            errorHandler = error;

            return this;
        }
    }
}
