﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DMC.Siemens.Common.PLC.Types;

namespace DMC.Siemens.Common.PLC
{
    public class DataEntry
    {

        public string Name { get; set; }
        public DataType DataType { get; set; }
        public string DataTypeName { get; set; }
        public Constant<int> ArrayStartIndex { get; set; }
        public Constant<int> ArrayEndIndex { get; set; }
        public Constant<int> StringLength { get; set; }
        public string Comment { get; set; }
        public LinkedList<DataEntry> Children { get; set; } = new LinkedList<DataEntry>();

        public static DataEntry FromString(string dataEntry, TextReader dataReader)
        {

            DataEntry newEntry = new DataEntry();

            string trimmedData = dataEntry.Trim();
            string type = "";
            string[] splitString;
            int length = 0;
            int arrayStart = 0;
            bool isUdt = false;

            if (trimmedData.Contains("//"))
            {
                splitString = trimmedData.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                newEntry.Comment = splitString[1].Trim();
                trimmedData = splitString[0];
            }
            if (trimmedData.Contains(":"))
            {
                splitString = trimmedData.Split(':');
                type = splitString[1].Trim().Trim(';');
                newEntry.Name = splitString[0].Trim();

                // Check to see if there is a UDT first
                if (type.Contains("\""))
                {
                    Match match = Regex.Match(type, "\"([^\"]*)\"");
                    // Use Regex to grab the value between the quotes
                    if (match.Success && match.Groups != null && match.Groups.Count > 1)
                    {
                        newEntry.DataTypeName = match.Groups[1].Value;
                        isUdt = true;
                    }
                    
                }
                if (type.ToUpper().Contains("ARRAY["))
                {
                    newEntry.Name = newEntry.Name.Trim('\"');
                    splitString = type.Split(new string[] { "[", "..", "]" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length == 4)
                    {
                        if (int.TryParse(splitString[2], out length)) // See if the array end index is an integer
                        {
                            newEntry.ArrayEndIndex = new Constant<int>(length);
                        }
                        else // If not, the array index is a constant defined elsewhere
                        {
                            newEntry.ArrayEndIndex = new Constant<int>(splitString[2].Trim('\"'));
                        }
                        if (int.TryParse(splitString[1], out arrayStart)) // Do the same as above for the start index
                        {
                            newEntry.ArrayStartIndex = new Constant<int>(arrayStart);
                        }
                        else
                        {
                            newEntry.ArrayStartIndex = new Constant<int>(splitString[1].Trim('\"'));
                        }
                        
                    }
                    splitString = type.ToUpper().Split(new string[] { " OF " }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length > 1 && !isUdt)
                    {
                        newEntry.DataTypeName = splitString[1].Trim();
                    }

                    type = "ARRAY";

                }
                else if (type.ToUpper().Contains("STRING"))
                {
                    splitString = type.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length > 1)
                    {
                        if (int.TryParse(splitString[1], out length)) // Check to make sure the string length is an integer
                        {
                            newEntry.StringLength = new Constant<int>(length);
                        }
                        else // If not, it's a constant referenced elsewhere
                        {
                            newEntry.StringLength = new Constant<int>(splitString[1].Trim('\"'));
                        }

                    }
                    else
                    {
                        newEntry.StringLength = new Constant<int>(254);
                        newEntry.Name = newEntry.Name.Trim('\"');
                    }

                    type = "STRING";

                }
                else if (type.ToUpper().Contains("STRUCT"))
                {
                    string line;
                    while ((line = dataReader.ReadLine()) != null && !line.Contains("END_STRUCT"))
                    {
                        newEntry.Children.AddLast(DataEntry.FromString(line, dataReader));
                    }
                }

            }

            DataType t;
            if (isUdt)
            {
                t = DataType.UDT;
            }
            else
            {
                if (!Enum.TryParse<DataType>(type, true, out t))
                {
                    // Invalid type detected
                }
            }

            newEntry.DataType = t;

            return newEntry;

        }
    }
}
