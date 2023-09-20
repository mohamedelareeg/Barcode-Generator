using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Barcode_Generator
{
    public static class SettingsHelper
    {
        private static readonly string SettingsFileName = "appsettings.dat";
        private static readonly byte[] EncryptionKey = HexStringToByteArray("4cef85845ee33ffdc9a0efb1fe8a252c068d12f766acc6405024e2fcf8e4056a");
        private static readonly byte[] InitializationVector = HexStringToByteArray("0583a4f14a0b0d30e27893b5508efcfe");

        // Helper function to convert a hexadecimal string to a byte array
        private static byte[] HexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public static string GenerateRandomKey()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] keyBytes = new byte[32]; // 256 bits
                rng.GetBytes(keyBytes);
                return BitConverter.ToString(keyBytes).Replace("-", "").ToLower();
            }
        }

        public static string GenerateRandomIV()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] ivBytes = new byte[16]; // 128 bits
                rng.GetBytes(ivBytes);
                return BitConverter.ToString(ivBytes).Replace("-", "").ToLower();
            }
        }
        public static AppSettings LoadAppSettings()
        {
            try
            {
                using (FileStream stream = new FileStream(SettingsFileName, FileMode.OpenOrCreate))
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = EncryptionKey;
                    aesAlg.IV = InitializationVector;


                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
                    using (StreamReader reader = new StreamReader(cryptoStream))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        return (AppSettings)formatter.Deserialize(reader.BaseStream);
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions or create new settings if not found
                return new AppSettings();
            }
        }

        public static void SaveAppSettings(AppSettings settings)
        {
            try
            {
                using (FileStream stream = new FileStream(SettingsFileName, FileMode.Create))
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = EncryptionKey;
                    aesAlg.IV = InitializationVector;


                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(cryptoStream, settings);
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions if needed
            }
        }
        public static string GenerateDeviceIdentifier()
        {
            try
            {
                // Collect various hardware and system information
                StringBuilder uniqueStringBuilder = new StringBuilder();

                // Processor ID
                string processorId = GetProcessorId();
                uniqueStringBuilder.AppendLine(processorId);

                // Motherboard Serial Number
                string motherboardSerial = GetMotherboardSerial();
                uniqueStringBuilder.AppendLine(motherboardSerial);

                // MAC Addresses of Network Adapters
                string macAddresses = GetMacAddresses();
                uniqueStringBuilder.AppendLine(macAddresses);

                // BIOS Serial Number
                string biosSerial = GetBiosSerialNumber();
                uniqueStringBuilder.AppendLine(biosSerial);

                // Windows Product Key
                string windowsProductKey = GetWindowsProductKey();
                uniqueStringBuilder.AppendLine(windowsProductKey);

                // Disk Drive Serial Numbers
                string diskDriveSerials = GetDiskDriveSerialNumbers();
                uniqueStringBuilder.AppendLine(diskDriveSerials);

                // System BIOS Version
                string biosVersion = GetBiosVersion();
                uniqueStringBuilder.AppendLine(biosVersion);

                // System UUID (Universally Unique Identifier)
                string systemUuid = GetSystemUuid();
                uniqueStringBuilder.AppendLine(systemUuid);

                // Graphics Card Information
                string graphicsCardInfo = GetGraphicsCardInfo();
                uniqueStringBuilder.AppendLine(graphicsCardInfo);


                // Generate a unique hash based on the collected information
                string deviceIdentifier = CalculateSHA256Hash(uniqueStringBuilder.ToString());

                return deviceIdentifier;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error generating device identifier: {ex.Message}");
                return null;
            }
        }
        private static string CalculateSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder hashBuilder = new StringBuilder();

                foreach (byte b in hashBytes)
                {
                    hashBuilder.Append(b.ToString("x2"));
                }

                return hashBuilder.ToString();
            }
        }
        private static string GetProcessorId()
        {
            string processorId = "";

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    processorId = obj["ProcessorId"].ToString();
                    break; // Take the first processor ID found
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting Processor ID: {ex.Message}");
            }

            return processorId;
        }
        private static string GetMotherboardSerial()
        {
            string motherboardSerial = "";

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    motherboardSerial = obj["SerialNumber"].ToString();
                    break; // Take the first motherboard serial number found
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting Motherboard Serial Number: {ex.Message}");
            }

            return motherboardSerial;
        }

        private static string GetMacAddresses()
        {
            StringBuilder macAddresses = new StringBuilder();

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT MacAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    string macAddress = obj["MacAddress"] as string;
                    if (!string.IsNullOrEmpty(macAddress))
                    {
                        macAddresses.AppendLine(macAddress);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting MAC Addresses: {ex.Message}");
            }

            return macAddresses.ToString();
        }

        private static string GetBiosSerialNumber()
        {
            string biosSerial = "";

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    biosSerial = obj["SerialNumber"].ToString();
                    break; // Take the first BIOS serial number found
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting BIOS Serial Number: {ex.Message}");
            }

            return biosSerial;
        }

        private static string GetWindowsProductKey()
        {
            string productKey = "";

            try
            {
                using (System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM SoftwareLicensingService"))
                {
                    ManagementObjectCollection objCollection = searcher.Get();
                    foreach (ManagementObject obj in objCollection)
                    {
                        if (obj["OA3xOriginalProductKey"] != null)
                        {
                            productKey = obj["OA3xOriginalProductKey"].ToString();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting Windows Product Key: {ex.Message}");
            }

            return productKey;
        }

        private static string GetDiskDriveSerialNumbers()
        {
            StringBuilder diskDriveSerials = new StringBuilder();

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    string serialNumber = obj["SerialNumber"] as string;
                    if (!string.IsNullOrEmpty(serialNumber))
                    {
                        diskDriveSerials.AppendLine(serialNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting Disk Drive Serial Numbers: {ex.Message}");
            }

            return diskDriveSerials.ToString();
        }
        private static string GetBiosVersion()
        {
            string biosVersion = "";

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Version FROM Win32_BIOS");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    biosVersion = obj["Version"].ToString();
                    break; // Take the first BIOS version found
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting BIOS Version: {ex.Message}");
            }

            return biosVersion;
        }

        private static string GetSystemUuid()
        {
            string systemUuid = "";

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    systemUuid = obj["UUID"].ToString();
                    break; // Take the first system UUID found
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting System UUID: {ex.Message}");
            }

            return systemUuid;
        }

        private static string GetGraphicsCardInfo()
        {
            StringBuilder graphicsCardInfo = new StringBuilder();

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    string name = obj["Name"] as string;
                    string caption = obj["Caption"] as string;

                    if (!string.IsNullOrEmpty(name))
                    {
                        graphicsCardInfo.AppendLine($"Name: {name}");
                    }

                    if (!string.IsNullOrEmpty(caption))
                    {
                        graphicsCardInfo.AppendLine($"Caption: {caption}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting Graphics Card Information: {ex.Message}");
            }

            return graphicsCardInfo.ToString();
        }


    }
}
