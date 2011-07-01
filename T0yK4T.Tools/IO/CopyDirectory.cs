using System.IO;

namespace T0yK4T.Tools.IO
{
    /// <summary>
    /// A static class providing a few classes to work with System.IO.Director(y)(ies)
    /// </summary>
    public static class ToyDirectory
    {
        /// <summary>
        /// Contains info needed by various methods in this class
        /// </summary>
        public struct DirCopyInfo
        {
            /// <summary>
            /// The destination (including destination folder)
            /// </summary>
            public string Destination;

            /// <summary>
            /// The source (The folder to copy)
            /// </summary>
            public string Source;

            /// <summary>
            /// Only used internally
            /// </summary>
            internal string BaseDir;

            /// <summary>
            /// Indicates wether or not we want to overwrite the destination files
            /// </summary>
            public bool OverWrite;
        }

        /// <summary>
        /// Starts a Directory copy operation in another thread
        /// </summary>
        /// <param name="CopyFrom">Source and Destination</param>
        public static void BeginAsyncCopy(DirCopyInfo CopyFrom)
        {
            System.Threading.Thread Worker = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(ThreadCopy));
            CopyFrom.BaseDir = CopyFrom.Source;
            Worker.Start((object)CopyFrom);
        }

        private static void ThreadCopy(object CopyFrom)
        {
            Copy((DirCopyInfo)CopyFrom);
        }

        /// <summary>
        /// Copies a directory
        /// </summary>
        /// <param name="CopyFrom">Source and Destination</param>
        public static void Copy(DirCopyInfo CopyFrom)
        {
            DirCopyInfo Info = CopyFrom;
            if (!Directory.Exists(Info.Source))
            {
                throw new FileNotFoundException();
            }
            if (!Directory.Exists(Info.Destination))
            {
                //Console.WriteLine("Creating Directory: " + Info.Destination);
                Directory.CreateDirectory(Info.Destination);
            }
            string[] DirFiles;
            string[] DirDirs;
            try
            {
                DirFiles = Directory.GetFiles(Info.Source);
                DirDirs = Directory.GetDirectories(Info.Source);
            }
            catch { throw new FileNotFoundException(); }
            foreach (string SingleDir in DirDirs)
            {
                string DirName = "\\";
                DirName += SingleDir.Split('\\')[SingleDir.Split('\\').Length - 1];
                DirCopyInfo NextInfo = new DirCopyInfo();
                NextInfo.BaseDir = Info.BaseDir;
                NextInfo.Destination = Info.Destination + DirName;
                NextInfo.Source = SingleDir;
                Copy(NextInfo);
            }
            foreach (string SingleFile in DirFiles)
            {
                try
                {
                    string FileName = SingleFile.Split('\\')[SingleFile.Split('\\').Length - 1];
                    File.Copy(SingleFile, Info.Destination + "\\" + FileName, Info.OverWrite);
                }
                catch { return; }
            }
        }
    }
}
