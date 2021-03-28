using System.ComponentModel.DataAnnotations;

namespace PB_JAW.Models
{
    public class MapModel
    {
        [Required(ErrorMessage = "Please select a building.")]
        public string Building { get; set; }

        // roomnumber attribute
        [Required(ErrorMessage = "Please select a room number.")]
        public int RoomNumber { get; set; }
    }
}
