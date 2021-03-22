using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Included;
using Python.Runtime;
using System.IO;
using System.Data.SQLite;

namespace PB_JAW.Models
{
    public class MapUtilities
    {
        private readonly IWebHostEnvironment host;
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

        public List<string> FindDetails(string BuildingNumber)
        {
            List<string> details = new List<string>();

            switch (BuildingNumber)
            {
                case "0":
                    details.Add("Business Education Complex");
                    details.Add("bec");
                    details.Add("/wwwroot/template/BEC.jpeg");
                    break;
                case "1":
                    details.Add("Patrick F. Taylor Hall");
                    details.Add("pft");
                    details.Add("/wwwroot/template/PFT-1.jpeg");
                    break;
                case "2":
                    details.Add("Lockett Hall");
                    details.Add("loc");
                    details.Add("/wwwroot/template/LOCKETT.jpeg");
                    break;
            }
            return details;
        }

        // code to create map
        public async Task<List<string>> CreateMap(List<MapModel> Maps)
        {
            List<string> names = new List<string>();

            // for each map in list 
            for (int i = 0; i < Maps.Count; i++)
            {
                // no starting destination
                if(Maps[i].Building.Contains("-1"))
                {
                    names.Add("No starting location selected");
                }
                else
                {
                    // python memory store
                    IntPtr gs = await StartPython();

                    // 0 = name, 1 = dictionary, 2 = template path
                    List<string> details = new List<string>();
                    details = FindDetails(Maps[i].Building);

                    // var for map
                    string buildingName = details[0];
                    string dictionary = details[1];
                    string templatePath = details[2];
                    string roomNumber = Maps[i].RoomNumber.ToString();

                    // name of new file
                    string name = buildingName + "_" + roomNumber + ".jpeg";

                    CreateImage(templatePath, dictionary, roomNumber, name, gs);

                    // add new file to names array
                    names.Add(name);

                    details.Clear();
                }
                
            }
            // return new image file location (array)
            return names;
        }
        /*
        
        public string timeQuery(int roomNum, string building){
            string time2exit;
        
            switch(building){
            
                case building = "bec":
                    break;

                case building = "pft":
             
                    break;

                case building = "loc":
            
                    break;

                default:
                    printf("Room Not Found, Cannot Perform Query");
                    break;
            
            
                    return time2exit;

        }

        // calculate travel time between start and ending positions
        void TravelTime()
        {
            // grab current time
            // grab source destination and end destination
            // dynamically calulcate time using sql and priorty queue

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
        */


        // calculate text directions for the user
        string Directions(List<MapModel> Maps)
        {
            string directions;
            SQLiteConnection sqlCon = new SQLiteConnection("DataSource = Locations.db; Version=3; New=True;Compress=True;");
            try
            {
                sqlCon.Open();
                Console.WriteLine("Connection is established");
            }
            catch
            {
                Console.WriteLine("Connection not established");
            }
            // Maps[0] == starting
            // Maps[1] == destination

            // Maps[0].RoomNumber.ToString();
            // List<string> startingDetails = new List<string>();
            // startingDetails = FindDetails(Maps[0].Building);

            string extDirections = exitDirections(srcRoom, srcBuild, destBuild, sqlCon);
            string toDirections = destDirections(destRoom, destBuild, srcBuild, sqlCon);
            string campDirections = campusDirections(srcBuild, destBuild, sqlCon);

            directions = extDirections + campDirections + toDirections;
            //Delete, used for testing
            Console.WriteLine(directions);

            return directions;
        }
        string exitDirections(string srcRoom, string srcBuild, string destBuild, SQLiteConnection con)
        {
            string directions = "";
            using var cmd = new SQLiteCommand(con);
            if (srcBuild == null)
            {
                directions = "why the fuck";
            }
            else if (srcBuild == "Business Education Complex")
            {
                if (destBuild == srcBuild)
                {
                    directions += "You are already in the Business Education Complex.";
                }

                else if (destBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT ExtToPFT FROM BEC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
                else if (destBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT ExtToLoc FROM BEC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
            }

            else if (srcBuild == "Lockett Hall")
            {
                if (destBuild == srcBuild)
                {
                    directions += "You are already in Locket Hall.";
                }

                else if (destBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT ExtToPFT FROM LOC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
                else if (destBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT ExtToBEC FROM LOC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
            }

            else if (srcBuild == "Patrick F. Taylor Hall")
            {
                if (destBuild == srcBuild)
                {
                    directions += "You are already in Patrick F. Taylor Hall.";
                }

                else if (destBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT ExtToBEC FROM PFT WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
                else
                {
                    cmd.CommandText = "SELECT ExtToLoc FROM PFT WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
            }
            return directions;
        }

        string destDirections(string destRoom, string destBuild, string srcBuild, SQLiteConnection con)
        {
            string directions = "\n\n";
            using var cmd = new SQLiteCommand(con);
            if (destBuild == "Business Education Complex")
            {
                if (destBuild == srcBuild)
                {
                    directions += "Please use the map to find your classroom.";
                }

                else if (srcBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT FrmPFTEnt FROM BEC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
                else if (srcBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT FrmLocEnt FROM BEC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
            }

            else if (destBuild == "Lockett Hall")
            {
                if (destBuild == srcBuild)
                {
                    directions += "Please use the map to find your classroom.";
                }

                else if (srcBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT FrmPFTEnt FROM LOC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
                else if (srcBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT FrmBECEnt FROM LOC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
            }

            else if (destBuild == "Patrick F. Taylor Hall")
            {
                if (destBuild == srcBuild)
                {
                    directions += "Please refer to the map to find your classroom.";
                }

                else if (srcBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT FrmBECEnt FROM PFT WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
                else if (srcBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT FrmLocEnt FROM PFT WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    directions += cmd.ExecuteScalar().ToString();
                }
            }
            return directions;
        }

        string campusDirections(string srcBuild, string destBuild, SQLiteConnection con)
        {
            string directions = "\n\n";
            using var cmd = new SQLiteCommand(con);
            if (srcBuild == null)
            {
                directions = "";
            }
            else if (destBuild == "Patrick F. Taylor Hall")
            {
                if (srcBuild == destBuild)
                {
                    directions += "Your classroom is nearby. ";
                }
                else if (srcBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT FrmBEC FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    directions += cmd.ExecuteScalar().ToString();
                }
                else if (srcBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT FrmLoc FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    directions += cmd.ExecuteScalar().ToString();
                }
            }
            else if (destBuild == "Business Education Complex")
            {
                if (srcBuild == destBuild)
                {
                    directions += "Your classroom is nearby. ";
                }
                else if (srcBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT FrmPFT FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    directions += cmd.ExecuteScalar().ToString();
                }
                else if (srcBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT FrmLoc FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    directions += cmd.ExecuteScalar().ToString();
                }
            }
            else if (destBuild == "Lockett Hall")
            {
                if (srcBuild == destBuild)
                {
                    directions += "Your classroom is nearby. ";
                }
                else if (srcBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT FrmBEC FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    directions += cmd.ExecuteScalar().ToString();
                }
                else if (srcBuild == "Patrick F. Taylor")
                {
                    cmd.CommandText = "SELECT FrmPFT FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    directions += cmd.ExecuteScalar().ToString();
                }
            }
            return directions;
        }
    }
}
