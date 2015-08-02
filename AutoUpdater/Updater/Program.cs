using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.Net;
using Octokit.Internal;
using System.Net.Http;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Configuration;

namespace Updater
{
    internal class Program
    {
        private static  void Main(string[] args)
        {
            AppCode().Wait();
        }

        private static async Task AppCode()
        {
            Console.WriteLine("Checking for update...");
            bool updateDownloaded = await CheckForUpdate();

            if (updateDownloaded)
            {
                //Apply the update
                using (FileStream zipToOpen = new FileStream(@"Release.zip", System.IO.FileMode.Open))
                {
                    PerformUpdate(new ZipArchive(zipToOpen, ZipArchiveMode.Read));
                }
                //Delete the file that was downloaded by the updater
                //File.Delete("Release.zip"); 
            }
            //Start the target application
            //Process.Start(@"Target Application");
        }

        public static void PerformUpdate(ZipArchive archive)
        {
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(System.Environment.CurrentDirectory, file.FullName);
                if (file.Name == "")
                {
                    // Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }

        public static async Task<bool> CheckForUpdate()
        {
            var client = new GitHubClient(new ProductHeaderValue("auto-updater"));

            var tokenAuth = new Credentials("token"); // NOTE: not real token
            client.Credentials = tokenAuth;

            var request = client.Release.GetAll("kdolan", "AutoUpdater");
            var releases = await request;
            var latest = releases[0];

            var remoteVersion = latest.TagName;
            var currentVersion = FileVersionInfo.GetVersionInfo("Updater.exe"); //Get the version of this file. Set in AssemblyInfo.cs. Look for ProductInformationVersion

            if (remoteVersion != currentVersion.ProductVersion) //If the remote version does not equal the current version then download the remote version
            {
                //Download Release.zip here
                var response =
                    await
                        client.Connection.Get<object>(new Uri(latest.Url), new Dictionary<string, string>(),
                            "application/octet-stream");
                //Then call to PerformUpdate to apply the update
                return true;
            }
            else
            {
                return false; //No Update needed
            }
        }


    }
}
