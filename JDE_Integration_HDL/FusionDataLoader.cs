using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace JDE_Integration_HDL
{
    public class FusionDataLoader
    {
        private string LoaderType = string.Empty;
        private string fileName = string.Empty;
        private string FileRepository = string.Empty;
        private string TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        private string JARFilePath = string.Empty;
        private string UCMurl = string.Empty;
        private string UCMUserName = string.Empty;
        private string UCMPassword = string.Empty;
        private string JavaExePath = string.Empty;
        private string LoaderServiceURL = string.Empty;
        private string strUCMContentID = string.Empty;
        private string RetryDelay = string.Empty;
        private string NumRetry = string.Empty;
        private int iNumRetry = 1;

        public string GetUCMContentID(string FileName)
        {
            GetEnvironmentDetails();
            GeneratingUCMContentID(FileName);
            return strUCMContentID;
        }

        private void GetEnvironmentDetails()
        {
            UCMurl = ConfigurationManager.AppSettings["UCMurl"];
            UCMUserName = ConfigurationManager.AppSettings["UCMUserName"];
            UCMPassword = ConfigurationManager.AppSettings["UCMPassword"];
            FileRepository = ConfigurationManager.AppSettings["FileRepository"];
            JARFilePath = ConfigurationManager.AppSettings["JARFilePath"];
            LoaderServiceURL = ConfigurationManager.AppSettings["LoaderServiceURL"];
            JavaExePath = ConfigurationManager.AppSettings["JavaExePath"];
            RetryDelay = ConfigurationManager.AppSettings["RetryDelay"];
            NumRetry = ConfigurationManager.AppSettings["NumRetry"];
        }

        private void GeneratingUCMContentID(string FileName)
        {
            fileName = FileRepository + "RESULT_" + TimeStamp + ".txt";
            StreamWriter streamWriter = new StreamWriter(FileRepository + "BATCH_" + TimeStamp + ".bat");
            string str1 = FileName.Split('\\').GetValue(FileName.Split('\\').Length - 1).ToString().Replace(".zip", "");
            string str2 = "java -classpath \"" + JARFilePath + "\" oracle.ucm.client.UploadTool --url=\"" + UCMurl + "\" --username=\"" + UCMUserName + "\" --password=\"" + UCMPassword + "\" --primaryFile=\"" + FileName + "\" --dDocTitle=\"" + str1 + "\" --dSecurityGroup=\"FAFusionImportExport\" --dDocAccount=\"hcm/dataloader/import\" >" + fileName;
            streamWriter.WriteLine(str2);
            streamWriter.Flush();
            streamWriter.Close();
            streamWriter.Dispose();
            Process process = Process.Start(new ProcessStartInfo("java.exe", "-classpath \"" + JARFilePath + "\" oracle.ucm.client.UploadTool --url=\"" + UCMurl + "\" --username=\"" + UCMUserName + "\" --password=\"" + UCMPassword + "\" --primaryFile=\"" + FileName + "\" --dDocTitle=\"" + str1 + "\" --dSecurityGroup=\"FAFusionImportExport\" --dDocAccount=\"hcm/dataloader/import\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
            process.WaitForExit();
            string strOutput = process.StandardOutput.ReadToEnd();
            string strError = process.StandardError.ReadToEnd();
            int intExitCode = process.ExitCode;

            if (intExitCode == 0)
            {

                strUCMContentID = strOutput.Substring(strOutput.IndexOf("UCM"), Convert.ToInt16(ConfigurationManager.AppSettings["UCMContentID_Length"].ToString()));
           }

            if (strOutput.Contains("Form validation failed") & strError.Contains("Form validation failed"))
            {
                Console.WriteLine("Authentication failed..Please check the credentials and reset the password if needed" + Environment.NewLine);
            }

            if (strOutput.Contains("Forbidden") & strError.Contains("Forbidden"))
            {
                Console.WriteLine("Unable to access Fusion environment/Web service..Please check whether the environment is up and running" + Environment.NewLine);
            }
        }
    }
}
