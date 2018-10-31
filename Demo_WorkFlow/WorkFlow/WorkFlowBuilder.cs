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
        public delegate object ActionResultDelegate(object input);

        public delegate void ExceptionDelegate(Exception input);

        public delegate void CompleteDelegate(Exception input);

        private List<ActionResultDelegate> flows;
        private ExceptionDelegate errorHandler;
        private ActionResultDelegate completeHandler;

        public string FlowId { get { return flowId; } }

        private string StandByFlowUrl { get; set; }

        public int CurrentFlowIndex { get { return currentFlowIndex; } }

        public DataStorage ExtraData { get; set; }

        private int currentFlowIndex;
        private string flowId;

        private readonly IDistributedCache _distributedCache;

        

        public WorkFlowBuilder(string _flowId, IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            if (string.IsNullOrEmpty(_flowId) || _flowId.Trim().Length == 0)
            {
                InitializeData();
            }
            else
            {
                flowId = _flowId;
                var dataFromStorage = ByteArrayToObject<Dictionary<string, object>>(_distributedCache.Get(flowId));

                StandByFlowUrl = dataFromStorage["StandByFlowUrl"].ToString();
                currentFlowIndex = (int)dataFromStorage["CurrentFlowIndex"];
                ExtraData = dataFromStorage["ExtraData"] as DataStorage;
            }
        }

        private void InitializeData()
        {
            flowId = Guid.NewGuid().ToString();
            flows = new List<ActionResultDelegate>();
            currentFlowIndex = 0;
        }

        byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        OutputType ByteArrayToObject<OutputType>(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            OutputType obj = (OutputType)binForm.Deserialize(memStream);
            return obj;
        }

        public async Task<IActionResult> Run(object firstInput)
        {
            object returnData = firstInput;

            try
            {
                for (; currentFlowIndex < flows.Count; currentFlowIndex++)
                {
                    returnData = flows[currentFlowIndex](returnData);

                    if (returnData as IActionResult != null)
                    {
                        return returnData as IActionResult;
                    }
                }
            }
            catch (Exception ex)
            {
                errorHandler(ex);
            }
            finally
            {
                if (completeHandler != null)
                {
                    completeHandler(this);
                }
            }

            throw new Exception("Nothing to do now.");
        }

        public void Build()
        {
            flows.Reverse();

            Dictionary<string, object> obj = new Dictionary<string, object>(3);
            obj.Add("StandByFlowUrl", StandByFlowUrl);
            obj.Add("CurrentFlowIndex", CurrentFlowIndex);
            obj.Add("ExtraData", ExtraData);

            var data = ObjectToByteArray(obj);

            _distributedCache.Set(FlowId, data);
        }

        /// <summary>
        /// Adds the middleware to funcs list. 
        /// </summary>
        /// <param name="middleware"> The middleware to be invoked. </param>
        /// <remarks>
        /// 
        /// </remarks>
        public WorkFlowBuilder<DataStorage> AddFlow(ActionResultDelegate middleware)
        {
            flows.Add(middleware);

            return this;
        }

        public WorkFlowBuilder<DataStorage> OnComplete(ActionResultDelegate complete)
        {
            completeHandler = complete;

            return this;
        }

        public WorkFlowBuilder<DataStorage> OnError(ExceptionDelegate error)
        {
            errorHandler = error;

            return this;
        }
    }
}
