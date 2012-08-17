/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Base;
using System.Threading;
using NooSphere.Helpers;

namespace NooSphere.ActivitySystem.FileServer
{
    public class FileService:IFileStore
    {
        #region Events
        public event FileAddedHandler FileAdded;
        public event FileChangedHandler FileChanged;
        public event FileAddedHandler FileCopied;
        public event FileRemovedHandler FileRemoved;
        public event FileDownloadRequestHandler FileDownloadedFromCloud;     
        #endregion  

        #region Properties
        public string BasePath { get; set; }
        #endregion

        #region Private Members
        private readonly Dictionary<Guid, Resource> _files = new Dictionary<Guid, Resource>();
        #endregion

        #region Public Methods
        public FileService(string path)
        {
            BasePath = path;
        }
        public void AddFile(Resource resource, byte[] fileInBytes,FileSource source)
        {
            var t = new Thread(() =>
            {
                Check(resource, fileInBytes);
                SaveToDisk(fileInBytes, resource);
                if(_files.ContainsKey(resource.Id)) //check if newer!
                    UpdateFile(resource,fileInBytes,source);
                else
                    _files.Add(resource.Id, resource);

                switch (source)
                {
                    case FileSource.Cloud:
                        if (FileDownloadedFromCloud != null)
                            FileDownloadedFromCloud(this, new FileEventArgs(resource));
                        break;
                    case FileSource.Local:
                        if (FileAdded != null)
                            FileAdded(this, new FileEventArgs(resource));
                        break;
                    case FileSource.System:
                        if (FileCopied != null)
                            FileCopied(this, new FileEventArgs(resource));
                        break; 
                }
                Log.Out("FileService", string.Format("Added file {0} to store", resource.Name), LogCode.Log);
            }) {IsBackground = true};
            t.Start();
        }
        private bool isNewer(Resource resource, Resource resource2)
        {
             //return DateTime.Parse(resource.LastWriteTime) <= DateTime.Parse(resource2.LastWriteTime);
            return false; //always write
        }
        public void AddFile(Resource resource, Stream stream, FileSource source)
        {
            AddFile(resource, GetBytesFromStream(resource, stream), FileSource.Local);
        }
        public void UpdateFile(Resource resource,Stream stream,FileSource source)
        {
            UpdateFile(resource, GetBytesFromStream(resource, stream), source);
        }
        public void UpdateFile(Resource resource, byte[] fileInBytes, FileSource source)
        {
            var t = new Thread(() =>
            {
                SaveToDisk(fileInBytes, resource);
                _files[resource.Id] = resource;

                switch (source)
                {
                    case FileSource.Cloud:
                        if (FileDownloadedFromCloud != null)
                            FileDownloadedFromCloud(this, new FileEventArgs(resource));
                        break;
                    case FileSource.Local:
                        if (FileChanged != null)
                            FileChanged(this, new FileEventArgs(resource));
                        break;
                }
                Log.Out("FileService", string.Format("Updated file {0} to store", resource.Name), LogCode.Log);
            }) { IsBackground = true };
            t.Start();
        }
        public void RemoveFile(Resource resource)
        {
            _files.Remove(resource.Id);
            File.Delete(BasePath+resource.RelativePath);
            if (FileRemoved != null)
                FileRemoved(this, new FileEventArgs(resource));
            Console.WriteLine("FileStore: Removed file {0} from store", resource.Name); 
        }
        public Stream GetStreamFromFile(Resource resource)
        {
            var fi = new FileInfo(BasePath + resource.RelativePath);
            var buffer = new byte[fi.Length];
            var mem = new MemoryStream();
            using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fs.Read(buffer, 0, (int) fs.Length);
            }
            mem.Write(buffer,0,buffer.Length);
            return mem;
        }
        public byte[] GetBytesFromFile(Resource resource)
        {
            var fi = new FileInfo(BasePath + resource.RelativePath);
            var buffer = new byte[fi.Length];

            using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                fs.Read(buffer, 0, (int)fs.Length);

            return buffer;
        }
        public void Updatefile(Resource resource, byte[] fileInBytes)
        {
            var t = new Thread(() =>
            {
                _files[resource.Id] = resource;
                SaveToDisk(fileInBytes,resource);
                if (FileChanged != null)
                    FileChanged(this, new FileEventArgs(resource));
                Console.WriteLine("FileStore: Updated file {0} in store", resource.Name); 
            }) {IsBackground = true};
            t.Start();
        }
        public void IntializePath(Activity act)
        {
            if (!Directory.Exists(BasePath + act.Id))
                Directory.CreateDirectory(BasePath + act.Id);
        }
        #endregion

        #region Private Methods
        private byte[] GetBytesFromStream(Resource resource, Stream stream)
        {
            var buffer = new byte[resource.Size];
            var ms = new MemoryStream();
            int bytesRead;
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            ms.Close();
            return buffer;
        }
        private void Check(Resource resource, byte[] fileInBytes)
        {
            if (_files == null)
                throw new Exception("Filestore: Not initialized");
            if (resource == null)
                throw new Exception(("Filestore: Resource not found"));
            if (fileInBytes == null)
                throw new Exception(("Filestore: Bytearray null"));
            if (fileInBytes.Length == 0)
                throw new Exception(("Filestore: Bytearray empty"));
        }
        private void SaveToDisk(byte[] fileInBytes, Resource resource)
        {
            try
            {
                var path = Path.Combine(BasePath, resource.RelativePath);
                var dir = Path.GetDirectoryName(path);
                if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                using (var fileToupload = new FileStream(path, FileMode.Create))
                {
                    fileToupload.Write(fileInBytes, 0, fileInBytes.Length);
                    fileToupload.Close();
                    fileToupload.Dispose();

                    //File.SetCreationTimeUtc(path, DateTime.Parse(resource.CreationTime));
                    //File.SetLastWriteTimeUtc(path, DateTime.Parse(resource.LastWriteTime));
                    Console.WriteLine("FileStore: Saved file {0} to disk at {1}", resource.Name,path); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        #endregion
    }
}