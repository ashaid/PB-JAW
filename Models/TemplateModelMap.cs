using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace PB_JAW.Models
{
    public class TemplateModelMap
    {

        // building attribute

        public TemplateModelMap() 
        {
            Maps = new List<MapModel>();
        }

        public List<MapModel> Maps { get; set; }

        public void AddMap(MapModel m)
        {
            Maps.Add(m);
        }
    }
}
