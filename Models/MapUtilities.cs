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

        // code to createmap using python package
        public string CreateMap(List<TemplateModelMap> Maps)
        {
            return "placeholder";
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
