using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogStandardizer
{
    public static class Standardizer
    {
        // Mapper for levels
        private static readonly Dictionary<string, string> LevelMapper = new Dictionary<string, string>()
        {
            {"INFORMATION", "INFO"},
            {"INFO", "INFO"},
            {"WARNING", "WARN"},
            {"WARN", "WARN"},
            {"DEBUG", "DEBUG"},
            {"ERROR", "ERROR"}
        };

        public static void GetStandardizer(string InputFile_Path, string OutputFile_Path)
        {
            
            string ProblemsFile_Path = "problems.txt";

            using (var inputStream = new FileStream(InputFile_Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) 
            using (var writer = new StreamWriter(OutputFile_Path))
            using (var reader = new StreamReader(inputStream)) 
            using (var problemWriter = new StreamWriter(ProblemsFile_Path))
            {
                string? line;
                while((line = reader.ReadLine())!=null)
                {
                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // check if the line doesn't contain '|', means it is the second case
                    if (!line.Contains("|"))
                    {
                        #region First case

                        // [0] date in dd.MM.yyyy
                        // [1] time
                        // [2] level
                        // [3] message

                        try
                        {
                            
                            var PartsOfLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            
                            string datePart = PartsOfLine[0].Trim(); 
                            string time_out = PartsOfLine[1].Trim();
                            string levelPart = PartsOfLine[2].Trim();
                            string message_out = string.Join(" ", PartsOfLine,3, PartsOfLine.Length-3).Trim();

                            // Parse date
                            if (!DateTime.TryParseExact(datePart, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                throw new FormatException("Invalid date value in Format 1");
                            }
                            string date_out = date.ToString("yyyy-MM-dd");

                            if (!LevelMapper.TryGetValue(levelPart, out string level_out))
                            {
                                level_out = levelPart;
                            }

                            // For Format 1, caller is missing so we assign "DEFAULT".
                            string caller_out = "DEFAULT";

                            string outputLine = $"{date_out}\t{time_out}\t{level_out}\t{caller_out}\t{message_out}";
                            writer.WriteLine(outputLine);
                        }
                        catch (Exception)
                        {
                            problemWriter.WriteLine(line);
                        }

                        #endregion
                    }
                    else
                    {
                        #region Second case

                        // [0] "2025-03-10 15:14:51.5882"
                        // [1] " INFO" _To_ level
                        // [2] "11" _To_ ignored, Not required
                        // [3] "MobileComputer.GetDeviceId" _To_ caller
                        // [4] "Код устройства: '@MINDEO-M40-D-410244015546'" _To_ message

                        try
                        {
                            var PartsOfLine = line.Split('|');

                            // Parse date and time
                            string dateTimePart = PartsOfLine[0].Trim(); // to remove any extra spaces or unwanted newline 
                            // We expect date and time separated by a space.
                            var dateTimePart_Separated = dateTimePart.Split(' ');

                            string date_part = dateTimePart_Separated[0].Trim(); // expected "yyyy-MM-dd"
                            string time_part = dateTimePart_Separated[1].Trim(); 

                            // Validate and reformat date.
                            if (!DateTime.TryParseExact(date_part, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                throw new FormatException("Invalid date value in Format 2");
                            }
                            string date_Out = date.ToString("yyyy-MM-dd");

                            string levelIn = PartsOfLine[1].Trim();
                            if (!LevelMapper.TryGetValue(levelIn, out string levelOut))
                            {
                                levelOut = levelIn;
                            }

                            // [3] "MobileComputer.GetDeviceId" _To_ caller
                            string caller = PartsOfLine[3].Trim();

                            // [4] " Код устройства: '@MINDEO-M40-D-410244015546'" _To_ message
                            string message = PartsOfLine[4].Trim();

                            // Build the output
                            string outputLine = $"{date_Out}\t{time_part}\t{levelOut}\t{caller}\t{message}";
                            writer.WriteLine(outputLine);
                        }
                        catch (Exception ex)
                        {
                            problemWriter.WriteLine(line);
                        }

                        #endregion


                    }

                }
            }

            Console.WriteLine("Processing completed.");
        }
    }
}
