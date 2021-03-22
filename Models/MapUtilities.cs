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
                //Console.WriteLine("### Current working directory:\n\t" + os.getcwd());
                //Console.WriteLine("### PythonPath:\n\t" + PythonEngine.PythonPath);

                // pillow version
                dynamic pillow = Py.Import("PIL");
                //Console.WriteLine("Pillow version: " + pillow.__version__);
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

        public List<string> FindDetails(string BuildingNumber, List<string> details)
        {

            switch (BuildingNumber)
            {
                case "0":
                    details.Add("Business Education Complex");
                    details.Add("bec");
                    details.Add("/wwwroot/template/BEC.jpeg");
                    details.Add("FrmBEC");
                    details.Add("ExtBEC");
                    details.Add("TimeBEC");
                    break;
                case "1":
                    details.Add("Patrick F. Taylor Hall");
                    details.Add("pft");
                    details.Add("/wwwroot/template/PFT-1.jpeg");
                    details.Add("FrmPFT");
                    details.Add("ExtPFT");
                    details.Add("TimePFT");
                    break;
                case "2":
                    details.Add("Lockett Hall");
                    details.Add("loc");
                    details.Add("/wwwroot/template/LOCKETT.jpeg");
                    details.Add("FrmLoc");
                    details.Add("ExtLoc");
                    details.Add("TimeLoc");
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

                    // 0 = name, 1 = dictionary, 2 = template path, 3 = dest direction, 4 = exit direction, 5 = time traveled
                    List<string> details = new List<string>();
                    details = FindDetails(Maps[i].Building, details);

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
            Directions(Maps);
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
        void Directions(List<MapModel> Maps)
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
            //Maps[0] == starting
            //Maps[1] == destination

            // Maps[0].RoomNumber.ToString();
            List<string> startingDetails = new List<string>();
            FindDetails(Maps[0].Building, startingDetails);
            List<string> endingDetails = new List<string>();
            FindDetails(Maps[1].Building, endingDetails);
            // startingDetails = FindDetails(Maps[0].Building);
            // 0 = name, 1 = dictionary, 2 = template path, 3 = dest direction, 4 = exit direction, 5 = time traveled
            string srcRoom = Maps[0].RoomNumber.ToString(); 
            string destRoom = Maps[1].RoomNumber.ToString(); 
            string srcBuild = startingDetails[1].ToUpper().Replace("\n", String.Empty);
            string destBuild = endingDetails[1].ToUpper().Replace("\n", String.Empty);

            string extDirections =  exitDirections(srcRoom, srcBuild, endingDetails[4], sqlCon);
            string toDirections = destDirections(destRoom, destBuild, startingDetails[3], sqlCon);
            string campDirections = campusDirections(startingDetails[3], endingDetails[0], sqlCon);

            directions = extDirections + campDirections + toDirections;
            //Delete, used for testing
            Console.WriteLine(directions);

            sqlCon.Close();
        }
        string exitDirections(string srcRoom, string srcBuild, string destBuild, SQLiteConnection con)
        {

            using var cmd = new SQLiteCommand(con);
            string directions = "";

            if (srcBuild == "BEC")
            {
                cmd.CommandText = "SELECT ExtPFT FROM BEC WHERE ROOM = @roomNum";
                //sqlCall(destBuild, srcRoom, directions, cmd);
                cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                cmd.Prepare();
                directions = cmd.ExecuteScalar().ToString();
            }
            else if (srcBuild == "PFT")
            {
                cmd.CommandText = "SELECT @columnid FROM PFT WHERE ROOM = @roomNum";
                //sqlCall(destBuild, srcRoom, directions, cmd);
                cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                cmd.Prepare();
                directions = cmd.ExecuteScalar().ToString();
            }
            else if (srcBuild == "LOC") 
            {
                cmd.CommandText = "SELECT @columnid FROM LOC WHERE ROOM = @roomNum";
                //sqlCall(destBuild, srcRoom, directions, cmd);
                cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                cmd.Prepare();
                directions = cmd.ExecuteScalar().ToString();
            }

            return directions;
        }

        string destDirections(string destRoom, string destBuild, string srcBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            string directions = "";
            if (destBuild == "BEC")
            {
                cmd.CommandText = "SELECT @columnid FROM BEC WHERE ROOM = @roomNum";
                //sqlCall(srcBuild, destRoom, directions, cmd);
                cmd.Parameters.AddWithValue("@roomNum", destRoom);
                cmd.Prepare();
                directions = cmd.ExecuteScalar().ToString();
            }
            else if (destBuild == "PFT")
            {
                cmd.CommandText = "SELECT FrmBEC FROM PFT WHERE ROOM = @roomNum";
                //sqlCall(srcBuild, destRoom, directions, cmd);
                cmd.Parameters.AddWithValue("@roomNum", destRoom);
                cmd.Prepare();
                directions = cmd.ExecuteScalar().ToString();
            }
            else if (destBuild == "LOC")
            {
                cmd.CommandText = "SELECT @columnid FROM LOC WHERE ROOM = @roomNum";
                //sqlCall(srcBuild, destRoom, directions, cmd);
                cmd.Parameters.AddWithValue("@roomNum", destRoom);
                cmd.Prepare();
                directions = cmd.ExecuteScalar().ToString();
            }
            return directions;
        }

        string campusDirections(string srcBuild, string destBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT FrmBEC FROM CAMPUS WHERE BuildingID = @buildingID";
            cmd.Parameters.AddWithValue("@columnid", srcBuild);
            cmd.Parameters.AddWithValue("@buildingID", destBuild);
            string directions = cmd.ExecuteScalar().ToString();
            return directions;
        }
    }
}
