using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PB_JAW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace PB_JAW.Controllers
{
    public class MapController : Controller
    {
        private readonly IWebHostEnvironment host;

        public MapController(IWebHostEnvironment host)
        {
            this.host = host;
        }

        /**
         * Using input fields create a new map template
         *
         * method: MapBuilder
         *
         * return type: View
         *
         * @author Anthony Shaidaee
         * @since 2/15/2021
         *
         */
        [HttpGet]
        public ViewResult MapBuilder()
        {
            TemplateModelMap templateModel = new TemplateModelMap();

            // create starting map
            MapModel start = new MapModel();
            templateModel.AddMap(start);
            // create destination map
            MapModel destination = new MapModel();
            templateModel.AddMap(destination);

            return View("MapBuilder", templateModel);
        }

        /**
         * create/save map and return new view for the user to see
         * goes through necessary checks to see if model is valid
         *
         * method: SaveMapAsync
         *
         * return type: Task<ViewResult>
         *
         * parameters:
         * templateModel [TemplateModelMap] model with maps in it
         *
         * @author Anthony Shaidaee
         * @since 2/15/2021
         *
         */
        [HttpPost]
        public async Task<ViewResult> SaveMapAsync(TemplateModelMap templateModel)
        {
            MapUtilities util = new MapUtilities(host);

            // if user has selected "---" dropdown option
            if (templateModel.Maps[0].Building.Contains("-2") ||  templateModel.Maps[1].Building.Contains("-2"))
            {
                ModelState.AddModelError("", "Please select a building");
            }

            if (!util.CheckRoom(templateModel.Maps)) 
            {
                ModelState.AddModelError("", "Invalid Room Number");
            }

            // if room number does not exist in the database 
            //util.CheckRoom(templateModel.Maps);
            // ModelState.AddModelError("", "Invalid Room Number");
            // main driver for map creation
            if (ModelState.IsValid)
            {
                // code to generate template 
                try
                {
                    List<string> fileNames = new List<string>();
                    fileNames = await util.CreateMap(templateModel.Maps);

                    TempData["Map0"] = fileNames[0]; // file location of first map
                    TempData["Map1"] = fileNames[1]; // file location of second map
                    TempData["Directions"] = util.Directions(templateModel.Maps);
                    TempData["Times"] = util.TimeQuery(templateModel.Maps);
                }
                catch
                {
                    Console.WriteLine("something went wrong");
                }
                return View("Result", templateModel);
            }
            else // if model errors out
            {
                ModelState.AddModelError("", "");
                return View("MapBuilder", templateModel);
            }
        }

    }
}
