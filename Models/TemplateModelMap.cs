using System.Collections.Generic;
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

        /**
        * This method adds a new map type to a list of the same
        * name
        *
        * method: AddMap
        *
        * return type: void
        *
        * parameters:
        * m         [MapModel] MapModel object to be added to list Maps
        *
        * @author Anthony Shaidaee
        * @since 2/4/2021
        *
        */
        public void AddMap(MapModel m)
        {
            Maps.Add(m);
        }

    }
}
