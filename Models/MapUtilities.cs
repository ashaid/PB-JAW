using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
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

        /**
         * This method initializes python and returns the pointer.
         * Also protection against running multiple python initializations.
         *
         * method: StartPython()
         *
         * return type: Task<IntPtr>
         *
         * @author Anthony Shaidaee
         * @since 3/1/2021
         *
         */
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

        /**
         * This method takes the template path, dictionary,
         * room number, name of new file, and python pointer
         * and calls the corresponding python script to actually
         * create the new images
         *
         * method: CreateImage
         *
         * return type: void
         *
         * parameters:
         * templatePath      [string]        path to template
         * dictionary        [string]        correct python dictionary for building
         * roomNumber        [string]        room number 
         * name              [string]        name of new image
         * gs                [IntPtr]        pointer to python intialization
         *
         * @author Anthony Shaidaee
         * @since 3/1/2021
         *
         */
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
            Console.WriteLine(dictionary);
            mod.main(host.ContentRootFileProvider.GetFileInfo(templatePath).PhysicalPath, dictionary, roomNumber, name);
            PythonEngine.ReleaseLock(gs);
        }

        /**
         * This method takes a building number and an empty list
         * and returns the lsit filled with the name of building,
         * dictionary name, template path, from and src fields, and
         * time field.
         *
         * method: FindDetails
         *
         * return type: List<string>
         *
         * parameters:
         * BuildingNumber      [string]        building number
         * details             [List<string>]  array to be filled out with correct details
         *
         * @author Anthony Shaidaee
         * @since 3/23/2021
         *
         */
        public List<string> FindDetails(string BuildingNumber, List<string> details)
        {

            switch (BuildingNumber)
            {
                case "0":
                    details.Add("Business Education Complex");
                    details.Add("bec");
                    details.Add("/wwwroot/template/default/BEC.jpeg");
                    details.Add("FrmBEC");
                    details.Add("ExtBEC");
                    details.Add("TimeBEC");
                    break;
                case "1":
                    details.Add("Patrick F. Taylor Hall");
                    details.Add("pft");
                    details.Add("/wwwroot/template/default/PFT-1.jpeg");
                    details.Add("FrmPFT");
                    details.Add("ExtPFT");
                    details.Add("TimePFT");
                    break;
                case "2":
                    details.Add("Lockett Hall");
                    details.Add("loc");
                    details.Add("/wwwroot/template/default/LOCKETT-1.jpeg");
                    details.Add("FrmLoc");
                    details.Add("ExtLoc");
                    details.Add("TimeLoc");
                    break;
            }
            return details;
        }

        /**
         * This method takes the template model maps and converts to
         * corresponding details to create the maps. Returns a list
         * of the created file locations to be displayed to the user.
         *
         * method: CreateMap
         *
         * return type: Task<List<string>
         *
         * parameters:
         * Maps      [List<MapModel>]        list of maps
         *
         * @author Anthony Shaidaee
         * @since 3/23/2021
         *
         */
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

        /**
        * This method calls string queries to multiple tables in the Locations.db
        * database and adds them to a string which will detail the directions from
        * the source room to the destination room
        *
        * method: Directions
        *
        * return type: string
        *
        * parameters:
        * Maps      [List<MapModel>]        List which contains source
        * building and destination building input from the user
        *
        *
        * @author Brennen Calato
        * @since 3/21/2021
        *
        */
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

        /**
        * This method queries the building table of the destination
        * building and takes the directions from the source room to the
        * nearest exit (nearest to destination building)
        *
        * method: ExitDirections
        *
        * return type: string
        *
        * parameters:
        * srcRoom      [string]    used for database navigation
        * 
        * srcBuild      [string]    used for database navigation
        * 
        * destBuild     [string]    used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Brennen Calato
        * @since 3/21/2021
        *
        */
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

        /**
        * This method queries the building table of the destination
        * building and takes the directions from the destination room to the
        * nearest exit (nearest to source building)
        *
        * method: DestDirections
        *
        * return type: string
        *
        * parameters:
        * destRoom      [string]    used for database navigation
        * 
        * srcBuild      [string]    used for database navigation
        * 
        * destBuild     [string]    used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Brennen Calato
        * @since 3/21/2021
        *
        */
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

        /**
        * This method queries the CAMPUS table in Locations.db
        * and takes the directions from the source building to
        * the destination building
        *
        * method: CampusDirections
        *
        * return type: string
        *
        * parameters:
        * srcBuild      [string]    used for database navigation
        * 
        * destBuild     [string]    used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Brennen Calato
        * @since 3/21/2021
        *
        */
        string CampusDirections(string srcBuild, string destBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            string directions = "";

            cmd.CommandText = "SELECT " + srcBuild + " FROM CAMPUS WHERE BuildingID = @buildingID";
            cmd.Parameters.AddWithValue("@buildingID", destBuild);
            directions = cmd.ExecuteScalar().ToString();
            return directions;
        }

        /**
        * This method calls queries to multiple tables in the Locations.db
        * database and adds them to the users current time to make an estimated
        * time of arrival
        *
        * method: TimeQuery
        *
        * return type: string
        *
        * parameters:
        * Maps      [List<MapModel>]        List which contains source
        * building and destination building input from the user
        *
        *
        * @author Joshua Rovira
        * @since 3/21/2021
        *
        */
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
                    double entToDest = DestTimes(destRoom, destBuild, startingDetails[5], sqlCon);
                    double buildTime = CampusTimes(startingDetails[5], endingDetails[0], sqlCon);

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

        /**
        * This method queries the building table of the source
        * building and takes the time from the source room to the
        * nearest exit (nearest to destination building)
        *
        * method: ExitTimes
        *
        * return type: double
        *
        * parameters:
        * srcRoom       [string]    used for database navigation
        * 
        * srcBuild      [string]    used for database navigation
        * 
        * destBuild     [string]    used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Joshua Rovira
        * @since 3/21/2021
        *
        */
        double ExitTimes(string srcRoom, string srcBuild, string destBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT " + destBuild + " FROM " + srcBuild + " WHERE ROOM = @roomNum";
            cmd.Parameters.AddWithValue("@roomNum", srcRoom);

            double exitTime = Convert.ToDouble(cmd.ExecuteScalar());
            return exitTime;
        }

        /**
        * This method queries the building table of the destination
        * building and takes the time from the destination room to the
        * nearest exit (nearest to source building)
        *
        * method: DestTimes
        *
        * return type: double
        *
        * parameters:
        * destRoom      [string]    used for database navigation
        * 
        * srcBuild      [string]    used for database navigation
        * 
        * destBuild     [string]    used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Joshua Rovira
        * @since 3/21/2021
        *
        */
        double DestTimes(string destRoom, string destBuild, string srcBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT " + srcBuild + " FROM " + destBuild + " WHERE ROOM = @roomNum";
            cmd.Parameters.AddWithValue("@roomNum", destRoom);

            double timeToDest = Convert.ToDouble(cmd.ExecuteScalar());
            return timeToDest;
        }

        /**
        * This method queries the CAMPUS table
        * and takes the time from the source building to 
        * the destination building
        *
        * method: CampusTimes
        *
        * return type: double
        *
        * parameters:
        * 
        * srcBuild      [string]    used for database navigation
        * 
        * destBuild     [string]    used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Joshua Rovira
        * @since 3/21/2021
        *
        */
        double CampusTimes(string srcBuild, string destBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT " + srcBuild + " FROM CAMPUS WHERE BuildingID = @buildingid";
            cmd.Parameters.AddWithValue("@buildingid", destBuild);

            double timeToBuild = Convert.ToDouble(cmd.ExecuteScalar());
            return timeToBuild;
        }

        /**
        * This method queries the building tables for both
        * the source and destination rooms to find the roomIDs
        * for the called room. If the room exists in the building,
        * a true value is returned.
        *
        * method: CheckRoom
        *
        * return type: bool
        *
        * parameters:
        * Maps      [List<MapModel>]        List which contains source
        *                                   building and destination building input from the user
        *
        *
        * @author Brennen Calato
        * @since 3/23/2021
        *
        */
        public bool CheckRoom(List<MapModel> Maps)
        {
            bool validRooms = false;

            List<string> startingDetails = new List<string>();
            FindDetails(Maps[0].Building, startingDetails);
            List<string> endingDetails = new List<string>();
            FindDetails(Maps[1].Building, endingDetails);
            string destRoom = Maps[1].RoomNumber.ToString();
            string destBuild = endingDetails[1].ToUpper().Replace("\n", String.Empty);
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
            using var cmd = new SQLiteCommand(sqlCon);
            cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM " + destBuild + " WHERE Room = @roomNum)";
            cmd.Parameters.AddWithValue("@roomNum", destRoom);
            int validDestRoom = Convert.ToInt32(cmd.ExecuteScalar());
            if (Maps[0].Building.Contains("-1") && validDestRoom == 1)
            {
                validRooms = true;
            }
            else 
            {
                string srcBuild = startingDetails[1].ToUpper().Replace("\n", String.Empty);
                string srcRoom = Maps[0].RoomNumber.ToString();
                cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM " + srcBuild + " WHERE Room = @roomNum)";
                cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                int validStartRoom = Convert.ToInt32(cmd.ExecuteScalar());
                if (validStartRoom == 1 && validDestRoom == 1)
                {
                    validRooms = true;
                }
            }
            return validRooms;
        }
    }
}
