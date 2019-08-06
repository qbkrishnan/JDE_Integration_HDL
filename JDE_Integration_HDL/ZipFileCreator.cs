using Ionic.Utils.Zip;
using System;
using System.Configuration;
using System.IO;

namespace JDE_Integration_HDL
{
    public class ZipFileCreator
    {
        private static string FileRepository = ConfigurationManager.AppSettings["FileRepository"];
        private static string TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        public static string DATFilePath = ConfigurationManager.AppSettings["DATFile"].ToString();
        public static string ZipFolderPath = ZipFileCreator.FileRepository + "Auto_JDE_" + ZipFileCreator.TimeStamp + ".zip";

        public static string CreateZipFile()
        {
            using (ZipFile zipFile = new ZipFile(ZipFileCreator.ZipFolderPath))
            {
                foreach (FileInfo file in new DirectoryInfo(ZipFileCreator.DATFilePath).GetFiles())
                    zipFile.AddFile(ZipFileCreator.DATFilePath + file.Name, "");
                zipFile.Save();
            }
            return ZipFileCreator.ZipFolderPath;
        }
    }
}
