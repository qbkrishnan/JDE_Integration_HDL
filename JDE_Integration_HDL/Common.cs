using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;

namespace JDE_Integration_HDL
{

    public class Common
    {
        private DataTable dtInputData = new DataTable();
        private DataTable dtPersonData = new DataTable();
        private DataTable dtLookupData = new DataTable();
        private string InputFilePath = string.Empty;
        private string DATFilePath = ConfigurationManager.AppSettings["DATFile"].ToString();
        private string LogFilePath = ConfigurationManager.AppSettings["LogFilePath"].ToString();

        public void GeneratingDATFiles(string InputFile)
        {
            InputFilePath = InputFile;
            dtInputData = ImportDataIntoDataTable(InputFile, "|", 0);
            CreatingDATFiles();
        }


        private DataTable ImportDataIntoDataTable(
          string FilePath,
          string Delimiter,
          int iHeaderLine)
        {
            DataTable dataTable = new DataTable();
            try
            {
                string[] strArray1 = File.ReadAllLines(FilePath);
                string[] strArray2 = strArray1[iHeaderLine].Split(Delimiter.ToCharArray());
                int length = strArray2.GetLength(0);
                for (int index = 0; index < length; ++index)
                    dataTable.Columns.Add(strArray2[index].ToLower(), typeof(string));
                for (int index1 = iHeaderLine + 1; index1 < strArray1.GetLength(0); ++index1)
                {
                    if (strArray1[index1] != "")
                    {
                        string[] strArray3 = strArray1[index1].Split(Delimiter.ToCharArray());
                        DataRow row = dataTable.NewRow();
                        for (int index2 = 0; index2 < length; ++index2)
                            row[index2] = (object)strArray3[index2];
                        dataTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return dataTable;
        }

        public void CreatingDATFiles()
        {
            RemoveOldFiles(DATFilePath);
            Generate_Location_Details(DATFilePath, "Location", InputFilePath);
        }

        public string GenerateDetails(string DetailsFor, string InputFilePath)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("METADATA|Location|FLEX:PER_LOCATIONS_DF|yardiCode(PER_LOCATIONS_DF=Global Data Elements)|LocationCode|SetCode|EffectiveStartDate|EffectiveEndDate|ActiveStatus|LocationName|Description|AddressLine1|AddressLine2");
            stringBuilder.AppendLine("|AddressLine3|AddressLine4|TownOrCity|Region1|Region2|Region3|Country|PostalCode");
            foreach (DataRow row in (InternalDataCollectionBase)dtInputData.Rows)
            {
                stringBuilder.Append("MERGE|Location|");
                stringBuilder.Append("Global Data Elements|");
                if (row["LOCATION_NAME"].ToString().IndexOf('(') != -1)
                    stringBuilder.Append(row["LOCATION_NAME"].ToString().Split('(').GetValue(1).ToString().Replace(")", "").ToString() + "|");
                else
                    stringBuilder.Append("|");
                stringBuilder.Append(row["LOCATION_ID"].ToString() + "|");
                stringBuilder.Append("COMMON|");
                stringBuilder.Append(row["EFFECTIVE_START_DATE"].ToString().Replace("-", "/") + "|");
                stringBuilder.Append(row["EFFECTIVE_END_DATE"].ToString().Replace("-", "/") + "|");
                stringBuilder.Append(row["ACTIVE_STATUS"].ToString() + "|");
                stringBuilder.Append(row["LOCATION_NAME"].ToString() + "|");
                stringBuilder.Append(row["DESCRIPTION"].ToString() + "|");
                stringBuilder.Append(row["ADDRESS_LINE_1"].ToString() + "|");
                stringBuilder.Append(row["ADDRESS_LINE_2"].ToString() + "|");
                stringBuilder.Append(row["ADDRESS_LINE_3"].ToString() + "|");
                stringBuilder.Append(row["ADDRESS_LINE_4"].ToString() + "|");
                stringBuilder.Append(row["TOWN_OR_CITY"].ToString() + "|");
                stringBuilder.Append(row["REGION_1"].ToString() + "|");
                stringBuilder.Append(row["REGION_2"].ToString() + "|");
                stringBuilder.Append(row["REGION_3"].ToString() + "|");
                stringBuilder.Append(row["COUNTRY"].ToString() + "|");
                stringBuilder.AppendLine(row["POSTAL_CODE"].ToString());
            }
            return stringBuilder.ToString();
        }

        public void Generate_Location_Details(
          string FilePath,
          string DATFileName,
          string InputFilePath)
        {
            Directory.CreateDirectory(FilePath);
            FileStream fileStream = new FileStream(FilePath + "Location.dat", FileMode.CreateNew, FileAccess.ReadWrite);
            StreamWriter streamWriter = new StreamWriter((Stream)fileStream);
            streamWriter.Write(GenerateDetails(DATFileName, InputFilePath));
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }

        public void RemoveOldFiles(string strFilePath)
        {
            foreach (FileInfo file in new DirectoryInfo(strFilePath).GetFiles())
            {
                if (File.Exists(strFilePath + (object)file))
                    File.Delete(strFilePath + (object)file);
            }
        }

        public string ArchiveOldFiles()
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                string path = ConfigurationManager.AppSettings["ArchiveInputFile"] + "\\Archive_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                foreach (string file in Directory.GetFiles(ConfigurationManager.AppSettings["InputFile"]))
                {
                    string str = file.Split('\\').GetValue(file.Split('\\').Length - 1).ToString();
                    File.Move(file, path + "\\" + str);
                    stringBuilder.Append("File has been successfully moved");
                }
            }
            catch (Exception ex)
            {
                stringBuilder.Append(ex.Message.ToString());
            }
            return stringBuilder.ToString();
        }

        public void GeneratingLOGFile(string Message)
        {
            using (FileStream fileStream = new FileStream(LogFilePath + "DSIntegrationLogFile" + DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("HHmmssfff") + ".txt", FileMode.OpenOrCreate))
            {
                using (StreamWriter streamWriter = new StreamWriter((Stream)fileStream))
                {
                    streamWriter.BaseStream.Seek(0L, SeekOrigin.End);
                    streamWriter.Write(Message);
                    streamWriter.WriteLine();
                    streamWriter.Flush();
                }
            }
        }
    }
}
