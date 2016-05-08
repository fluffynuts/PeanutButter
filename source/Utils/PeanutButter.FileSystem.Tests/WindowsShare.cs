using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Principal;

namespace PeanutButter.FileSystem.Tests
{
    // lifted from:
    // http://www.harindaka.com/2011/11/having-to-write-piece-of-code-for-above.html
    internal class WindowsShare
    {
        public enum MethodStatus : uint
        {
            Success = 0,     //Success
            AccessDenied = 2,     //Access denied
            UnknownFailure = 8,     //Unknown failure
            InvalidName = 9,     //Invalid name
            InvalidLevel = 10,     //Invalid level
            InvalidParameter = 21,     //Invalid parameter
            DuplicateShare = 22,     //Duplicate share
            RedirectedPath = 23,     //Redirected path
            UnknownDevice = 24,     //Unknown device or directory
            NetNameNotFound = 25     //Net name not found
        }

        public enum ShareType : uint
        {
            DiskDrive = 0x0,     //Disk Drive
            PrintQueue = 0x1,     //Print Queue
            Device = 0x2,     //Device
            IPC = 0x3,     //IPC
            DiskDriveAdmin = 0x80000000,     //Disk Drive Admin
            PrintQueueAdmin = 0x80000001,     //Print Queue Admin
            DeviceAdmin = 0x80000002,     //Device Admin
            IpcAdmin = 0x80000003     //IPC Admin
        }

        public enum AccessMaskTypes
        {
            FullControl = 2032127,
            Change = 1245631,
            ReadOnly = 1179817
        }

        private ManagementObject _winShareObject;

        private WindowsShare(ManagementObject obj)
        {
            _winShareObject = obj;
        }

        public ManagementObject ManagementObject
        {
            get { return _winShareObject; }
        }

        public uint AccessMask
        {
            get { return Convert.ToUInt32(_winShareObject["AccessMask"]); }
        }

        public bool AllowMaximum
        {
            get { return Convert.ToBoolean(_winShareObject["AllowMaximum"]); }
        }

        public string Caption
        {
            get { return Convert.ToString(_winShareObject["Caption"]); }
        }

        public string Description
        {
            get { return Convert.ToString(_winShareObject["Description"]); }
        }

        public DateTime InstallDate
        {
            get { return Convert.ToDateTime(_winShareObject["InstallDate"]); }
        }

        public uint MaximumAllowed
        {
            get { return Convert.ToUInt32(_winShareObject["MaximumAllowed"]); }
        }

        public string Name
        {
            get { return Convert.ToString(_winShareObject["Name"]); }
        }

        public string Path
        {
            get { return Convert.ToString(_winShareObject["Path"]); }
        }

        public string Status
        {
            get { return Convert.ToString(_winShareObject["Status"]); }
        }

        public ShareType Type
        {
            get { return (ShareType)Convert.ToUInt32(_winShareObject["Type"]); }
        }


        public MethodStatus Delete()
        {
            object result = _winShareObject.InvokeMethod("Delete", new object[] { });
            uint r = Convert.ToUInt32(result);

            return (MethodStatus)r;
        }

        public static MethodStatus Create(string path, string name, ShareType type, uint? maximumAllowed, string description, string password)
        {
            ManagementClass mc = new ManagementClass("Win32_Share");

            ManagementBaseObject shareParams = mc.GetMethodParameters("Create");
            shareParams["Path"] = path;
            shareParams["Name"] = name;
            shareParams["Type"] = (uint)type;
            shareParams["Description"] = description;

            if (maximumAllowed != null)
                shareParams["MaximumAllowed"] = maximumAllowed;

            if (!String.IsNullOrEmpty(password))
                shareParams["Password"] = password;

            ManagementBaseObject result = mc.InvokeMethod("Create", shareParams, null);

            return (MethodStatus)(result.Properties["ReturnValue"].Value);
        }

        public static IList<WindowsShare> GetAllShares()
        {
            IList<WindowsShare> result = new List<WindowsShare>();
            ManagementClass mc = new ManagementClass("Win32_Share");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                WindowsShare share = new WindowsShare(mo);
                result.Add(share);
            }

            return result;
        }

        public static WindowsShare GetShareByName(string name)
        {
            name = name.ToUpper();

            IList<WindowsShare> shares = GetAllShares();

            foreach (WindowsShare s in shares)
                if (s.Name.ToUpper() == name)
                    return s;

            return null;
        }

        public static WindowsShare GetShareByPath(string path)
        {
            path = path.ToUpper();

            IList<WindowsShare> shares = GetAllShares();

            foreach (WindowsShare s in shares)
                if (s.Path.ToUpper() == path)
                    return s;

            return null;
        }

        public MethodStatus SetPermission(string domain, string userName, AccessMaskTypes amtype)
        {
            NTAccount account = new NTAccount(domain, userName);
            SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            byte[] sidArray = new byte[sid.BinaryLength];
            sid.GetBinaryForm(sidArray, 0);

            ManagementObject trustee = new ManagementClass(new ManagementPath("Win32_Trustee"), null);
            trustee["Domain"] = domain;
            trustee["Name"] = userName;
            trustee["SID"] = sidArray;

            ManagementObject adminACE = new ManagementClass(new ManagementPath("Win32_Ace"), null);
            adminACE["AccessMask"] = (int)amtype;
            adminACE["AceFlags"] = 3;
            adminACE["AceType"] = 0;
            adminACE["Trustee"] = trustee;

            ManagementObject secDescriptor = new ManagementClass(new ManagementPath("Win32_SecurityDescriptor"), null);
            secDescriptor["ControlFlags"] = 4; //SE_DACL_PRESENT 
            secDescriptor["DACL"] = new object[] { adminACE };

            object result = _winShareObject.InvokeMethod("SetShareInfo", new object[] { Int32.MaxValue, this.Description, secDescriptor });
            uint r = Convert.ToUInt32(result);

            return (MethodStatus)r;
        }
    }
}