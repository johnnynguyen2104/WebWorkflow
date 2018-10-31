using Demo_WorkFlow.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Demo_WorkFlow.WorkFlow
{
    public class WorkFlowBuilder<DataStorage> where DataStorage : class
    {
       

        public string FlowId { get { return flowId; } }

        private string StandByFlowUrl { get; set; }

        public int CurrentFlowIndex { get { return currentFlowIndex; } }

        private FlowBuilder flows;

        private int currentFlowIndex;
        private string flowId;

        private readonly IDistributedCache _distributedCache;
        private DataStorage ExtraData;

        public WorkFlowBuilder(FlowBuilder _flows, IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            flows = _flows;

            InitializeData();
            Build();
        }

        public WorkFlowBuilder(string _flowId, FlowBuilder _flows, IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            flows = _flows;
            flowId = _flowId;

            var dataFromStorage = Common.ByteArrayToObject<Dictionary<string, object>>(_distributedCache.Get(flowId));

            StandByFlowUrl = dataFromStorage["StandByFlowUrl"]?.ToString();
            currentFlowIndex = (int)dataFromStorage["CurrentFlowIndex"];
            ExtraData = dataFromStorage["ExtraData"] as DataStorage;
        }

        private void InitializeData()
        {
            flowId = Guid.NewGuid().ToString();
            currentFlowIndex = 0;
        }

        private void Build()
        {

            Dictionary<string, object> obj = new Dictionary<string, object>(3);
            obj.Add("StandByFlowUrl", StandByFlowUrl);
            obj.Add("CurrentFlowIndex", currentFlowIndex);
            obj.Add("ExtraData", ExtraData);

            var data = Common.ObjectToByteArray(obj);

            _distributedCache.Set(FlowId, data, new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(3)
            });
        }


        public async Task<IActionResult> Run(object firstInput)
        {
            object returnData = firstInput;
            try
            {
                for (; currentFlowIndex < flows.Flows.Count; currentFlowIndex++)
                {
                    returnData = flows.Flows[currentFlowIndex](returnData, flowId);

                    if (returnData as IActionResult != null)
                    {
                        //avoiding running current flow after returnning ActionResult.
                        currentFlowIndex++;
                        Build();
                        return returnData as IActionResult;
                    }
                }
            }
            catch (Exception ex)
            {
                flows.ErrorHandler(ex);
                //clear flow data if got error
                _distributedCache.Remove(flowId);
            }
            finally
            {
                if (flows.CompleteHandler != null && currentFlowIndex == flows.Flows.Count)
                {
                    flows.CompleteHandler(flowId, ExtraData);
                    //clear flow data after finish 
                    _distributedCache.Remove(flowId);
                }
            }

            throw new Exception("Nothing to do next.");
        }
    }
}
