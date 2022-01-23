using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsSensorAgent
{
    class HDDInfoProvider
    {
        public static Dictionary<String, String> GetHDDInfo()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            Dictionary<String, String> hddValues = new Dictionary<String, String>();

            int driveIndex = 1;
            foreach (DriveInfo d in allDrives)
            {
                try
                {
                    hddValues.Add("hdd_drive_name_" + driveIndex, d.Name.Substring(0, 2));
                    hddValues.Add("hdd_drive_label_" + driveIndex, d.VolumeLabel);
                    hddValues.Add("hdd_drive_format_" + driveIndex, d.DriveFormat);
                    hddValues.Add("hdd_drive_total_bytes_" + driveIndex, d.TotalSize.ToString());
                    hddValues.Add("hdd_drive_free_bytes_" + driveIndex, d.TotalFreeSpace.ToString());
                    hddValues.Add("hdd_drive_type_" + driveIndex, d.DriveType.ToString());

                    driveIndex++;
                } catch (Exception ex)
                {
                    Console.WriteLine("Failed to get drive info: " + ex);
                }
            }

            return hddValues;
        }
    }
}
