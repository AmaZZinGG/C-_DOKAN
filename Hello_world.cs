using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using System.Text;
using System.IO;
using DokanNet.Native;


namespace Hello_world
{
    class Hello_World : IDokanOperations
    {


        string logger = "Errors:\n";
        DateTime dateTime = DateTime.Now;

        //Dictionary<int, string> people = new Dictionary<int, string>();
        Dictionary<string, List<string>> files = new Dictionary<string, List<string>>()
        {
        { "\\", new List<string>(){  "\\Склад_1", "\\Склад_2", "\\TOTAL", "\\ERRORS" } },
        { "\\TOTAL", new List<string>() {}},
        { "\\ERRORS", new List<string>() { "\\ERRORS\\logger.txt"}},
        { "\\Склад_1", new List<string>() { "\\Склад_1\\Молоко.txt", "\\Склад_1\\Кефир.txt" }},
        { "\\Склад_2", new List<string>() { "\\Склад_2\\Молоко.txt" }},

        //{ "\\HelloWorld.txt", new List<string>() {"HELLO"}},
         { "\\ERRORS\\logger.txt", new List<string>() {"Errors:\n"}},
        
        { "\\Склад_1\\Молоко.txt", new List<string>() {"20 from Tula"}},
        { "\\Склад_1\\Кефир.txt", new List<string>() {"100 from Moscow"}},
        { "\\Склад_2\\Молоко.txt", new List<string>() {"50 from Belgorod"}},
          //  .EndsWith(".txt")
    };


        public void update()
        {
            Console.WriteLine("SSSS\n");
            Dictionary<string, int> tovars = new Dictionary<string, int>() { };
            foreach (var file in this.files["\\"])
            {
                
                if (! file.EndsWith(".txt"))
                {
                    //Переходим в каталог
                    if((file == "\\TOTAL") || (file == "\\ERRORS")){
                        continue;
                    }
                    foreach (var text_file in this.files[file])
                    {
                        string[] namesFile = text_file.Split('\\');
                        String nameTovar = namesFile[namesFile.Length - 1];
                        int value = 0;
                        try
                        {
                            value = System.Convert.ToInt32(((this.files[text_file][0]).Split(' '))[0]);
                        }
                        catch
                        {
                            this.logger += "update -> In file " + text_file + "  Invalid value of the first element\n";
                            continue;
                        }
                        
                        if (tovars.ContainsKey(nameTovar))
                        {
                            tovars[nameTovar] += value;
                        }
                        else
                        {
                            tovars.Add(nameTovar, value);
                        }
                    }

                }
            }

            //Очистка ТОТАЛА
            //this.files["//TOTAL"].Clear();
           /* foreach (var tovar in this.files["\\TOTAL"])
            {
                try {
                    this.files[tovar] = new List<string>() { "" };
                    this.files.Remove(tovar);
                }
                catch { }


            }*/
            this.files["\\TOTAL"] = new List<string>() { };

            //Заполнение ТОТАЛа
            //throw new NotImplementedException();
            foreach (var tovar in tovars)
            {
                this.files["\\TOTAL"].Add("\\TOTAL\\" + tovar.Key);
                this.files.Remove("\\TOTAL\\" + tovar.Key);
                try
                {
                    this.files.Add("\\TOTAL\\" + tovar.Key, new List<string>() { tovar.Value.ToString() });
                }
                catch
                {
                    this.files["\\TOTAL\\" + tovar.Key] = new List<string>() { tovar.Value.ToString() };
                }
               // this.files.Add("\\TOTAL\\" + tovar.Key, new List<string>() { tovar.Value.ToString()});
            }


        }

     
        public void Cleanup(string fileName, IDokanFileInfo info)
        {
           //throw new NotImplementedException();
        }

        public void CloseFile(string fileName, IDokanFileInfo infooptions)
        {

            //throw new NotImplementedException();
        }

            public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            string[] names = fileName.Split('\\');
            string dir = "\\";
            if (names.Length > 2)
            {
                dir += names[1];
            }

            if (options.ToString() == "DeleteOnClose" && (fileName != "\\TOTAL") && (fileName != "\\ERRORS"))
            {
    
                this.files.Remove(fileName);
                this.files[dir].Remove(fileName);
                this.dateTime = DateTime.Now;
                this.update();
                return NtStatus.NoSuchFile;
            }

            TimeSpan interval = this.dateTime - DateTime.Now;
            if (-interval.TotalSeconds > 2)
            {
               
                if (names[names.Length - 1] == "Новая папка" && !this.files.ContainsKey(fileName))
                {
                    this.files.Add(fileName, new List<string>() { });
                    this.files[dir].Add(fileName);
                    this.update();
                }
                if (names[names.Length - 1] == "Новый текстовый документ.txt" && !this.files.ContainsKey(fileName))
                {
                    this.files.Add(fileName, new List<string>() {"0 NEW FILE" });
                    this.files[dir].Add(fileName);
                    this.update();
                }
                this.update();
            }

            //throw new NotImplementedException();

            
            return NtStatus.Success;

        }


        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            if ((fileName != "\\TOTAL") && (fileName != "\\ERRORS"))
            {
                this.files.Remove(fileName);
                this.files["\\"].Remove(fileName);
            }
            else
            {
                this.logger += "DeleteDirectory -> this directory(" + fileName + ") cannot be deleted!\n";
            }

