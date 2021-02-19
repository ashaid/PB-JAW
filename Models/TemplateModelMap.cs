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
        [Required(ErrorMessage = "Please select a building.")]
        public string Building { get; set; }

        // roomnumber attribute
        [Required(ErrorMessage = "Please select a room number.")]
        public string RoomNumber { get; set; }

        public TemplateModelMap()
        {
            Maps = new List<TemplateModelMap>();
        }

        public List<TemplateModelMap> Maps { get; set; }

        public void AddMap(TemplateModelMap m)
        {
            Maps.Add(m);
        }
    }
}
