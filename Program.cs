using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IomDatToCsv
{
    class Program
    {
        private const string NewFileFormatTag = "Grapher v1.0 Quarter Sec Linear";
        private const string OldFileFormatTag = "Healing Rhythms Event Data";

        static void Main(string[] args)
        {
            try
            {
                if (!args.Any())
                    DisplayHelp();

                var sourceFolder = args[0];
                var targetFolder = args[1];

                var sourceFiles = Directory.GetFiles(sourceFolder, "*.dat");

                ConvertFiles(sourceFiles, targetFolder);

                Exit();
            }
            catch (Exception ex)
            {
                DisplayFatal(ex);
            }
        }

        private static void ConvertFiles(string[] sourceFiles, string targetFolder)
        {
            Console.WriteLine("Converting {0} files.", sourceFiles.Count());

            foreach (var sourceFile in sourceFiles)
                ConvertFile(sourceFile, targetFolder);

            Console.WriteLine("File conversion is complete.");
        }

        private static void ConvertFile(string sourceFile, string targetFolder)
        {
            try
            {
                Console.WriteLine("Converting {0}", Path.GetFileName(sourceFile));

                var records = ReadSourceFile(sourceFile);

                AddDateTimeToRecords(sourceFile, records);

                WriteTargetFile(targetFolder, records);

                Console.WriteLine("File has been converted.");
            }
            catch (Exception ex)
            {
                DisplayError(ex);
            }
        }

        private static List<Record> ReadSourceFile(string sourceFile)
        {
            var sourceText = File.ReadAllText(sourceFile);

            if (!sourceText.StartsWith(NewFileFormatTag)
                && !sourceText.StartsWith(OldFileFormatTag))
                throw new InvalidFileFormatException();

            var isOldFormat = sourceText.StartsWith(OldFileFormatTag);

            var startIndex = sourceText.IndexOf("[[");
            var endIndex = sourceText.LastIndexOf("]]");

            var sourceData = sourceText.Substring(startIndex + 2, endIndex - startIndex - 2);

            var sourceRows = sourceData.Split(new[] {"], ["}, StringSplitOptions.None);

            var records = new List<Record>();

            foreach (var sourceRow in sourceRows)
                ProcessRow(sourceRow, records, isOldFormat);

            return records;
        }

        private static void AddDateTimeToRecords(string sourceFile, List<Record> records)
        {
            var sourceDateTime = File.GetLastWriteTime(sourceFile);
            var lastTimeStamp = records.Last().TimeStamp;

            foreach (var record in records)
                record.DateTime = sourceDateTime.AddMilliseconds(record.TimeStamp - lastTimeStamp);
        }

        private static void ProcessRow(string sourceRow, List<Record> records, bool isOldFileFormat)
        {
            var sourceFields = sourceRow.Split(new[] {", "}, StringSplitOptions.None);

            var record = isOldFileFormat
                ? CreateRecordFromOldFileFormat(sourceFields)
                : CreateRecordFromNewFileFormat(sourceFields);

            records.Add(record);
        }

        private static Record CreateRecordFromNewFileFormat(string[] sourceFields)
        {
            var record = new Record();

            record.TimeStamp = int.Parse(sourceFields[0].Replace("#TS: ", ""));
            record.ElectroDermalResponse = int.Parse(sourceFields[1].Replace("#EDR: ", ""));
            record.Detect = int.Parse(sourceFields[2].Replace("#DETECT: ", ""));
            record.HeartRate = decimal.Parse(sourceFields[3].Replace("#rate: ", ""));
            record.Label = sourceFields[4].Replace("#Lable: ", "");
            record.Coherence = int.Parse(sourceFields[5].Replace("#coh: ", ""));

            return record;
        }

        private static Record CreateRecordFromOldFileFormat(string[] sourceFields)
        {
            var record = new Record();

            record.TimeStamp = int.Parse(sourceFields[0].Trim());
            record.ElectroDermalResponse = int.Parse(sourceFields[1].Trim());
            record.Detect = int.Parse(sourceFields[2].Trim());
            record.HeartRate = decimal.Parse(sourceFields[3].Trim());
            record.Label = sourceFields[4].Trim();
            record.Coherence = 0;

            return record;
        }

        private static void WriteTargetFile(string targetFolder, List<Record> records)
        {
            var targetText = new StringBuilder();

            WriteHeaders(targetText);

            foreach (var record in records)
                WriteRow(targetText, record);

            var fileDateTime = records.First().DateTime;

            var targetFileName = fileDateTime.ToString("yyyyMMdd-HHmmss") + ".csv";
            
            var targetFilePath = targetFolder + "\\" + targetFileName;
            
            File.WriteAllText(targetFilePath, targetText.ToString());
        }

        private static void WriteHeaders(StringBuilder targetText)
        {
            targetText.Append("DateTime,")
                .Append("Time Stamp,")
                .Append("ElectroDermal Response,")
                .Append("Detect,")
                .Append("Heart Rate,")
                .Append("Label,")
                .Append("Coherence")
                .AppendLine();
        }

        private static void WriteRow(StringBuilder targetText, Record record)
        {
            targetText.Append(record.DateTime.ToString("O") + ",");
            targetText.Append(record.TimeStamp + ",");
            targetText.Append(record.ElectroDermalResponse + ",");
            targetText.Append(record.Detect + ",");
            targetText.Append(record.HeartRate + ",");
            targetText.Append(record.Label + ",");
            targetText.Append(record.Coherence);
            targetText.AppendLine();
        }

        private static void DisplayHelp()
        {
            var help = new StringBuilder()
                .AppendLine("Converts IOM .dat files to .csv files")
                .AppendLine("usage: IomDatToCsv.exe source-folder target-folder")
                .AppendLine()
                .AppendLine("The parameters are:")
                .AppendLine("  source-folder - the folder containing the IOM .dat files")
                .AppendLine("  target-folder - the folder to write the .csv files to")
                .AppendLine()
                .AppendLine("example: IomDatToCsv.exe \"C:\\Source\" \"C:\\Target\"");

            Console.WriteLine(help.ToString());

            Exit();
        }

        private static void DisplayError(Exception exception)
        {
            Console.WriteLine("ERROR: {0}", exception.Message);
        }

        private static void DisplayFatal(Exception exception)
        {
            Console.WriteLine("FATAL: {0}", exception.Message);

            Exit();
        }

        private static void Exit()
        {
            Console.WriteLine();

            Console.WriteLine("Press any key to exit.");

            Console.ReadKey();

            Environment.Exit(0);
        }
    }
}
