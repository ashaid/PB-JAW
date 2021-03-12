


namespace PB_JAW.Controllers
{
    public class DatabaseController : Controller
    {
        private int PFT2BEC; //static time from PFT to BEC
        private int PFT2Loc; //static time from PFT to Lockett
        private int BEC2Loc; //static time from BEC to Lockett

        private destinationRoom(string roomNum, string building, string time)
        {
            this.roomNum = roomNum;
            this.building = building;
            this.time2dxit = time;
        }

        private sourceRoom(string roomNum, string building, string time)
        {
            this.roomNum = roomNum;
            this.building = building;
            this.time2exit = time;
        }


        //queries

        public string destinationInfo() {
            //receive information from user
            //query information for destination
            if (//destination building is BEC)
        {
                //query room number information from BEC table
            }
            else if (//destination building is PFT)
        {
                //query room number information from PFT table
            }
            else if (//destination building is Lockett)
        {
                //query room number information from Loc table
            }
            else (){
                //throw wrong building exception
            }
        }

        public string sourceInfo() {
    //receive information from user
    //query information for source
    if (//source building is BEC)
                {
        //query room number information from BEC table
    }
    else if (//source building is PFT)
                {
        //query room number information from PFT table
    }
    else if (//source building is Lockett)
                {
        //query room number information from Loc table
    }
    else (){
        //throw wrong building exception
    }
}

        public string timeCalulator(destinationRoom a, sourceRoom b) //we probably only need half of these since it only calculates times
                                    //(i included cases for every possibly source and destination, but we shouldn't need to worry about it for this method
        {
            int totalTime;

            if(a.building == "BEC" && b.building == "PFT") //from BEC to PFT
            {
                totalTime = a.time2exit + b.time2exit + PFT2BEC;
                return totalTime;
            }
            else if (a.building == "BEC" && b.building == "Loc") //from BEC to Lockett
            {
                totalTime = a.time2exit + b.time2exit + BEC2Loc;
                return totalTime;
            }
            else if (a.building == "BEC" && b.building == "BEC") //from BEC to BEC
            {
                totalTime = 0; //within the same building. Throw catch for output saying "in the same building. ETA < 3 minutes or something
                return totalTime;
            }
            else if(a.building == "PFT" && b.building == "PFT") //from PFT to PFT
            {
                totalTime = 0;
                return totalTime;
            }
            else if (a.building == "PFT" && b.building == "BEC") //from PFT to BEC
            {
                totalTime = a.time2exit + b.time2exit + PFT2BEC;
                return totalTime;
            }
            else if (a.building == "PFT" && b.building == "Loc") //from PFT to Lockett
            {
                totalTime = a.time2exit + b.time2exit + PFT2Loc;
                return totalTime;
            }
            else if (a.building == "Loc" && b.building == "Loc") //from Lockett to Lockett
            {
                totalTime = 0;
                return totalTime;
            }
            else if (a.building == "Loc" && b.building == "BEC") //Lockett to BEC
            {
                totalTime = a.time2exit + b.time2exit + BEC2Loc;
                return totalTime;
            }
            else if (a.building == "Loc" && b.building == "PFT") //Lockett to PFT
            {
                totalTime = a.time2exit + b.time2exit + PFT2Loc;
                return totalTime;
            }
            else if (a.building == null  && b.building == "BEC") //from Off campus to BEC
            {
                totalTime = a.time2exit + b.time2exit + PFT2BEC;
                return totalTime;
            }
            else if (a.building == null && b.building == "PFT") // from Off campus to PFT
            {
                totalTime = a.time2exit + b.time2exit + PFT2BEC;
                return totalTime;
            }
            else if (a.building == null && b.building == "Loc") // from Off campus to Lockett
            {
                totalTime = a.time2exit + b.time2exit + PFT2BEC;
                return totalTime;
            }
            else
            {
                //thow error
            }


        }

    }
}