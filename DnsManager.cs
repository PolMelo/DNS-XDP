using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using Microsoft.Win32;

namespace DnsSwitcher
{
    public static class DnsManager
    {
        static readonly string BackupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "DnsSwitcher", "backup.txt");

        public static bool HasPendingBackup
        {
            get { return File.Exists(BackupPath); }
        }

        public static void Enable(string[] servers)
        {
            // Solo guarda el backup si no hay uno pendiente (así nunca se pisa la DNS original)
            if (!HasPendingBackup) SaveBackup();

            foreach (ManagementObject mo in GetAdapters())
                SetDns(mo, servers);

            FlushDns();
        }

        public static void Restore()
        {
            if (!HasPendingBackup) return;

            var saved = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(BackupPath))
            {
                var parts = line.Split('|');
                if (parts.Length == 2) saved[parts[0]] = parts[1];
            }

            foreach (ManagementObject mo in GetAdapters())
            {
                string guid = (string)mo["SettingID"];
                string ns;
                if (!saved.TryGetValue(guid, out ns)) continue;

                // Vacío = estaba en automático (DHCP) -> null la devuelve a automático
                string[] servers = string.IsNullOrWhiteSpace(ns)
                    ? null
                    : ns.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                SetDns(mo, servers);
            }

            File.Delete(BackupPath);
            FlushDns();
        }

    

        static System.Collections.Generic.List<ManagementObject> GetAdapters()
        {
            var list = new System.Collections.Generic.List<ManagementObject>();
            using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                foreach (ManagementObject mo in mc.GetInstances())
                    if ((bool)mo["IPEnabled"]) list.Add(mo);
            }
            return list;
        }

        static void SaveBackup()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BackupPath));
            var lines = new System.Collections.Generic.List<string>();
            foreach (ManagementObject mo in GetAdapters())
            {
                string guid = (string)mo["SettingID"];
                lines.Add(guid + "|" + ReadStaticDns(guid));
            }
            File.WriteAllLines(BackupPath, lines);
        }

        // NameServer vacío = DNS automática; con valor = DNS manual
        static string ReadStaticDns(string guid)
        {
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = hklm.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\" + guid))
            {
                return key == null ? "" : (key.GetValue("NameServer") as string ?? "");
            }
        }

        static void SetDns(ManagementObject mo, string[] servers)
        {
            var p = mo.GetMethodParameters("SetDNSServerSearchOrder");
            p["DNSServerSearchOrder"] = servers;
            var r = mo.InvokeMethod("SetDNSServerSearchOrder", p, null);
            uint code = Convert.ToUInt32(r["ReturnValue"]);
            if (code != 0 && code != 1)
                throw new Exception("No se pudo cambiar la DNS (código " + code + ").");
        }

        static void FlushDns()
        {
            var psi = new ProcessStartInfo("ipconfig", "/flushdns")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using (var p = Process.Start(psi)) { p.WaitForExit(); }
        }
    }
}