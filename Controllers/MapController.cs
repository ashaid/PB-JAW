﻿using Microsoft.AspNetCore.Hosting;
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

            // create first map
            MapModel start = new MapModel();
            templateModel.AddMap(start);
            numMaps--;


            MapModel destination = new MapModel();
            templateModel.AddMap(destination);
            numMaps--;


            return View("MapBuilder", templateModel);
        }

        // create/save map and return new view for the user to see
        //TAKES PLACE OF CreateModel() method on Map Controller Component
        [HttpPost]
        public async Task<ViewResult> SaveMapAsync(TemplateModelMap templateModel)
        {
            if (ModelState.IsValid)
            {
                // code to generate template map
                MapUtilities util = new MapUtilities(host);
                try
                {
                    List<string> fileNames = new List<string>();
                    fileNames = await util.CreateMap(templateModel.Maps);

                    TempData["Map0"] = fileNames[0];
                    TempData["Map1"] = fileNames[1];

                    TempData["BuildingName0"] = util.FindBuilding(templateModel.Maps[0].Building);
                    TempData["BuildingName1"] = util.FindBuilding(templateModel.Maps[1].Building);
                }
                catch
                {
                    Console.WriteLine("something went wrong");
                }
                return View("Result", templateModel);
            }
            else
            {
                ModelState.AddModelError("", "Please correct the highlighted error below.");
                return View("MapBuilder", templateModel);
            }
        }


    }
}