            this.dateTime = DateTime.Now;

            info.DeleteOnClose = true;
            return NtStatus.Success;
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            this.dateTime = DateTime.Now;
            return NtStatus.Success;
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
           
           files = new List<FileInformation>();

            FileInformation fi;

            Console.WriteLine(fileName);
            try { 
                foreach (var path in this.files[fileName])
                {
                    GetFileInformation(path, out fi, null);
                    files.Add(fi);
                }

            }
            catch
            {
                //Можно  логгер добавить
            }

            return NtStatus.Success;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            
            if (searchPattern == "*") return FindFiles(fileName, out files, info);
            else return FindFiles(fileName + "\\" + searchPattern, out files, info);
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            //throw new NotImplementedException();
            freeBytesAvailable = 0x4000000 ;
            totalNumberOfBytes = 0x4000000 * 5;
            totalNumberOfFreeBytes = 0x4000000;
            return NtStatus.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
 
            if (! this.files.ContainsKey(fileName))
            {
                fileInfo = new FileInformation { };
                return NtStatus.NoSuchFile;
            }

         
            if (fileName.EndsWith(".txt"))
            {
                string[] names = fileName.Split('\\');

                fileInfo = new FileInformation
                {
                    Attributes = FileAttributes.Normal,
                    CreationTime = DateTime.Now,
                    FileName = names[names.Length - 1],
                    LastAccessTime = DateTime.Now,
                    LastWriteTime = DateTime.Now,
                    Length = 57000 // Длина в байтах строки, которую мы возвращаем через ReadFile
                };
                return NtStatus.Success;
            }
            else if (fileName.EndsWith("\\"))
            {
                fileInfo = new FileInformation
                {
                    FileName = fileName.Replace("\\", ""),
                    LastAccessTime = DateTime.Now,
                    Attributes = FileAttributes.Directory
                };
                return NtStatus.Success;
            }
            else
            {
                fileInfo = new FileInformation
                {
                    FileName = fileName.Replace("\\", ""),
                    LastAccessTime = DateTime.Now,
                    Attributes = FileAttributes.Directory
                };
                return NtStatus.Success;
            }
            
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {

            //throw new NotImplementedException();
            security = null;
            return NtStatus.NotImplemented;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = "Hello world, Start!";
            features = FileSystemFeatures.None;
            fileSystemName = "Hello_World_Start";
            maximumComponentLength = 25600;
            return NtStatus.Success;
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            //throw new NotImplementedException();
            return NtStatus.Success;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            if (oldName == newName)
            {
                this.logger += "MoveFile -> oldName = newName (" + newName + "), renaming is not possible!\n";
                return NtStatus.Success;
            }

            if (this.files.ContainsKey(newName))
            {
                this.logger += "MoveFile -> newName (" + newName + "), already existse!\n";
                return NtStatus.Success;
            }
            

            string[] names = oldName.Split('\\');
            string dir = "\\";
            if (names.Length > 2)
            {
                dir += names[1]; 
            }
            Console.WriteLine(dir);

            Console.WriteLine(oldName);
            Console.WriteLine(newName);

            this.files.Add(newName, this.files[oldName]);
            this.files[dir].Add(newName);
            this.files[dir].Remove(oldName);
            this.files.Remove(oldName);
            this.dateTime = DateTime.Now;

            this.update();
            return NtStatus.Success;
           // throw new NotImplementedException();
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            bytesRead = 0;
            var x = Encoding.ASCII.GetBytes("_Hello World from HelloWorldFS!\r\nThis is just a test file./");
            if(fileName == "\\ERRORS\\logger.txt") x = Encoding.ASCII.GetBytes(this.logger.ToString());
            else x = Encoding.ASCII.GetBytes(this.files[fileName][0].ToString());
            //this.id += 1;
            if (info.Context == null) // memory mapped read
            {
                using (var stream = new MemoryStream(x))
                {
                    //buffer = Encoding.Default.GetBytes("Hello");
                    stream.Position = offset;
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                  
                }
            }
            return NtStatus.Success;
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
           // this.logger += "SetAllocationSize ||| " + fileName + "\n";
            return NtStatus.Success;
            throw new NotImplementedException();
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
          //  this.logger += "SetEndOfFile ||| " + fileName + "\n";
            return NtStatus.Success;
            throw new NotImplementedException();
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            //return NtStatus.Success;////////////aaaaaa
            throw new NotImplementedException();
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            //throw new NotImplementedException();
            return NtStatus.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
           // this.id += 1;
            string s = Encoding.Default.GetString(buffer);
            Console.WriteLine(s);
            this.files[fileName][0] = s;
           

            bytesWritten = 0;
            //var x = Encoding.ASCII.GetBytes("1234567");
            ////this.id += 1;
            //if (info.Context == null) // memory mapped read
            //{
            //    using (var stream = new MemoryStream(x))
            //    {
            //        this.stream = stream;
            //        this.stream.Position = offset;
            //        bytesWritten = this.stream.Read(buffer, 0, buffer.Length);


            //    }
            //}

            return NtStatus.Success;
           // throw new NotImplementedException();
        }
    }
}
