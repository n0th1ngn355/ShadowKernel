using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.NetworkInformation;

namespace Client.Helpers.Information
{
    class Ports
    {



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
		public string pid { get; set; }
		public string status { get; set; }
	}


}
