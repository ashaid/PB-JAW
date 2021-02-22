using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PB_JAW.Models
{
    public class MapUtilities
    {
        private IWebHostEnvironment host;
        public MapUtilities(IWebHostEnvironment host)
        {
            this.host = host;
        }
    
        // code to find image corresponding to user input
        void FindImage()
        {

        }
        string FindBuilding(string BuildingNumber)
        {
            string s = "";

            switch(BuildingNumber)
            {
                case "0":
                    s = "Business Education Complex";
                    break;
                case "1":
                    s = "Patrick F. Taylor Hall";
                    break;
                case "2":
                    s = "Lockett Hall";
                    break;
            }

            return s;
            
        }

        public string FindMapTemplate(string name) 
        {
            string templateName = "";
            if(name == "Business Education Complex")
            {
                templateName = "/wwwroot/template/BEC.jpeg";
            }

            else if(name == "Patrick F. Taylor Hall")
            {
                templateName = "/wwwroot/template/PFT-1.jpeg";
            }
            else if (name == "Lockett Hall")
            {
                templateName = "/wwwroot/template/LOCKETT.jpeg";
            }

            return templateName;
        }

        // code to createmap using python package
        public string CreateMap(List<TemplateModelMap> Maps)
        {
            // create initial map
            string buildingName = FindBuilding(Maps[0].Building);

            string name = FindBuilding(Maps[0].Building) + "_" + Maps[0].RoomNumber.ToString() + ".jpeg";
            string path = host.ContentRootFileProvider.GetFileInfo(FindMapTemplate(buildingName)).PhysicalPath;



            return "name";
        }

        // calculate travel time between start and ending positions
        void TravelTime()
        {

        }


        // calculate text directions for the user
        void Directions()
        {

        }
    }
}
