﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TuyaAPI
{
    public static class FileAccess
    {
        /// <summary>
        /// Reads a file
        /// </summary>
        /// <param name="Fullpath">Filename to read</param>
        /// <returns>Content of File</returns>
        public static List<object[]> ReadFile(string Fullpath)
        {
            List<object[]> erg = new List<object[]>();
            if (File.Exists(Fullpath))
            {
                StreamReader reader = new StreamReader(Fullpath, Encoding.UTF8);
                while (!reader.EndOfStream)
                {
                    erg.Add(reader.ReadLine().Split(new char[] { ';' }));
                }
                reader.Close();
            }
            return erg;
        }

        /// <summary>
        /// Reads a file
        /// </summary>
        /// <param name="Fullpath">Filename to read</param>
        /// <returns>Content of File</returns>
        public static List<string> ReadFileLines(string Fullpath)
        {
            List<string> erg = new List<string>();
            if (File.Exists(Fullpath))
            {
                StreamReader reader = new StreamReader(Fullpath, Encoding.UTF8);
                while (!reader.EndOfStream)
                {
                    erg.Add(reader.ReadLine());
                }
                reader.Close();
            }
            return erg;
        }
        /// <summary>
        /// Writes content to a file
        /// </summary>
        /// <param name="Fullpath">Filename to save</param>
        /// <param name="content">Content to write</param>
        public static void WriteFile(string Fullpath, List<object[]> content)
        {
            StreamWriter writer = new StreamWriter(Fullpath, false, Encoding.UTF8);
            foreach (object[] completeline in content)
            {
                string line = "";
                foreach (object value in completeline)
                    line = line + value.ToString() + ";";
                line = line.Substring(0, line.Length - 1);
                writer.WriteLine(line);
            }
            writer.Close();
        }
        /// <summary>
        /// Logtypes
        /// </summary>
        public enum LogType { LOG, ERROR, INFO, WARNING };
        /// <summary>
        /// Writes content to a file
        /// </summary>
        /// <param name="Fullpath">Filename to save</param>
        /// <param name="log">Line to Add</param>
        /// <param name="logType">Type of Log</param>
        /// <param name="source">Name of Logsource</param>
        public static void WriteLogFile(string Fullpath, string log, LogType logType, string source, string ProgramDataPath)
        {
            Fullpath = ProgramDataPath + "\\Logs\\" + Fullpath;
            string logline = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "] - [" + source + "] - [" + logType.ToString() + "] | " + log;
            List<string> content = ReadFileLines(Fullpath);
            StreamWriter writer = new StreamWriter(Fullpath, false, Encoding.UTF8);
            foreach (string completeline in content)
            {
                writer.WriteLine(completeline);
            }
            writer.WriteLine(logline);
            writer.Close();
        }
        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="Fullpath">Path to File</param>
        public static void DeleteFile(string Fullpath)
        {
            if (File.Exists(Fullpath))
                File.Delete(Fullpath);
        }
        /// <summary>
        /// Check, if folder is available, if not, the folder will created.
        /// </summary>
        /// <param name="Path">Path to folder</param>
        public static void CheckFolder(string Path)
        {
            Directory.CreateDirectory(Path);
        }
        /// <summary>
        /// Deletes an empty Directory
        /// </summary>
        /// <param name="Path">Path to Folder</param>
        /// <returns>true if deleted</returns>
        public static bool DeleteFolder(string Path)
        {
            if (Directory.Exists(Path))
            {
                try
                {
                    Directory.Delete(Path);
                }
                catch { }
                System.Threading.Thread.Sleep(2000);
            }
            return !Directory.Exists(Path);
        }

        /// <summary>
        /// Write Content to a CSV File
        /// </summary>
        /// <param name="Path">Path to the File</param>
        /// <param name="lines">Lines to write</param>
        public static void WriteCSVFile(string Path, List<string> lines)
        {
            StreamWriter writer = new StreamWriter(Path);
            foreach (string line in lines)
                writer.WriteLine(line);
            writer.Close();
        }

        public static void WriteJSONFile(string Path, string json)
        {
            File.WriteAllText(Path, json);
        }

        public static string ReadJSONFile(string Path)
        {
            return File.ReadAllText(Path);
        }
        /// <summary>
        /// Check if file exist
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static bool CheckFile(string Path)
        {
            return File.Exists(Path);
        }
    }
}
