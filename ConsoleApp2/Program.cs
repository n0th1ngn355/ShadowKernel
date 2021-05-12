using System;
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
			Process[] processlist = Process.GetProcesses();


			Console.WriteLine("{0,-50} {1,-20} {2,-50} {3,-20} {4,-30} {5,-30} {6,-30}\n", "PID", "Port", "P_name" ,"Protocol" , "Local", "Remote","Status");
			List<Port> a = GetNetStatPorts();
			foreach(var e in a)
            {
				Console.WriteLine("{0,-50} {1,-20} {2,-50} {3,-20} {4,-30} {5,-30}{6,-30}",e.pid, e.port_number , e.process_name , e.protocol, e.local_address, e.remote_address, e.status);
            }



			Console.Read();
		}





		public static List<Port> GetNetStatPorts()
		{
			var Ports = new List<Port>();

			try
			{
				using (Process p = new Process())
				{

					ProcessStartInfo ps = new ProcessStartInfo();
					ps.Arguments = "-a -n -o";
					ps.FileName = "netstat.exe";
					ps.UseShellExecute = false;
					ps.RedirectStandardInput = true;
					ps.RedirectStandardOutput = true;
					ps.RedirectStandardError = true;

					p.StartInfo = ps;
					p.Start();

					StreamReader stdOutput = p.StandardOutput;
					StreamReader stdError = p.StandardError;

					string content = stdOutput.ReadToEnd() + stdError.ReadToEnd();
					string exitStatus = p.ExitCode.ToString();

					if (exitStatus != "0")
					{
						// Command Errored. Handle Here If Need Be
					}

					//Get The Rows
					string[] rows = Regex.Split(content, "\r\n");
					foreach (string row in rows)
					{
						//Split it baby
						string[] tokens = Regex.Split(row, "\\s+");
						if (tokens.Length > 4 && (tokens[1].Equals("UDP") || tokens[1].Equals("TCP")))
						{
							string localAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");
							Ports.Add(new Port
							{
								local_address = localAddress,
								remote_address = tokens[3],
								protocol = localAddress.Contains("1.1.1.1") ? String.Format("{0}v6", tokens[1]) : String.Format("{0}v4", tokens[1]),
								port_number = localAddress.Split(':')[1],
								process_name = tokens[1] == "UDP" ? LookupProcess(Convert.ToInt16(tokens[4])) : LookupProcess(Convert.ToInt16(tokens[5])),
								pid = tokens[1] == "UDP" ? tokens[4] : tokens[5],
								status = tokens[1] == "UDP" ? "" : tokens[4]
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
		    }
			return Ports;
		}

		public static string LookupProcess(int pid)
		{
			string procName;
			try { procName = Process.GetProcessById(pid).ProcessName; }
			catch (Exception) { procName = "-"; }
			return procName;
		}

		// ===============================================
		// The Port Class We're Going To Create A List Of
		// ===============================================
		public class Port
		{
			public string local_address { get; set; }
			public string remote_address { get; set; }
			public string port_number { get; set; }
			public string process_name { get; set; }
			public string protocol { get; set; }
			public string pid{ get; set; }
			public string status{ get; set; }
		}


		 



	}


}
