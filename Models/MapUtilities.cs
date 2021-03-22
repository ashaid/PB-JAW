using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Included;
using Python.Runtime;
using System.IO;
using System.Data.SQLite;
using System.Globalization;

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

            switch (BuildingNumber)
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
            if (name == "Business Education Complex")
            {
                templateName = "/wwwroot/template/BEC.jpeg";
            }

            else if (name == "Patrick F. Taylor Hall")
            {
                templateName = "/wwwroot/template/PFT-1.jpeg";
            }
            else if (name == "Lockett Hall")
            {
                templateName = "/wwwroot/template/LOCKETT.jpeg";
            }

            return templateName;
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

                    // create initial map
                    string buildingName = FindBuilding(Maps[i].Building);
                    string dictionary = FindBuildingDictionary(buildingName);
                    string roomNumber = Maps[i].RoomNumber.ToString();

                    string templatePath = FindMapTemplate(buildingName);

                    // name of new file
                    string name = buildingName + "_" + roomNumber + ".jpeg";

                    CreateImage(templatePath, dictionary, roomNumber, name, gs);

                    names.Add(name);

                }
                
            }
            return names;
        }

        // calculate text directions for the user
        string Directions(string srcRoom, string srcBuild, string destRoom, string destBuild)
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

        string timeQuery(string srcRoom, string destRoom, string destBuild, string srcBuild){

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
            double srcToExit = exitTimes(srcRoom, srcBuild, destBuild, sqlCon); //returns time in seconds
            double buildTime = destTimes(destRoom, destBuild, srcBuild, sqlCon); //returns time in seconds
            double entToDest = campusTimes(srcBuild, destBuild, sqlCon); //returns time in seconds
            
            string eta = timeCompiler(srcToExit, buildTime, entToDest); //adds time to users current time to provide 
                                                                        //an estimated time of arrival
            string time = "Your expected arrival time is " + eta + ".\n";
            return time;
        }

        double exitTimes(string srcRoom, string srcBuild, string destBuild, SQLiteConnection con){
        
            double timeFromSrc;
            using var cmd = new SQLiteCommand(con);
            
            if (srcBuild == null)
            {
                timeFromSrc = -1; //time incalculable if coming from off of campus
            }
            else if (srcBuild == "Patrick F. Taylor Hall")
            {
                if (srcBuild == destBuild)
                {
                    timeFromSrc = 0; //negligible amount of time going from between classes
                }
                else if (destBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT TimeBEC FROM PFT WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    timeFromSrc = cmd.ExecuteScalar();
                }
                else if (destBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT TimeLOC FROM PFT WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    timeFromSrc = cmd.ExecuteScalar();
                }
            }
            else if (srcBuild == "Business Education Complex")
            {
                if (srcBuild == destBuild)
                {
                    timeFromSrc = 0;
                }
                else if (destBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT TimePFT FROM BEC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    timeFromSrc = cmd.ExecuteScalar();
                }
                else if (destBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT TimeLOC FROM BEC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    timeFromSrc = cmd.ExecuteScalar();
                }
            }
            else if (srcBuild == "Lockett Hall")
            {
                if (srcBuild == destBuild)
                {
                    timeFromSrc = 0;
                }
                lse if (destBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT TimeBEC FROM LOC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    timeFromSrc = cmd.ExecuteScalar();
                }
                else if (destBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT TimePFT FROM LOC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                    timeFromSrc = cmd.ExecuteScalar();
                }
            }

            return timeConverter(timeFromSrc);
        }

        double destTimes(string destRoom, string destBuild, string srcBuild, SQLiteConnection con){
        
            double timeToDest = 0;

             using var cmd = new SQLiteCommand(con);
            if (srcBuild == null)
            {
                timeToDest = -1; //time incalculable if coming from off of campus
            }
            else if (destBuild == "Patrick F. Taylor Hall")
            {
                if (srcBuild == destBuild)
                {
                    timeToDest = 0; //negligible amount of time going from between classes
                }
                else if (srcBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT TimeBEC FROM PFT WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    timeToDest += cmd.ExecuteScalar();
                }
                else if (srcBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT TimeLOC FROM PFT WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    timeToDest += cmd.ExecuteScalar();
                }
            }
            else if (destBuild == "Business Education Complex")
            {
                if (srcBuild == destBuild)
                {
                    timeToDest = 0;
                }
                else if (srcBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT TimePFT FROM BEC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    timeToDest += cmd.ExecuteScalar();
                }
                else if (srcBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT TimeLOC FROM BEC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    timeToDest += cmd.ExecuteScalar();
                }
            }
            else if (destBuild == "Lockett Hall")
            {
                if (srcBuild == destBuild)
                {
                    timeToDest = 0;
                }
                lse if (srcBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT TimeBEC FROM LOC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    timeToDest += cmd.ExecuteScalar();
                }
                else if (srcBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT TimePFT FROM LOC WHERE ROOM = @roomNum";
                    cmd.Parameters.AddWithValue("@roomNum", destRoom);
                    timeToDest += cmd.ExecuteScalar();
                }
            }
       
            return timeConverter(timeToDest);
        }

        double campusTimes(string srcBuild, string destBuild, SQLiteConnection con){
        
            double timeToBuild;
            using var cmd = new SQLiteCommand(con);
            
            if (srcBuild == null)
            {
                timeToBuild = -1; //time incalculable if coming from off of campus
            }
            else if (srcBuild == "Patrick F. Taylor Hall")
            {
                if (srcBuild == destBuild)
                {
                    timeToBuild = 0; //negligible amount of time going from between classes
                }
                else if (destBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT TimeBEC FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    timeToBuild = cmd.ExecuteScalar();
                }
                else if (destBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT TimeLOC FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    timeToBuild = cmd.ExecuteScalar();
                }
            }
            else if (srcBuild == "Business Education Complex")
            {
                if (srcBuild == destBuild)
                {
                    timeToBuild = 0;
                }
                else if (destBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT TimePFT FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    timeToBuild = cmd.ExecuteScalar();
                }
                else if (destBuild == "Lockett Hall")
                {
                    cmd.CommandText = "SELECT TimeLOC FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    timeToBuild = cmd.ExecuteScalar();
                }
            }
            else if (srcBuild == "Lockett Hall")
            {
                if (srcBuild == destBuild)
                {
                    timeToBuild = 0;
                }
                lse if (destBuild == "Business Education Complex")
                {
                    cmd.CommandText = "SELECT TimeBEC FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    timeToBuild = cmd.ExecuteScalar();
                }
                else if (destBuild == "Patrick F. Taylor Hall")
                {
                    cmd.CommandText = "SELECT TimePFT FROM CAMPUS WHERE BuildingID = @destBuild";
                    cmd.Parameters.AddWithValue("@destBuild", destBuild);
                    timeToBuild = cmd.ExecuteScalar();
                }
            }

            return timeConverter(timeToBuild);
        }


        double timeConverter(double a){
        
            if(a > 1.0){
                int wholeNum;
                while(wholeNum <= a){
                wholeNum++
                }

                double minutesToSeconds = wholeNum * 60;
                double seconds = 60*(a - wholeNum);
                double totalSeconds = minutesToSeconds + seconds;

                return totalSeconds;
                
            }
            else{
                return a*60;
            }
        }

        string timeCompiler(double srcToExit, double buildTime, double desToExit){

            DateTime localTime = DateTime.Now;

            return localTime.AddSeconds(srcToExit + buildTime + desToExit);
            
        }


        }


    }
}
