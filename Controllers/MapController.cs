using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PB_JAW.Controllers
{
    public class MapController : Controller
    {
        private IWebHostEnvironment host;
        public MapController(IWebHostEnvironment host)
        {
            this.host = host;
        }

        // using input fields create a new map template
        [HttpGet]
        public ViewResult Map(int? num)
        {
            TemplateModelMap templateModel = new TemplateModelMap();

            return View("Map", templateModel);
        }

        // create/save map and return new view for the user to see
        [HttpPost]
        public ViewResult SaveMap(TemplateModelMap templateModel)
        {
            if(ModelState.IsValid)
            {
                // code to generate template map
                MapUtilities util = new MapUtilities(host);
                try
                {

                }
                catch
                {

                }
                return View("Results");
            }
            else
            {
                ModelState.AddModelError("", "Please correct the highlighted error below.");
                return View("Map", templateModel);
            }
        }
    }
}
