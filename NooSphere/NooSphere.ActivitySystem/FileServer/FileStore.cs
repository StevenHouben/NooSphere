using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem;
using System.Threading;

namespace NooSphere.ActivitySystem.FileServer
{
    public class FileStore
    {
        #region Events
        public event FileAddedHandler FileAdded;
        public event FileChangedHandler FileChanged;
        public event FileRemovedHandler FileRemoved;
        public event FileDownloadRequestHandler FileDownloadedFromCloud;     
        #endregion

        #region Properties
        public string BasePath { get; set; }
        #endregion

        #region Private Members
        private Dictionary<Guid, Resource> files = new Dictionary<Guid, Resource>();
        #endregion

        #region Public Methods
        public FileStore(string path)
        {
            this.BasePath = path;
        }
        public void AddFile(Resource resource, byte[] fileInBytes,FileSource source)
        {
            Thread t = new Thread(() =>
            {
                SaveToDisk(fileInBytes, resource);
                files.Add(resource.Id, resource);

                if (source == FileSource.Cloud)
                {
                    if (FileDownloadedFromCloud != null)
                        FileDownloadedFromCloud(this, new FileEventArgs(resource));
                }
                else if (source == FileSource.Local)
                {
                    if (FileAdded != null)
                        FileAdded(this, new FileEventArgs(resource));
                }
                Console.WriteLine("FileStore: Added file {0} to store", resource.Name); 
            });
            t.IsBackground = true;
            t.Start();
        }
        public void RemoveFile(Resource resource)
        {
            files.Remove(resource.Id);
            File.Delete(BasePath+resource.RelativePath);
            if (FileRemoved != null)
                FileRemoved(this, new FileEventArgs(resource));
            Console.WriteLine("FileStore: Removed file {0} from store", resource.Name); 
        }
        public byte[] GetFile(Resource resource)
        {
            FileInfo fi = new FileInfo(BasePath + resource.RelativePath);
            byte[] buffer = new byte[fi.Length];

            using (FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                fs.Read(buffer, 0, (int)fs.Length);

            return buffer;
        }
        public void Updatefile(Resource resource, byte[] fileInBytes)
        {
            Thread t = new Thread(() =>
            {
                files[resource.Id] = resource;
                SaveToDisk(fileInBytes,resource);
                if (FileChanged != null)
                    FileChanged(this, new FileEventArgs(resource));
                Console.WriteLine("FileStore: Updated file {0} in store", resource.Name); 
            });
            t.IsBackground = true;
            t.Start();
        }
        #endregion

        #region Private Methods
        private void SaveToDisk(byte[] fileInBytes, Resource resource)
        {
            try
            {
                string path = this.BasePath + resource.RelativePath;
                using (FileStream fileToupload = new FileStream(path, FileMode.Create))
                {
                    fileToupload.Write(fileInBytes, 0, fileInBytes.Length);
                    fileToupload.Close();
                    fileToupload.Dispose();

                    File.SetCreationTimeUtc(path, DateTime.Parse(resource.CreationTime));
                    File.SetLastWriteTimeUtc(path, DateTime.Parse(resource.LastWriteTime));
                    Console.WriteLine("FileStore: Saved file {0} to disk at {1}", resource.Name,path); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

        }
        #endregion
    }
}