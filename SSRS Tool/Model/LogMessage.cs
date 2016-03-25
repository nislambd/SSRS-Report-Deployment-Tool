using System;

namespace SSRSDeployTool.Model
{

    public class LogMessage
    {
        public string Message { get; set; }

        public DateTime EventDate { get; set; }

        public string Type { get; set; }

        public string Color
        {
            get
            {
                if (Type == "Error") return "Red";

                return "Black";
            }

        }
    }
}