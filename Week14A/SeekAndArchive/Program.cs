using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SeekAndArchive
{
    class Program
    {
        private static List<FileInfo> _foundFiles;
        static List<FileSystemWatcher> _watchers;
        static List<DirectoryInfo> _archiveDirs;

       

        static void Main(string[] args)
        {
            string fileName = args[0];
            string directoryName = args[1];
            _foundFiles = new List<FileInfo>();
            _watchers = new List<FileSystemWatcher>();
            _archiveDirs = new List<DirectoryInfo>();

            //examine if the given directory exists at all 
            DirectoryInfo rootDir = new DirectoryInfo(directoryName);
            if (!rootDir.Exists)
            {
                Console.WriteLine("The specified directory does not exist.");
                return;
            }
            RecursiveSearch(_foundFiles, fileName, rootDir);

            //list the found files 
            Console.WriteLine("Found {0} files.", _foundFiles.Count);

            foreach (FileInfo fil in _foundFiles)
            {
                Console.WriteLine("{0}", fil.FullName);
            }

            foreach (FileInfo fil in _foundFiles)
            {
                FileSystemWatcher newWatcher = new FileSystemWatcher(fil.DirectoryName, fil.Name);
                newWatcher.Changed += new FileSystemEventHandler(WatcherChanged);

                newWatcher.EnableRaisingEvents = true;
                _watchers.Add(newWatcher);
            }

            for (int i = 0; i < _foundFiles.Count; i++)
            {
                _archiveDirs.Add(Directory.CreateDirectory("archive" +
                i.ToString()));
            }

            Console.ReadLine();
        }


        static void RecursiveSearch(List<FileInfo> foundFiles, string fileName, DirectoryInfo currentDirectory)
        {
            foreach (FileInfo fil in currentDirectory.GetFiles(fileName))
            {
                foundFiles.Add(fil);
            }
            foreach (DirectoryInfo dir in currentDirectory.GetDirectories())
            {
                RecursiveSearch(foundFiles, fileName, dir);
            }
        }

        static void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.All)
                Console.WriteLine("{0} has been changed!", e.FullPath);
            //find the the index of the changed file 
            FileSystemWatcher senderWatcher = (FileSystemWatcher)sender;
            int index = _watchers.IndexOf(senderWatcher, 0);

            //now that we have the index, we can archive the file 
            ArchiveFile(_archiveDirs[index], _foundFiles[index]);
        }

        static void ArchiveFile(DirectoryInfo archiveDir, FileInfo fileToArchive)
        {
            FileStream input = fileToArchive.OpenRead();
            FileStream output = File.Create(archiveDir.FullName + @"\" + fileToArchive.Name + ".gz");

            GZipStream compressor = new GZipStream(output, CompressionMode.Compress);

            int b = input.ReadByte();

            while (b != -1)
            {
                compressor.WriteByte((byte)b);
                b = input.ReadByte();
            }

            compressor.Close();
            input.Close();
            output.Close();
        }
    }
}