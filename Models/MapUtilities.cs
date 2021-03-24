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
                    details.Add("/wwwroot/template/LOCKETT-1.jpeg");
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
            // return new image file location (array)
            return names;
        }

        // calculate text directions for the user
        public string Directions(List<MapModel> Maps)
        {
            string directions = "";
            /*
             * establishes the sqlite connection and throws an error if the connection is not established
             * Citation: https://www.codeguru.com/csharp/.net/net_data/using-sqlite-in-a-c-application.html#:~:text=Getting%20Started%20with%20SQLite%20from%20a%20.&text=Open%20Visual%20Studio%2C%20select%20new,as%20pictured%20in%20Figure%201.
             * The connection and try/catch idea was originally posted by Tapas Pal
             */
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

            List<string> startingDetails = new List<string>();
            FindDetails(Maps[0].Building, startingDetails);
            List<string> endingDetails = new List<string>();
            FindDetails(Maps[1].Building, endingDetails);

            string srcRoom;
            string destRoom;
            string srcBuild;
            string destBuild;
            if (Maps[0].Building.Contains("-1")) 
            {
                directions = "You have selected no starting point, therefore, directions will not be provided. Please refer to the map.";
            }
            else 
            {
                // 0 = name, 1 = dictionary, 2 = template path, 3 = dest direction, 4 = exit direction, 5 = time traveled
                srcRoom = Maps[0].RoomNumber.ToString();
                destRoom = Maps[1].RoomNumber.ToString();
                srcBuild = startingDetails[1].ToUpper().Replace("\n", String.Empty);
                destBuild = endingDetails[1].ToUpper().Replace("\n", String.Empty);
                if (srcBuild == destBuild)
                {
                    directions = "You are already in the building. Please refer to the map in order to find your classroom, it is nearby!";
                }
                else
                {
                    string extDirections = ExitDirections(srcRoom, srcBuild, endingDetails[4], sqlCon);
                    string toDirections = DestDirections(destRoom, destBuild, startingDetails[3], sqlCon);
                    string campDirections = CampusDirections(startingDetails[3], endingDetails[0], sqlCon);
                    directions = extDirections + campDirections + toDirections;
                }
            }
            //closes sqlite connection
            sqlCon.Close();

            return directions;
        }
        string ExitDirections(string srcRoom, string srcBuild, string destBuild, SQLiteConnection con)
        {

            using var cmd = new SQLiteCommand(con);
            string directions = "";

            cmd.CommandText = "SELECT " + destBuild + " FROM " + srcBuild + " WHERE ROOM = @roomNum";
            cmd.Parameters.AddWithValue("@roomNum", srcRoom);
            cmd.Prepare();
            directions = cmd.ExecuteScalar().ToString();
            return directions;
        }

        string DestDirections(string destRoom, string destBuild, string srcBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            string directions = "";

            cmd.CommandText = "SELECT " + srcBuild + " FROM " + destBuild + " WHERE ROOM = @roomNum";
            cmd.Parameters.AddWithValue("@roomNum", destRoom);
            cmd.Prepare();
            directions = cmd.ExecuteScalar().ToString();

            return directions;
        }

        string CampusDirections(string srcBuild, string destBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            string directions = "";

            cmd.CommandText = "SELECT " + srcBuild + " FROM CAMPUS WHERE BuildingID = @buildingID";
            cmd.Parameters.AddWithValue("@buildingID", destBuild);
            directions = cmd.ExecuteScalar().ToString();
            return directions;
        }

        public string TimeQuery(List<MapModel> Maps)
        {
            //time variable declaration;
            string time = "";

            /*
             * establishes the sqlite connection and throws an error if the connection is not established
             * Citation: https://www.codeguru.com/csharp/.net/net_data/using-sqlite-in-a-c-application.html#:~:text=Getting%20Started%20with%20SQLite%20from%20a%20.&text=Open%20Visual%20Studio%2C%20select%20new,as%20pictured%20in%20Figure%201.
             * The connection and try/catch idea was originally posted by Tapas Pal
             */
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
            //establishes the library for the different SQL calls
            List<string> startingDetails = new List<string>();
            FindDetails(Maps[0].Building, startingDetails);
            List<string> endingDetails = new List<string>();
            FindDetails(Maps[1].Building, endingDetails);

            if (Maps[0].Building.Contains("-1"))
            {
                time = "You have no starting destination, therefore, arrival time can not be calculated. Walk fast!";
            }
            else
            {
                //sql variable declarations
                string srcRoom = Maps[0].RoomNumber.ToString();
                string destRoom = Maps[1].RoomNumber.ToString();
                string srcBuild = startingDetails[1].ToUpper().Replace("\n", String.Empty);
                string destBuild = endingDetails[1].ToUpper().Replace("\n", String.Empty);
                if (srcBuild == destBuild)
                {
                    time = "Your next class is in the same building. It will not take long to arrive there, but still move quickly!";
                }
                else
                {
                    double srcToExit = ExitTimes(srcRoom, srcBuild, endingDetails[5], sqlCon);
                    double buildTime = DestTimes(destRoom, destBuild, startingDetails[5], sqlCon);
                    double entToDest = CampusTimes(startingDetails[5], destBuild, sqlCon);

                    //round the minutes up to a solid minute
                    //meant to give the user a delayed time as no one will read the seconds and it will only make them walk faster if they think it will take longer
                    double eta = Math.Ceiling(srcToExit + buildTime + entToDest);

                    //grabs the users current time
                    DateTime currentTime = DateTime.Now;
                    //adds the travel time minutes to current time
                    DateTime updateTime = currentTime.AddMinutes(eta);
                    //changes the updated time to a string called arrival time
                    string arrivalTime = updateTime.ToString("t");
                    time = "Your expected arrival time is " + arrivalTime + ".\n";
                }
            }
            sqlCon.Close();
            return time;
        }

        double ExitTimes(string srcRoom, string srcBuild, string destBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT " + destBuild + " FROM " + srcBuild + " WHERE ROOM = @roomNum";
            cmd.Parameters.AddWithValue("@roomNum", srcRoom);

            double exitTime = Convert.ToDouble(cmd.ExecuteScalar());
            return exitTime;
        }

        double DestTimes(string destRoom, string destBuild, string srcBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT " + srcBuild + " FROM " + destBuild + " WHERE ROOM = @roomNum";
            cmd.Parameters.AddWithValue("@roomNum", destRoom);

            double timeToDest = Convert.ToDouble(cmd.ExecuteScalar());
            return timeToDest;
        }

        double CampusTimes(string srcBuild, string destBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT " + srcBuild + " FROM CAMPUS WHERE BuildingID = @buildingid";
            cmd.Parameters.AddWithValue("@buildingid", destBuild);

            double timeToBuild = Convert.ToDouble(cmd.ExecuteScalar());
            return timeToBuild;
        }
    }
}
