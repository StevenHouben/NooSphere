using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.ActivityManager;
using NooSphere.ActivitySystem.Events;
using System.Threading;

namespace NooSphere.ActivitySystem.FileServer
{
    public class FileStore
    {
        #region Events
        public event FileAddedHandler FileAdded;
        public event FileChangedHandler FileChanged;
        public event FileRemovedHandler FileRemoved;
        public event FileDownloadedHandler FileDownloadedFromCloud;
        #endregion

        #region Properties
        public string BasePath { get; set; }    //local file server folder
        #endregion

        #region Private Members
        private Dictionary<Guid, Resource> files = new Dictionary<Guid, Resource>();
        #endregion

        #region Public Methods
        public FileStore(string path)
        {
            this.BasePath = path;
        }
        public void AddFile(Resource resource, byte[] fileInBytes)
        {
            Thread t = new Thread(() =>
            {
                UploadFile(fileInBytes, resource);
                files.Add(resource.Id, resource); ;
                if (FileAdded != null)
                    FileAdded(this, new FileEventArgs(resource));
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
        }
        public byte[] UploadToStream(Resource resource)
        { 
            FileInfo fi = new FileInfo(BasePath+ resource.RelativePath);
            byte[] buffer = new byte[fi.Length];

            using (FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                fs.Read(buffer, 0, (int)fs.Length);

            return buffer;
        }
        public void DownloadToFile(Resource resource, byte[] byteStream)
        {
            Thread t = new Thread(() =>
            {
                string AbsolutePath = Path.Combine(BasePath, resource.RelativePath);
                FileStream fs = new FileStream(AbsolutePath, FileMode.Create);
                fs.Write(byteStream, 0, resource.Size);
                fs.Close();

                File.SetCreationTimeUtc(AbsolutePath, DateTime.Parse(resource.CreationTime));
                File.SetLastWriteTimeUtc(AbsolutePath, DateTime.Parse(resource.LastWriteTime));

                if (FileDownloadedFromCloud != null)
                    FileDownloadedFromCloud(this, new FileEventArgs(resource));
            });
            t.IsBackground = true;
            t.Start();
        }
        public void Updatefile(Resource resource, byte[] fileInBytes)
        {
            Thread t = new Thread(() =>
            {
                files[resource.Id] = resource;
                UploadFile(fileInBytes,resource);
                if (FileChanged != null)
                    FileChanged(this, new FileEventArgs(resource));
            });
            t.IsBackground = true;
            t.Start();
        }
        #endregion

        #region Private Methods
        private void UploadFile(byte[] fileInBytes, Resource resource)
        {
            try
            {
                using (FileStream fileToupload = new FileStream(this.BasePath + resource.RelativePath, FileMode.Create))
                {

                    fileToupload.Write(fileInBytes, 0, fileInBytes.Length);
                    fileToupload.Close();
                    fileToupload.Dispose();
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