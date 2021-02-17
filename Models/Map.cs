using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace PB_JAW.Models
{
    public class Map
    {
        [Required(ErrorMessage = "Please select a building.")]
        public string Building { get; set; }

        [Required(ErrorMessage = "Please select a room number.")]
        public string RoomNumber { get; set; }
    }
}
