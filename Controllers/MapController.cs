using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PB_JAW.Models;
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
        public ViewResult MapBuilder(int? numMaps)
        {
            TemplateModelMap templateModel = new TemplateModelMap();

            while(numMaps > 0)
            {
                TemplateModelMap start = new TemplateModelMap();
                templateModel.AddMap(start);
                numMaps--;

                TemplateModelMap destination = new TemplateModelMap();
                templateModel.AddMap(destination);
                numMaps--;
            }

            return View("MapBuilder", templateModel);
        }

        // create/save map and return new view for the user to see
        //TAKES PLACE OF CreateModel() method on Map Controller Component
        [HttpPost]
        public ViewResult SaveMap(TemplateModelMap templateModel)
        {
            if(ModelState.IsValid)
            {
                // code to generate template map
                MapUtilities util = new MapUtilities(host);
                try
                {
                    string fileName = util.CreateMap(templateModel.Maps);
                    TempData["GenerateFile"] = fileName;
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
