﻿using GzsTool.Core.Common;
using GzsTool.Core.Common.Interfaces;
using GzsTool.Core.Fpk;
using GzsTool.Core.Qar;
using GzsTool.Core.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SnakeBite.GzsTool
{
    public static class GzsLib
    {
        // Extract full archive
        public static List<string> ExtractArchive<T>(string FileName, string OutputPath) where T : ArchiveFile, new()
        {
            LoadDictionaries();
            using (FileStream archiveFile = new FileStream(FileName, FileMode.Open))
            {
                List<string> outFiles = new List<string>();
                T archive = new T();
                archive.Name = Path.GetFileName(FileName);
                archive.Read(archiveFile);

                // Select single file for output

                foreach (var v in archive.ExportFiles(archiveFile))
                {
                    string outDirectory = Path.Combine(OutputPath, Path.GetDirectoryName(v.FileName));
                    string outFileName = Path.Combine(OutputPath, v.FileName);
                    if (!Directory.Exists(outDirectory)) Directory.CreateDirectory(outDirectory);
                    using (FileStream outStream = new FileStream(outFileName, FileMode.Create))
                    {
                        // copy to output stream
                        v.DataStream().CopyTo(outStream);
                        outFiles.Add(v.FileName);
                    }
                    Application.DoEvents();
                }

                return outFiles;
            }
        }

        // Extract single file from archive
        public static bool ExtractFile<T>(string SourceArchive, string FilePath, string OutputFile) where T : ArchiveFile, new()
        {
            // Get file hash from path
            ulong fileHash = Tools.NameToHash(FilePath);

            using (FileStream archiveFile = new FileStream(SourceArchive, FileMode.Open))
            {
                T archive = new T();
                archive.Name = Path.GetFileName(SourceArchive);
                archive.Read(archiveFile);

                // Select single file for output
                var outFile = archive.ExportFiles(archiveFile).FirstOrDefault(entry => Tools.NameToHash(entry.FileName) == fileHash);

                if (outFile != null)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(OutputFile))) Directory.CreateDirectory(Path.GetDirectoryName(OutputFile));
                    using (FileStream outStream = new FileStream(OutputFile, FileMode.Create))
                    {
                        // copy to output stream
                        outFile.DataStream().CopyTo(outStream);
                    }
                    return true;
                }
                else
                {
                    // file not found
                    return false;
                }
            }
        }

        // Extract single file from archive
        public static bool ExtractFileByHash<T>(string SourceArchive, ulong FileHash, string OutputFile) where T : ArchiveFile, new()
        {
            LoadDictionaries();

            // Get file hash from path
            ulong fileHash = FileHash;

            using (FileStream archiveFile = new FileStream(SourceArchive, FileMode.Open))
            {
                T archive = new T();
                archive.Name = Path.GetFileName(SourceArchive);
                archive.Read(archiveFile);

                // Select single file for output
                var outFile = archive.ExportFiles(archiveFile).FirstOrDefault(entry => Tools.NameToHash(entry.FileName) == fileHash);

                if (outFile != null)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(OutputFile))) Directory.CreateDirectory(Path.GetDirectoryName(OutputFile));
                    using (FileStream outStream = new FileStream(OutputFile, FileMode.Create))
                    {
                        // copy to output stream
                        outFile.DataStream().CopyTo(outStream);
                    }
                    return true;
                }
                else
                {
                    // file not found
                    return false;
                }
            }
        }

        // Read file hashes contained within QAR archive
        public static List<GameFile> ListArchiveHashes<T>(string ArchiveName) where T : ArchiveFile, new()
        {
            using (FileStream archiveFile = new FileStream(ArchiveName, FileMode.Open))
            {
                List<GameFile> archiveContents = new List<GameFile>();
                T archive = new T();
                archive.Name = Path.GetFileName(ArchiveName);
                archive.Read(archiveFile);
                foreach (var x in archive.ExportFiles(archiveFile))
                {
                    archiveContents.Add(new GameFile() { FilePath = x.FileName, FileHash = Tools.NameToHash(x.FileName), QarFile = archive.Name });
                }
                return archiveContents;
            }
        }

        // Read file hashes contained within QAR archive
        public static List<string> ListArchiveContents<T>(string ArchiveName) where T : ArchiveFile, new()
        {
            using (FileStream archiveFile = new FileStream(ArchiveName, FileMode.Open))
            {
                List<string> archiveContents = new List<string>();
                T archive = new T();
                archive.Name = Path.GetFileName(ArchiveName);
                archive.Read(archiveFile);
                foreach (var x in archive.ExportFiles(archiveFile))
                {
                    archiveContents.Add(x.FileName);
                }
                return archiveContents;
            }
        }

        // Load filename dictionaries
        public static void LoadDictionaries()
        {
            Hashing.ReadDictionary("qar_dictionary.txt");
            Hashing.ReadMd5Dictionary("fpk_dictionary.txt");
        }

        // Export FPK archive with specified parameters
        public static void WriteFpkArchive(string FileName, string SourceDirectory, List<string> Files)
        {
            FpkFile q = new FpkFile() { Name = FileName };
            foreach (string s in Files)
            {
                q.Entries.Add(new FpkEntry() { FilePath = s });
            }

            using (FileStream outFile = new FileStream(FileName, FileMode.Create))
            {
                IDirectory fileDirectory = new FileSystemDirectory(SourceDirectory);
                q.Write(outFile, fileDirectory);
            }
        }

        // Export QAR archive with specified parameters
        public static void WriteQarArchive(string FileName, string SourceDirectory, List<string> Files, uint Flags)
        {
            List<QarEntry> qarEntries = new List<QarEntry>();
            foreach (string s in Files)
            {
                qarEntries.Add(new QarEntry() { FilePath = s, Hash = Tools.NameToHash(s), Compressed = Path.GetExtension(s).Contains(".fpk") ? true : false });
            }

            QarFile q = new QarFile() { Entries = qarEntries, Flags = Flags, Name = FileName };

            using (FileStream outFile = new FileStream(FileName, FileMode.Create))
            {
                IDirectory fileDirectory = new FileSystemDirectory(SourceDirectory);
                q.Write(outFile, fileDirectory);
            }
        }
    }
}