using Microsoft.AspNetCore.Hosting;
using Python.Included;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

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
                Installer.PipInstallModule("scipy");
                Installer.PipInstallModule("numpy");
                Installer.PipInstallModule("scikit-network");
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }

            IntPtr gs = PythonEngine.AcquireLock();

            try
            {
                dynamic os = PythonEngine.ImportModule("os");
                //Console.WriteLine("### Current working directory:\n\t" + os.getcwd());
                //Console.WriteLine("### PythonPath:\n\t" + PythonEngine.PythonPath);

                dynamic pillow = Py.Import("PIL");
                //Console.WriteLine("Pillow version: " + pillow.__version__);
                dynamic scipy = Py.Import("scipy");
                dynamic np = Py.Import("numpy");
                dynamic sknetwork = Py.Import("sknetwork");
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
                    details.Add("BEC");
                    break;
                case "1":
                    details.Add("Patrick F. Taylor Hall");
                    details.Add("pft");
                    details.Add("/wwwroot/template/default/PFT-1.jpeg");
                    details.Add("FrmPFT");
                    details.Add("ExtPFT");
                    details.Add("TimePFT");
                    details.Add("PFT");
                    break;
                case "2":
                    details.Add("Lockett Hall");
                    details.Add("loc");
                    details.Add("/wwwroot/template/default/LOCKETT-1.jpeg");
                    details.Add("FrmLoc");
                    details.Add("ExtLoc");
                    details.Add("TimeLoc");
                    details.Add("LOC");
                    break;
                case "3":
                    details.Add("Patrick F. Taylor Hall");
                    details.Add("pft2");
                    details.Add("/wwwroot/template/default/PFT-2.jpeg");
                    details.Add("FrmPFT");
                    details.Add("ExtPFT");
                    details.Add("TimePFT");
                    details.Add("PFT");
                    break;
                case "4":
                    details.Add("Lockett Hall");
                    details.Add("loc2");
                    details.Add("/wwwroot/template/default/Lockett-2.jpeg");
                    details.Add("FrmLoc");
                    details.Add("ExtLoc");
                    details.Add("TimeLoc");
                    details.Add("LOC");
                    break;
                case "5":
                    details.Add("Lockett Hall");
                    details.Add("locb");
                    details.Add("/wwwroot/template/default/LockettBasement.jpeg");
                    details.Add("FrmLoc");
                    details.Add("ExtLoc");
                    details.Add("TimeLoc");
                    details.Add("LOC");
                    break;

            }
            return details;
        }
        public async Task<List<string>> EditMap(List<MapModel> Maps)
        {
            /*     for each map call edit image
            *      set starting and ending location 
            *      different buildings for the first dest = predetermined exit and second map starting = predetermine starting
            *      highlighting classrooms and entrances and exits while drawing the path 
            */
            List<string> names = new List<string>();
            List<string> details1 = new List<string>();
            List<string> details2 = new List<string>();
            bool done = false;
            while (!done)
            {
                IntPtr gs = await StartPython();

                details1 = FindDetails(Maps[0].Building, details1);
                // var for map 1
                string buildingName1 = details1[0];
                string dictionary1 = details1[1];
                string templatePath1 = details1[2];
                string roomNumber1 = Maps[0].RoomNumber.ToString();
                string name1 = buildingName1 + "_" + roomNumber1 + ".jpeg";


                details2 = FindDetails(Maps[1].Building, details2);
                // var for map 2
                string buildingName2 = details2[0];
                string dictionary2 = details2[1];
                string templatePath2 = details2[2];
                string roomNumber2 = Maps[1].RoomNumber.ToString();
                string name2 = buildingName2 + "_" + roomNumber2 + ".jpeg";

                // if same building and floor
                if (dictionary1 == dictionary2)
                {
                    string newName = buildingName1 + "_" + roomNumber1 + "_to_" + roomNumber2 + ".jpeg";
                    int start = Int32.Parse(roomNumber1);
                    int dest = Int32.Parse(roomNumber2);
                    PythonPath(templatePath1, newName, dictionary1, start, dest, gs);
                    names.Add(newName);

                    done = true;
                }
                else
                {
                    //creates a dictionary of the source building
                    Dictionary<string, int> findExitNode = Nodes(details1[1]);
                    int start = Int32.Parse(roomNumber1);
                    _ = Int32.Parse(roomNumber2);
                    //sets destRoom to the default exit of the source building towards the other building
                    int dest = findExitNode[details2[4]];
                    //Calls the python path method
                    PythonPath(templatePath1, name1, details1[1], start, dest, gs);

                    //creates a dictionary of the destination building
                    Dictionary<string, int> findEntNode = Nodes(details2[1]);
                    //sets source room to the default entrace node of the destination building from the source building
                    start = findEntNode[details1[3]];
                    //sets the destination room to the original user input
                    dest = Maps[1].RoomNumber;
                    //restarts Python as it is closed by the previous PythonPath
                    IntPtr gs2 = await StartPython();
                    //Calls the python path method
                    PythonPath(templatePath2, name2, details2[1], start, dest, gs2);

                    done = true;
                }

            }
            return names;
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
        //public async Task<List<string>> CreateMap(List<MapModel> Maps)
        //{
        //    List<string> names = new List<string>();

        //    // for each map in list 
        //    for (int i = 0; i < Maps.Count; i++)
        //    {
        //        // no starting destination
        //        if (Maps[i].Building.Contains("-1"))
        //        {
        //            names.Add("No starting location selected");
        //        }
        //        else
        //        {
        //            // python memory store
        //            IntPtr gs = await StartPython();

        //            // 0 = name, 1 = dictionary, 2 = template path, 3 = dest direction, 4 = exit direction, 5 = time traveled
        //            List<string> details = new List<string>();
        //            details = FindDetails(Maps[i].Building, details);

        //            // var for map
        //            string buildingName = details[0];
        //            string dictionary = details[1];
        //            string templatePath = details[2];
        //            string roomNumber = Maps[i].RoomNumber.ToString();

        //            // name of new file
        //            string name = buildingName + "_" + roomNumber + ".jpeg";

        //            CreateImage(templatePath, dictionary, roomNumber, name, gs);

        //            // add new file to names array
        //            names.Add(name);
        //            details.Clear();
        //        }

        //    }
        //    // return new image file location (array)
        //    return names;
        //}

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
        * @author Brennen Calato and Josh Rovira
        * @since 3/21/2021
        *
        */
        public string Directions(List<MapModel> Maps)
        {
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
            //Creates list for starting details
            List<string> startingDetails = new List<string>();
            //Finds the preset possible details of the source building
            FindDetails(Maps[0].Building, startingDetails);
            //Creates list for ending details
            List<string> endingDetails = new List<string>();
            //Finds the preset possible details of the destination building
            FindDetails(Maps[1].Building, endingDetails);

            //variable declarations
            string srcRoom;
            string destRoom;
            string srcBuild;
            string destBuild;
            string directions;
            if (Maps[0].Building.Contains("-1"))
            {
                directions = "You have selected no starting point, therefore, directions will not be provided. Please refer to the map.";
            }
            else
            {
                // 0 = name, 1 = dictionary, 2 = template path, 3 = dest direction, 4 = exit direction, 5 = time traveled, 6 = table identifier
                srcRoom = Maps[0].RoomNumber.ToString();
                destRoom = Maps[1].RoomNumber.ToString();
                srcBuild = startingDetails[6].Replace("\n", String.Empty);
                destBuild = endingDetails[6].Replace("\n", String.Empty);
                //checks if starting building and destination building are the same
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
        * srcRoom       [string]    the source room, used for database navigation
        * 
        * srcBuild      [string]    the source building, used for database navigation
        * 
        * destBuild     [string]    the destination building, used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Brennen Calato and Josh Rovira
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
        * destRoom      [string]    the destination room, used for database navigation
        * 
        * srcBuild      [string]    the source building, used for database navigation
        * 
        * destBuild     [string]    the destination building, used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Brennen Calato and Josh Rovira
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
        * srcBuild      [string]    the source building, used for database navigation
        * 
        * destBuild     [string]    the destinatioon building, used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Brennen Calato and Josh Rovira
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
        *                                   user input foor room numbers and buildings
        *
        *
        * @author Joshua Rovira and Brennen Calato
        * @since 3/21/2021
        *
        */
        public string TimeQuery(List<MapModel> Maps)
        {

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

            //time variable declaration;
            string time;
            if (Maps[0].Building.Contains("-1"))
            {
                time = "You have no starting destination, therefore, arrival time can not be calculated. Walk fast!";
            }
            else
            {
                //sql variable declarations
                string srcRoom = Maps[0].RoomNumber.ToString();
                string destRoom = Maps[1].RoomNumber.ToString();
                string srcBuild = startingDetails[6].Replace("\n", String.Empty);
                string destBuild = endingDetails[6].Replace("\n", String.Empty);
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
        * srcRoom       [string]    the source room, used for database navigation
        * 
        * srcBuild      [string]    the source build, used for database navigation
        * 
        * destBuild     [string]    the destination building, used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Joshua Rovira and Brennen Calato
        * @since 3/21/2021
        *
        */
        double ExitTimes(string srcRoom, string srcBuild, string destBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = "SELECT " + destBuild + " FROM " + srcBuild + " WHERE ROOM = @roomNum"
            };
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
        * destRoom      [string]    the destination room, used for query
        * 
        * srcBuild      [string]    the source building, used for query
        * 
        * destBuild     [string]    the destination building, used for query
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Joshua Rovira and Brennen Calato
        * @since 3/21/2021
        *
        */
        double DestTimes(string destRoom, string destBuild, string srcBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = "SELECT " + srcBuild + " FROM " + destBuild + " WHERE ROOM = @roomNum"
            };
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
        * srcBuild      [string]    the source building, used for database navigation
        * 
        * destBuild     [string]    the destination building, used for database navigation
        * 
        * con           [SQLiteConnection]  connects to Locations.db database
        *
        *
        * @author Joshua Rovira and Brennen Calato
        * @since 3/21/2021
        *
        */
        double CampusTimes(string srcBuild, string destBuild, SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = "SELECT " + srcBuild + " FROM CAMPUS WHERE BuildingID = @buildingid"
            };
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
        * Maps      [List<MapModel>]        List which contains room and building input from the user
        *
        *
        * @author Brennen Calato
        * @since 3/23/2021
        *
        */
        public bool CheckRoom(List<MapModel> Maps)
        {
            //whether user entered rooms are valid
            bool validRooms = false;

            //Creates list for starting details
            List<string> startingDetails = new List<string>();
            //Finds the preset possible details of the source building
            FindDetails(Maps[0].Building, startingDetails);
            //Creates list for ending details
            List<string> endingDetails = new List<string>();
            //Finds the preset possible details of the destination building
            FindDetails(Maps[1].Building, endingDetails);
            //sets the destination room to the user input and converts the int to a string
            string destRoom = Maps[1].RoomNumber.ToString();
            //sets destBuild to its table identifier and deletes its new line character
            string destBuild = endingDetails[6].Replace("\n", String.Empty);
            /*
             * establishes the sqlite connection and prints an error if the connection is not established
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
            //creates a new SQLite command
            using var cmd = new SQLiteCommand(sqlCon)
            {
                //checks if user entered room number exists in the database, returns 1 or 0
                CommandText = "SELECT EXISTS(SELECT 1 FROM " + destBuild + " WHERE Room = @roomNum)"
            };
            cmd.Parameters.AddWithValue("@roomNum", destRoom);
            //converts the return value of the query to an int and assigns it to validDestRoom
            int validDestRoom = Convert.ToInt32(cmd.ExecuteScalar());
            //in the case of the user having no starting destination, checks if destination room is valid
            if (Maps[0].Building.Contains("-1") && validDestRoom == 1)
            {
                validRooms = true;
            }
            //checks whether the source and destination rooms are valid
            else
            {
                //sets the srcBuild to is table identifier and removes new line character
                string srcBuild = startingDetails[6].Replace("\n", String.Empty);
                //sets srcRoom to string version of the user entered room number
                string srcRoom = Maps[0].RoomNumber.ToString();
                //checks if rooms are the same, if true returns that rooms entered are invalid
                if (srcRoom == destRoom)
                {
                    return validRooms;
                }
                //checks whether the srcRoom is in the database
                cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM " + srcBuild + " WHERE Room = @roomNum)";
                cmd.Parameters.AddWithValue("@roomNum", srcRoom);
                int validStartRoom = Convert.ToInt32(cmd.ExecuteScalar());
                //checks whether srcRoom and destRoom are both valid, if they are it sets validRooms to true
                if (validStartRoom == 1 && validDestRoom == 1)
                {
                    validRooms = true;
                }
            }
            //closes SQLite Connection
            sqlCon.Close();
            return validRooms;
        }

        /*
         * This method is used to provide control flow to the method which draws paths onto images.
         * It clarifies which exit/entrance nodes are needed if a user is not just travelling 
         * within a building.
         * 
         * method: CreatePath
         * 
         * return type: Task
         * 
         * parameters:
         *      Maps        [List<MapsModel>]       list that contains room and building data from user input
         *      
         *      path1       [string]                Path to the source image that will have the path drawn on
         *      
         *      path2       [string]                Path to the destination image that will have the path drawn on
         *      
         * @author Brennen Calato
         * @since 3/26/2021
         */
        //public async Task CreatePath(List<MapModel> Maps, string path1, string path2)
        //{
        //    IntPtr gs = await StartPython();
        //    //Creates list for starting details
        //    List<string> startingDetails = new List<string>();
        //    //Finds the preset possible details of the source building
        //    FindDetails(Maps[0].Building, startingDetails);
        //    //Creates list for ending details
        //    List<string> endingDetails = new List<string>();
        //    //Finds the preset possible details of the destination building
        //    FindDetails(Maps[1].Building, endingDetails);
        //    string srcBuild = startingDetails[1];
        //    string destBuild = endingDetails[1];

        //    //user entered source room number
        //    int srcRoom = Maps[0].RoomNumber;
        //    //user entered destination room number
        //    int destRoom = Maps[1].RoomNumber;


        //    if (srcBuild == destBuild)
        //    {
        //        PythonPath(path1, srcBuild, srcRoom, destRoom, gs);
        //        IntPtr gs2 = await StartPython();
        //        PythonPath(path2, srcBuild, srcRoom, destRoom, gs2);
        //    }
        //    else
        //    {
        //        //creates a dictionary of the source building
        //        Dictionary<string, int> findExitNode = Nodes(srcBuild);
        //        //sets destRoom to the default exit of the source building towards the other building
        //        destRoom = findExitNode[endingDetails[4]];
        //        //Calls the python path method
        //        PythonPath(path1, srcBuild, srcRoom, destRoom, gs);

        //        //creates a dictionary of the destination building
        //        Dictionary<string, int> findEntNode = Nodes(destBuild);
        //        //sets source room to the default entrace node of the destination building from the source building
        //        srcRoom = findEntNode[startingDetails[3]];
        //        //sets the destination room to the original user input
        //        destRoom = Maps[1].RoomNumber;
        //        //restarts Python as it is closed by the previous PythonPath
        //        IntPtr gs2 = await StartPython();
        //        //Calls the python path method
        //        PythonPath(path2, destBuild, srcRoom, destRoom, gs2);
        //    }

        //}

        /*
         * This method returns a dictionary of entrance and exit nodes depending on the building
         * 
         * method: Nodes
         * 
         * return type: Dictionary<string, int>
         * 
         * parameters:
         *      build           [string]    the building which the nodes are being retrieved for
         * 
         * @author Brennen Calato
         * @since 3/26/2021
         */
        public Dictionary<string, int> Nodes(string build)
        {
            Dictionary<string, int> findNode = new Dictionary<string, int>();
            if (build == "bec")
            {
                findNode.Add("FrmPFT", 9999);
                findNode.Add("FrmLoc", 9999);
                findNode.Add("ExtPFT", 9999);
                findNode.Add("ExtLoc", 9999);
            }
            else if (build == "loc")
            {
                findNode.Add("FrmPFT", -1);
                findNode.Add("FrmBEC", 9999);
                findNode.Add("ExtPFT", -1);
                findNode.Add("ExtBEC", 9999);
            }
            else if (build == "pft")
            {
                findNode.Add("FrmBEC", 9999);
                findNode.Add("FrmLoc", -2);
                findNode.Add("ExtBEC", 9999);
                findNode.Add("ExtLoc", -2);
            }

            return findNode;
        }

        /*
         * This method calls a python file that used nodes to find the shortest path between classrooms.
         * It then draws the path between the rooms on the result maps.
         * 
         * method: PythonPath
         * 
         * return type: void
         * 
         * parameters:
         *      templatePath        [string]        The path to the template map image
         *      
         *      dictionary          [string]        the necessary dictionary intepretation of a building
         *                                          such as "ExtBEC", "BEC", or "FrmBEC"
         *                                    
         *      srcRoom             [int]           the room from which the user is starting
         *      
         *      destRoom            [int]           the room the user is trying to reach
         *      
         *      gs                  [IntPtr]        an int pointer used to point to the started python file
         *      
         * @author Anthony Shaidaee
         * @since 3/26/2021
         */
        void PythonPath(string templatePath, string newName, string dictionary, int srcRoom, int destRoom, IntPtr gs)
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
            dynamic mod = Py.Import("path_finding");
            try
            {
                // path, building, start=1615, dest=1615
                mod.main(host.ContentRootFileProvider.GetFileInfo(templatePath).PhysicalPath, newName, dictionary, srcRoom, destRoom);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            PythonEngine.ReleaseLock(gs);
        }
    }
}
