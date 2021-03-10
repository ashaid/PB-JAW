using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Included;
using Python.Runtime;
using System.IO;

namespace PB_JAW.Models
{
    public class MapUtilities
    {
        private IWebHostEnvironment host;
        public MapUtilities(IWebHostEnvironment host)
        {
            this.host = host;
        }

        static async Task<IntPtr> StartPython()
        {

            Installer.InstallPath = Path.GetFullPath(".");
            Installer.LogMessage += Console.WriteLine;
            await Installer.SetupPython();

            
            // initial setup
            if (PythonEngine.IsInitialized == false)
            {
                Installer.TryInstallPip();
                Installer.PipInstallModule("pillow");
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
            IntPtr gs = PythonEngine.AcquireLock();
            try
            {
                dynamic os = PythonEngine.ImportModule("os");
                Console.WriteLine("### Current working directory:\n\t" + os.getcwd());
                Console.WriteLine("### PythonPath:\n\t" + PythonEngine.PythonPath);

                    // pillow version
                dynamic pillow = Py.Import("PIL");
                Console.WriteLine("Pillow version: " + pillow.__version__);
                
            }

            catch (PythonException pe)
            {
                Console.WriteLine(pe);
            }
            
             return gs;
            
            
        }
        // code to find image corresponding to user input
        void CreateImage(string templatePath, string dictionary, string roomNumber, string name, IntPtr gs)
        {

                // string cleaning
                string path = "/python/";
                path = host.ContentRootFileProvider.GetFileInfo(path).PhysicalPath;
                path = path.Replace('\\', '/');

                // set PYTHON global variable to look for script
                int returnValue = PythonEngine.RunSimpleString($"import sys;sys.path.insert(1, '{path}');");
                if (returnValue != 0)
                {
                    //throw exception or other failure handling
                    Console.WriteLine("!!!!!! incorrect PATH setting !!!!!!!!!!!!");
                }

                // import main.py to run
                dynamic mod = Py.Import("main");
                // call main with [map].jpeg, [dict], [room number] [name of new image]
                mod.main(host.ContentRootFileProvider.GetFileInfo(templatePath).PhysicalPath, dictionary, roomNumber, name);
                PythonEngine.ReleaseLock(gs);
            
            

        }

        public string FindBuildingDictionary(string BuildingNumber)
        {
            string s = "";

            switch (BuildingNumber)
            {
                case "Business Education Complex":
                    s = "bec";
                    break;
                case "Patrick F. Taylor Hall":
                    s = "pft";
                    break;
                case "Lockett Hall":
                    s = "loc";
                    break;
            }
            return s;
        }
        public string FindBuilding(string BuildingNumber)
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
        public async Task<string> CreateMap(List<MapModel> Maps)
        {
            IntPtr gs = await StartPython();

            // create initial map
            string buildingName = FindBuilding(Maps[0].Building);
            string dictionary = FindBuildingDictionary(buildingName);
            string roomNumber = Maps[0].RoomNumber.ToString();

            string templatePath = FindMapTemplate(buildingName);

            // name of new file
            string name = buildingName + "_" + roomNumber + ".jpeg";

            // path of template
            string path = host.ContentRootFileProvider.GetFileInfo(templatePath).PhysicalPath;


            CreateImage(templatePath, dictionary, roomNumber, name, gs);
            
            return name;
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
