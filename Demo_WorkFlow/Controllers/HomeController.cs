using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Demo_WorkFlow.Models;
using Demo_WorkFlow.WorkFlow;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Demo_WorkFlow.Controllers
{
    public class Student
    {
        public string id { get; set; }
    }
    public class HomeController : Controller
    {
        IDistributedCache distributedCache;
        FlowBuilder flowBuilder;
        public HomeController(IDistributedCache _distributedCache)
        {
            distributedCache = _distributedCache;

            flowBuilder = new FlowBuilder();

            flowBuilder
               .AddFlow((a, flowId) => FirstMethod((int)a))
               .AddFlow((a, flowId) => SecondMethod((int)a))
               .AddActionFlow((a, flowId) => RedirectFunction(a.ToString(), flowId))
               .AddFlow((a, flowId) => HandleResponseFromAPI(a as FormCollection));


        }

        public async Task<IActionResult> Index()
        {
            WorkFlowBuilder<Student> workFlowBuilder = new WorkFlowBuilder<Student>(flowBuilder, distributedCache);


            return await workFlowBuilder.Run(1);
        }

        public async Task<IActionResult> AfterRedirect(string flowId, IFormCollection form)
        {
            WorkFlowBuilder<Student> workFlowBuilder = new WorkFlowBuilder<Student>(flowBuilder, distributedCache);
            if (!string.IsNullOrEmpty(flowId))
            {
                workFlowBuilder = new WorkFlowBuilder<Student>(flowId, flowBuilder, distributedCache);
            }

            return await workFlowBuilder.Run(form);
        }

        [Route("/Home/ReceiveRedirect/{input}/{flowId}")]
        public IActionResult ReceiveRedirect(string input, string flowId)
        {
            input += "Hello";

            return Redirect($"/Home/AfterRedirect?flowId={flowId}");
        }

        IActionResult RedirectFunction(string input, string flowId)
        {
            input += "World";

            return Redirect($"/Home/ReceiveRedirect/{input}/{flowId}");
        }

        IActionResult HandleResponseFromAPI(IFormCollection form)
        {
            return Ok();
        }

        int FirstMethod(int a)
        {
            return a++;
        }

        string SecondMethod(int data)
        {
            return data.ToString();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
