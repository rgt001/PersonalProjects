using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils.FTPHandler
{
    public static class FtpHandler
    {
        private static FtpWebRequest CreateRequest(string path)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(NormalizeFTP(path)));
            string[] credentials = GetCredentials(path);
            request.Credentials = new NetworkCredential(credentials[0], credentials[1]);
            return request;
        }

        public static bool FileExists(string path)
        {
            var request = CreateRequest(path);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false;
                }
            }
            return false;
        }

        public static void UploadFile(string source, string destination, string user, string password)
        {
            try
            {
                destination = NormalizeFTP(destination);
                FileInfo arquivoInfo = new FileInfo(source);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(destination));

                CheckPath(destination, user, password);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(user, password);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;
                request.ContentLength = arquivoInfo.Length;

                using (FileStream fs = arquivoInfo.OpenRead())
                {
                    byte[] buffer = new byte[2048];
                    int bytesSent = 0;
                    int bytes = 0;

                    using (Stream stream = request.GetRequestStream())
                    {
                        while (bytesSent < arquivoInfo.Length)
                        {
                            bytes = fs.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, bytes);
                            bytesSent += bytes;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                String status = ((FtpWebResponse)e.Response).StatusDescription;
                throw e;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void CheckPath(string destination, string user, string password)
        {
            var teste = new Uri(destination);
            var startPath = teste.AbsoluteUri.Replace(teste.AbsolutePath, "");
            var absolutePath = teste.AbsolutePath.Split('/');
            for (int i = 1; i < absolutePath.Length; i++)
            {
                try
                {
                    string path = absolutePath[i];
                    if (i == absolutePath.Length - 1 && path.OccurrencesCount('.') == 1) return;

                    startPath += "/" + path;
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(startPath);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    request.UseBinary = true;
                    request.Credentials = new NetworkCredential(user, password);
                    ((FtpWebResponse)request.GetResponse()).GetResponseStream();
                }
                catch (Exception) { }
            }
        }

        public static void DownloadFile(string source, string destination, string user, string password)
        {
            try
            {
                source = NormalizeFTP(source);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(source));
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(user, password);
                request.UseBinary = true;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream rs = response.GetResponseStream())
                    {
                        using (FileStream ws = new FileStream(destination, FileMode.Create))
                        {
                            byte[] buffer = new byte[2048];
                            int bytesRead = rs.Read(buffer, 0, buffer.Length);

                            while (bytesRead > 0)
                            {
                                ws.Write(buffer, 0, bytesRead);
                                bytesRead = rs.Read(buffer, 0, buffer.Length);
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public static void DownloadFile(string source, string destination)
        {
            var credentials = GetCredentials(source);
            string user = credentials[0];
            string password = credentials[1];

            DownloadFile(source, destination, user, password);
        }

        public static void UploadFile(string source, string destination)
        {
            var credentials = GetCredentials(destination);
            string user = credentials[0];
            string password = credentials[1];

            UploadFile(source, destination, user, password);
        }

        private static string[] GetCredentials(string url)
        {
            url = url.Replace("ftp://", "");
            url = url.Substring(0, url.LastIndexOf('@'));
            return url.Split(':');
        }

        private static string NormalizeFTP(string url)
        {
            var credentials = GetCredentials(url);
            return url.Replace(credentials[0] + ":" + credentials[1] + "@", "");
        }
    }
}
