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
            if(ModelState.IsValid)
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

                }
                return View("Result", templateModel);
            }
            else
            {
                ModelState.AddModelError("", "Please correct the highlighted error below.");
                return View("MapBuilder", templateModel);
            }
        }

        /*
    {
        private int PFT2BEC; //static time from PFT to BEC
        private int PFT2Loc; //static time from PFT to Lockett
        private int BEC2Loc; //static time from BEC to Lockett

        private destinationRoom(string roomNum, string building, string time)
        {
            this.roomNum = roomNum;
            this.building = building;
            this.time2dxit = time;
        }

        private sourceRoom(string roomNum, string building, string time)
        {
            this.roomNum = roomNum;
            this.building = building;
            this.time2exit = time;
        }


        //queries

        public string destinationInfo() {
            //receive information from user
            //query information for destination
            if (//destination building is BEC)
        {
                //query room number information from BEC table
            }
            else if (//destination building is PFT)
        {
                //query room number information from PFT table
            }
            else if (//destination building is Lockett)
        {
                //query room number information from Loc table
            }
            else (){
                //throw wrong building exception
            }
        }

        public string sourceInfo() {
    //receive information from user
    //query information for source
    if (//source building is BEC)
                {
        //query room number information from BEC table
    }
    else if (//source building is PFT)
                {
        //query room number information from PFT table
    }
    else if (//source building is Lockett)
                {
        //query room number information from Loc table
    }
    else (){
        //throw wrong building exception
    }
}

       */
        public string timeCalulator(destinationRoom a, sourceRoom b){
        
            int totalTime;

           switch(a.building){
            
                case a.building = "BEC":
                    if(b.building == "PFT") //from BEC to PFT
                    {
                        totalTime = a.time2exit + b.time2exit + PFT2BEC;
                    }
                    else if (b.building == "Loc") //from BEC to Lockett
                    {
                        totalTime = a.time2exit + b.time2exit + BEC2Loc;
                    }
                    else if (b.building == "BEC") //from BEC to BEC
                    {
                        totalTime = 0; //within the same building. Throw catch for output saying "in the same building. ETA < 3 minutes or something
                    }
                    else{//throw error
                    }
                    break;

                case a.building = "PFT":
                    if(b.building == "PFT") //from BEC to PFT
                    {
                        totalTime = 0 //within the same building. Throw catch for output saying "in the same building. ETA < 3 minutes or something
                    }
                    else if (b.building == "Loc") //from BEC to Lockett
                    {
                        totalTime = a.time2exit + b.time2exit + PFT2Loc;
                    }
                    else if (b.building == "BEC") //from BEC to BEC
                    {
                        totalTime = a.time2exit + b.time2exit + PFT2BEC; 
                    }
                    else{//throw error
                    }
                    break;

                case a.building = "Loc":
                    if(b.building == "PFT") //from BEC to PFT
                    {
                        totalTime = a.time2exit + b.time2exit + PFT2Loc;
                    }
                    else if (b.building == "Loc") //from BEC to Lockett
                    {
                        totalTime = 0; //within same building. throw catch
                    }
                    else if (b.building == "BEC") //from BEC to BEC
                    {
                        totalTime = a.time2exit + b.time2exit + BEC2Loc;
                    }
                    else{//throw error
                    }
                    break;

                case b.building = null: //source room null means user is approaching from off of campus
                    if(a.building == "PFT"){
                    // printf("You are arriving from off of campus to Patrick F. Taylor Hall. Unexpected ETA. \n");
                    totalTime = 0;
                    }
                    else if(a.building == "Loc"){
                    // printf("You are arriving from off of campus to Lockett Hall. Unexpected ETA. \n");
                    totalTime = 0;
                    }
                    else if(a.building == "BEC"){
                    // printf("You are arriving from off of campus to the Business Education Complex. Unexpected ETA. \n");
                    totalTime = 0;
                    }
                    else{
                    //thow error
                    }

                default: //throw error
                    totalTime = 0;
                    break;

                    return totalTime;
            }


        }


    }
}

    }
}
