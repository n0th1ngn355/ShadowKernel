using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Management;
using System.Globalization;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;


namespace ConsoleApp2
{
	class Program
	{

		static void Main(string[] args)
		{
			var driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive");
			foreach (ManagementObject d in driveQuery.Get())
			{
				var deviceId = d.Properties["DeviceId"].Value;
				//Console.WriteLine("Device");
				//Console.WriteLine(d);
				var partitionQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_DiskDriveToDiskPartition", d.Path.RelativePath);
				var partitionQuery = new ManagementObjectSearcher(partitionQueryText);
				foreach (ManagementObject p in partitionQuery.Get())
				{
					//Console.WriteLine("Partition");
					//Console.WriteLine(p);
					var logicalDriveQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_LogicalDiskToPartition", p.Path.RelativePath);
					var logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
					foreach (ManagementObject ld in logicalDriveQuery.Get())
					{
						//Console.WriteLine("Logical drive");
						//Console.WriteLine(ld);

						var physicalName = Convert.ToString(d.Properties["Name"].Value); // \\.\PHYSICALDRIVE2
						var diskName = Convert.ToString(d.Properties["Caption"].Value); // WDC WD5001AALS-xxxxxx
						var diskModel = Convert.ToString(d.Properties["Model"].Value); // WDC WD5001AALS-xxxxxx
						var diskInterface = Convert.ToString(d.Properties["InterfaceType"].Value); // IDE
						var capabilities = (UInt16[])d.Properties["Capabilities"].Value; // 3,4 - random access, supports writing
						var mediaLoaded = Convert.ToBoolean(d.Properties["MediaLoaded"].Value); // bool
						var mediaType = Convert.ToString(d.Properties["MediaType"].Value); // Fixed hard disk media
						var mediaSignature = Convert.ToUInt32(d.Properties["Signature"].Value); // int32
						var mediaStatus = Convert.ToString(d.Properties["Status"].Value); // OK

						var driveName = Convert.ToString(ld.Properties["Name"].Value); // C:
						var driveId = Convert.ToString(ld.Properties["DeviceId"].Value); // C:
						var driveCompressed = Convert.ToBoolean(ld.Properties["Compressed"].Value);
						var driveType = Convert.ToUInt32(ld.Properties["DriveType"].Value); // C: - 3
						var fileSystem = Convert.ToString(ld.Properties["FileSystem"].Value); // NTFS
						var freeSpace = Convert.ToUInt64(ld.Properties["FreeSpace"].Value); // in bytes
						var totalSpace = Convert.ToUInt64(ld.Properties["Size"].Value); // in bytes
						var driveMediaType = Convert.ToUInt32(ld.Properties["MediaType"].Value); // c: 12
						var volumeName = Convert.ToString(ld.Properties["VolumeName"].Value); // System
						var volumeSerial = Convert.ToString(ld.Properties["VolumeSerialNumber"].Value); // 12345678

						//Console.WriteLine("PhysicalName: {0}", physicalName);
						//Console.WriteLine("DiskName: {0}", diskName);
						//Console.WriteLine("DiskModel: {0}", diskModel);
						//Console.WriteLine("DiskInterface: {0}", diskInterface);
						//// Console.WriteLine("Capabilities: {0}", capabilities);
						//Console.WriteLine("MediaLoaded: {0}", mediaLoaded);
						//Console.WriteLine("MediaType: {0}", mediaType);
						//Console.WriteLine("MediaSignature: {0}", mediaSignature);
						//Console.WriteLine("MediaStatus: {0}", mediaStatus);

						//Console.WriteLine("DriveName: {0}", driveName);
						//Console.WriteLine("DriveId: {0}", driveId);
						//Console.WriteLine("DriveCompressed: {0}", driveCompressed);
						//Console.WriteLine("DriveType: {0}", driveType);
						//Console.WriteLine("FileSystem: {0}", fileSystem);
						//Console.WriteLine("FreeSpace: {0}", (float)freeSpace / 1073741824);
						//Console.WriteLine("TotalSpace: {0}", (float)totalSpace / 1073741824);
						//Console.WriteLine("DriveMediaType: {0}", driveMediaType);
						//Console.WriteLine("VolumeName: {0}", volumeName);
						//Console.WriteLine("VolumeSerial: {0}", volumeSerial);

						//Console.WriteLine(new string('-', 79));
					}
				}
			}

			List<InstalledProgram> installedprograms = new List<InstalledProgram>();
			string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
			{
				foreach (string subkey_name in key.GetSubKeyNames())
				{
					using (RegistryKey subkey = key.OpenSubKey(subkey_name))
					{
						if (subkey.GetValue("DisplayName") != null)
						{
							installedprograms.Add(new InstalledProgram
							{
								DisplayName = (string)subkey.GetValue("DisplayName"),
								Version = (string)subkey.GetValue("DisplayVersion"),
								InstalledDate = (string)subkey.GetValue("InstallDate"),
								Publisher = (string)subkey.GetValue("Publisher"),
								UnninstallCommand = (string)subkey.GetValue("UninstallString"),
								ModifyPath = (string)subkey.GetValue("ModifyPath")
							});
							string a = "";
							if (subkey.GetValue("InstallDate") != null)
							{
								a = subkey.GetValue("InstallDate").ToString();
								a = a.Substring(6, 2) + "."+ a.Substring(4, 2) + "." + a.Substring(0, 4);
							}
							Console.WriteLine(subkey.GetValue("DisplayName") + " " + subkey.GetValue("DisplayVersion")
								+ " " + a + " " + subkey.GetValue("Publisher"));
						}
					}

				}
			}


			Console.Read();
		}

		public class InstalledProgram
		{
			public string DisplayName { get; set; }
			public string Version { get; set; }
			public string InstalledDate { get; set; }
			public string Publisher { get; set; }
			public string UnninstallCommand { get; set; }
			public string ModifyPath { get; set; }
		}






	}


}
