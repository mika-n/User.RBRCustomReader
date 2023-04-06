//
// INIFile.cs - Veeeery simple INI file reader
//
// Copyright (c) 2023 mika-n, www.rallysimfans.hu. No promises and/or warranty given what so ever. This may or may not work. Use at your own risk.
//
// WTFPL licensed to public domain, free for commercial and personal use, modifications and redistribution allowed. http://www.wtfpl.net/
//
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleIniFile
{
    public class IniFile
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key,string def, StringBuilder retVal, int size,string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileInt(string section, string key, int def, string filePath);

        /// <summary>
        /// Read string value from INI file
        /// </summary>
        static public string ReadString(string section, string key, string defaultValue, string iniFilename)
        {
            StringBuilder iniValue = new StringBuilder(255);
            _ = GetPrivateProfileString(section, key, defaultValue, iniValue, 255, iniFilename);
            
            // Trim and remove doublq quotes from the beginning and end of the string value
            string strResult = iniValue.ToString().Trim();

            if (strResult.Length > 1 && strResult[0] == '"')
                strResult = strResult.Substring(1);

            if (strResult.Length > 0 && strResult[strResult.Length - 1] == '"')
                strResult = strResult.Substring(0, strResult.Length - 1);

            return strResult;
        }

        /// <summary>
        /// Read integer value from INI file
        /// </summary>
        static public int ReadInt(string section, string key, int defaultValue, string iniFilename)
        {
            return GetPrivateProfileInt(section, key, defaultValue, iniFilename);
        }
    }
}
