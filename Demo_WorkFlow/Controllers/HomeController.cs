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
        public HomeController(IDistributedCache _distributedCache)
        {
            distributedCache = _distributedCache;
        }
        public async Task<IActionResult> Index(string flowId, IFormCollection form)
        {
            WorkFlowBuilder<Student> workFlowBuilder = new WorkFlowBuilder<Student>(flowId, distributedCache);

            workFlowBuilder
                .AddFlow((a) => FirstMethod((int)a))
                .AddFlow((a) => SecondMethod((int)a))
                .Build();


            return await workFlowBuilder.Run(1);
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
