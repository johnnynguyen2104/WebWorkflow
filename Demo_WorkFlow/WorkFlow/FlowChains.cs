using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo_WorkFlow.WorkFlow
{
    public class FlowChains<Data>
        where Data : class
    {
        internal delegate Task<object> ActionResultDelegate<TInput>(TInput inputs);

        internal List<Func<ActionResultDelegate<Data>, ActionResultDelegate<Data>>> _inputActionResultFuncs;
        internal ActionResultDelegate<Data> mainDelegate;


        public FlowChains()
        {
            _inputActionResultFuncs = new List<Func<ActionResultDelegate<Data>, ActionResultDelegate<Data>>>();
        }

        /// <summary>
        /// Adds the middleware to funcs list. 
        /// </summary>
        /// <param name="middleware"> The middleware to be invoked. </param>
        /// <remarks>
        /// 
        /// </remarks>
        internal void Use(Func<ActionResultDelegate<Data>, ActionResultDelegate<Data>> middleware)
        {
            // Keeps a reference to the currently invoked delegate instance.
            _inputActionResultFuncs.Add(middleware);
        }
    }
}
