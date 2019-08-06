using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;

namespace JDE_Integration_HDL
{
    public class SFTPImportExport
    {
        public string ServerType = string.Empty;
        public string ServerIP = string.Empty;
        public string UserName = string.Empty;
        public string Password = string.Empty;
        public string PortNo = string.Empty;
        public string RemoteFilePath = string.Empty;
        public string DestPath = string.Empty;
        public string Name_Con = string.Empty;
        public string InputFile = string.Empty;
        private StringBuilder sb = new StringBuilder();

        public string GettingFilesFromOracleSFTP(string SFTPDetails, string strInputFile)
        {
            try
            {
                StreamReader streamReader = new StreamReader((Stream)System.IO.File.OpenRead(SFTPDetails));
                string[] strArray = streamReader.ReadLine().Split('|');
                InputFile = strInputFile;
                while (!streamReader.EndOfStream)
                {
                    string str = streamReader.ReadLine();
                    if (str.Split('|').Length == strArray.Length)
                    {
                        strArray = str.Split('|');
                        for (int index = 0; index < strArray.Length; ++index)
                        {
                            ServerIP = strArray[0].ToString();
                            ServerType = strArray[1].ToString();
                            PortNo = strArray[2].ToString();
                            UserName = strArray[3].ToString();
                            Password = strArray[4].ToString();
                            Name_Con = strArray[5].ToString();
                            RemoteFilePath = strArray[6].ToString();
                            DestPath = strArray[7].ToString();
                        }
                    }
                    if (ServerType == "SFTP")
                        GettingFileFrom_SFTP_Server();
                    else if (ServerType == "FTP")
                        GettingFileFrom_FTP_Server();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sb.ToString();
        }

        private void GettingFileFrom_SFTP_Server()
        {
            try
            {
                int num = 0;
                string str = string.Empty;
                Console.WriteLine("............Downloading Files from SFTP............" + Environment.NewLine + Environment.NewLine);
                Console.WriteLine("SFTP Connection initiated successfully." + Environment.NewLine);
                this.sb.Append("............Downloading Files from SFTP............" + Environment.NewLine + Environment.NewLine);
                this.sb.Append("SFTP Connection initiated successfully." + Environment.NewLine);
                Sftp sftp = new Sftp(this.ServerIP, this.UserName, this.Password);
                if (this.PortNo != "")
                {
                    sftp.Connect(int.Parse(this.PortNo));
                }
                else
                {
                    sftp.Connect();
                }
                Console.WriteLine("SFTP Connection Opened successfully." + Environment.NewLine);
                this.sb.Append("SFTP Connection Opened successfully." + Environment.NewLine);
                foreach (string str2 in sftp.GetFileList(this.RemoteFilePath))
                {
                    int index = 0;
                    while (true)
                    {
                        char[] separator = new char[] { ',' };
                        if (index >= this.Name_Con.Split(separator).Length)
                        {
                            break;
                        }
                        char[] chArray = new char[] { ',' };
                        if (str2.ToString().StartsWith(this.Name_Con.Split(chArray).GetValue(index).ToString()))
                        {
                            sftp.Get(this.RemoteFilePath + "/" + str2, this.DestPath);
                            Console.WriteLine("The file '" + str2.ToString() + "' has been successfully downloaded in '" + this.DestPath + "'" + Environment.NewLine);
                            this.sb.Append("The file '" + str2.ToString() + "' has been successfully downloaded in '" + this.DestPath + "'" + Environment.NewLine);
                            num++;
                            if (this.DestPath.TrimEnd(new char[] { '\\' }) == this.InputFile.TrimEnd(new char[] { '\\' }))
                            {
                                if (str != "")
                                {
                                    str = str + ",";
                                }
                                str = str + this.RemoteFilePath + "/" + str2;
                            }
                        }
                        index++;
                    }
                }
                if (str != "")
                {
                    int index = 0;
                    while (true)
                    {
                        char[] separator = new char[] { ',' };
                        if (index >= str.Split(separator).Length)
                        {
                            break;
                        }
                        MethodInfo getMethod = sftp.GetType().GetProperty("SftpChannel", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
                        object obj2 = getMethod.Invoke(sftp, null);
                        char[] chArray5 = new char[] { ',' };
                        ((ChannelSftp)obj2).rm((String)str.Split(chArray5).GetValue(index).ToString());
                        index++;
                    }
                }
            }
            catch (Exception exception1)
            {
                throw exception1;
            }
        }

        private void GettingFileFrom_FTP_Server()
        {
            try
            {
                foreach (string ftpFile in GetFTPFileList())
                {
                    if (ftpFile.ToString().Contains(Name_Con))
                        FTPFilesDownload(ftpFile);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string[] GetFTPFileList()
        {
            string[] strArray2;
            string str = "";
            StringBuilder builder = new StringBuilder();
            WebResponse response = null;
            StreamReader reader = null;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + this.ServerIP + str));
                request.UseBinary = true;
                request.Credentials = new NetworkCredential(this.UserName, this.Password);
                request.Method = "NLST";
                request.Proxy = null;
                request.KeepAlive = false;
                request.UsePassive = false;
                reader = new StreamReader(request.GetResponse().GetResponseStream());
                string str2 = reader.ReadLine();
                while (true)
                {
                    if (str2 == null)
                    {
                        builder.Remove(builder.ToString().LastIndexOf('\n'), 1);
                        strArray2 = builder.ToString().Split(new char[] { '\n' });
                        break;
                    }
                    builder.Append(str2);
                    builder.Append("\n");
                    str2 = reader.ReadLine();
                }
            }
            catch (Exception)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                strArray2 = null;
            }
            return strArray2;
        }

        private void FTPFilesDownload(string file)
        {
            string str = "";
            try
            {
                if (new Uri("ftp://" + ServerIP + "/" + file).Scheme != Uri.UriSchemeFtp)
                    return;
                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + ServerIP + str + "/" + file));
                ftpWebRequest.Credentials = (ICredentials)new NetworkCredential(UserName, Password);
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.Method = "RETR";
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Proxy = (IWebProxy)null;
                ftpWebRequest.UsePassive = false;
                FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse();
                Stream responseStream = response.GetResponseStream();
                FileStream fileStream = new FileStream(DestPath + file, FileMode.Create);
                int count1 = 2048;
                byte[] buffer = new byte[count1];
                for (int count2 = responseStream.Read(buffer, 0, count1); count2 > 0; count2 = responseStream.Read(buffer, 0, count1))
                    fileStream.Write(buffer, 0, count2);
                Console.WriteLine(file + " has been downloaded from FTP to '" + DestPath + "'" + Environment.NewLine);
                sb.Append(file + " has been downloaded from FTP to '" + DestPath + "'" + Environment.NewLine);
                fileStream.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
