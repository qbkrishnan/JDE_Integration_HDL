using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace JDE_Integration_HDL
{
    class Program
    {
        private static string UCMContentID = string.Empty;
        private static string SFTPDetails = ConfigurationManager.AppSettings["SFTPDetails"].ToString();
        private static string InputFile = ConfigurationManager.AppSettings["InputFile"].ToString();
        private static SFTPImportExport objSFTP = new SFTPImportExport();
        private static FusionDataLoader objDataLoader = new FusionDataLoader();
        private static string FileName = string.Empty;
        private static StringBuilder sbMsg = new StringBuilder();
        private static Common objCommon = new Common();

        static void Main(string[] args)
        {
            sbMsg.Append(objSFTP.GettingFilesFromOracleSFTP(SFTPDetails, InputFile));
            string[] files = Directory.GetFiles(InputFile);
            int num = 0;
            foreach (string str in files)
            {
                InputFile.Split('\\').GetValue(InputFile.Split('\\').Length - 1).ToString();
                ++num;
                objCommon.GeneratingDATFiles(str);
                Console.WriteLine("DAT files created successfully." + Environment.NewLine);
                sbMsg.AppendLine("DAT files created successfully." + Environment.NewLine);
                FileName = ZipFileCreator.CreateZipFile();
                Console.WriteLine("Zip File '" + FileName + "' has been created" + Environment.NewLine);
                sbMsg.AppendLine("Zip File '" + FileName + "' has been created" + Environment.NewLine);

                // commented for testing purpose

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                UCMContentID = objDataLoader.GetUCMContentID(FileName);
                Console.WriteLine("The UCM ContentID '" + UCMContentID + "' has been successfully generated" + Environment.NewLine + Environment.NewLine);
                sbMsg.AppendLine("The UCM ContentID '" + UCMContentID + "' has been successfully generated" + Environment.NewLine + Environment.NewLine);
                fnExecutingUCM(UCMContentID);
                sbMsg.AppendLine("Data succesfully loaded to fusion.Thank you." + Environment.NewLine);
                sbMsg.AppendLine("***************************" + Environment.NewLine + Environment.NewLine);
                sbMsg.AppendLine(objCommon.ArchiveOldFiles());

            }
            if (num == 0)
            {
                Console.WriteLine("No Input File(s) found." + Environment.NewLine + Environment.NewLine);
                sbMsg.AppendLine("No Input File(s) found." + Environment.NewLine + Environment.NewLine);
            }
            objCommon.GeneratingLOGFile(sbMsg.ToString());
            Console.WriteLine("Data succesfully loaded to fusion.Thank you." + Environment.NewLine);
            Console.WriteLine("***************************" + Environment.NewLine + Environment.NewLine);
            Environment.Exit(0);
        }

        public static void fnExecutingUCM(string FusionUCMContentID)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><ns1:importAndLoadDataAsync xmlns:ns1=\"http://xmlns.oracle.com/apps/hcm/common/dataLoader/core/dataLoaderIntegrationService/types/\"><ns1:ContentId>" + FusionUCMContentID + "</ns1:ContentId><ns1:Parameters></ns1:Parameters></ns1:importAndLoadDataAsync></soap:Body></soap:Envelope>");
            string base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["UCMUserName"] + ":" + ConfigurationManager.AppSettings["UCMPassword"]));
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["LoaderServiceURL"]);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "text/xml;charset=UTF-8";
            httpWebRequest.ContentLength = (long)bytes.Length;
            httpWebRequest.Headers.Add("Authorization", "Basic " + base64String);
            httpWebRequest.Headers.Add("SOAPAction", "http://xmlns.oracle.com/apps/hcm/common/batchLoader/core/loaderIntegrationService/submitBatch");
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            XDocument xdocument;
            using (WebResponse response = httpWebRequest.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                    xdocument = XDocument.Load(responseStream);
            }
            StreamWriter streamWriter = new StreamWriter(ConfigurationManager.AppSettings["FileRepository"] + "RESP_" + FusionUCMContentID + ".txt");
            streamWriter.WriteLine((object)xdocument);
            streamWriter.Close();
        }
    }
}
