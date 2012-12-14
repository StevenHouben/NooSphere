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
#if ANDROID
using Microsoft.Http;
#else
using System.Net.Http;
#endif
using System.Threading.Tasks;
using NooSphere.ActivitySystem.Helpers;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Base;

namespace NooSphere.ActivitySystem.FileServer
{
    public class FileStore:IFileStore
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
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Dictionary<Guid, Resource> _files = new Dictionary<Guid, Resource>();
        private readonly object _lookUpLock = new object();
        private readonly object _fileLock = new object();

        #endregion

        #region Public Methods
        public FileStore(string path)
        {
            BasePath = path;
        }
        public void AddFile(Resource resource, byte[] fileInBytes,FileSource source)
        {
            //Check if we have a valid file
            Check(resource, fileInBytes);

            //See if we have file 
            if (_files.ContainsKey(resource.Id))
            {
                if (IsNewer(_files[resource.Id], resource))
                    UpdateFile(resource, fileInBytes, source);
                else return;
            }
            else
            {
                SaveToDisk(fileInBytes, resource);
                _files.Add(resource.Id, resource);
            }
                    
            //Check what the source is and who we should inform
            switch (source)
            {
                case FileSource.ActivityCloud:
                    if (FileDownloadedFromCloud != null)
                        FileDownloadedFromCloud(this, new FileEventArgs(resource));
                    break;
                case FileSource.ActivityManager:
                    if (FileAdded != null)
                        FileAdded(this, new FileEventArgs(resource));
                    break;
                case FileSource.ActivityClient:
                    if (FileCopied != null)
                        FileCopied(this, new FileEventArgs(resource));
                    break; 
            }
            Log.Out("FileStore", string.Format("Added file {0} to store", resource.Name), LogCode.Log);
        }   
        public void DownloadFile(Resource resource, string path, FileSource source, string _connectionId = null)
        {
            Rest.DownloadStream(path, _connectionId).ContinueWith(stream =>
            {
                Log.Out("FileStore", string.Format("Finished download for {0}", resource.Name), LogCode.Log);
                AddFile(resource, stream.Result, source);
            });
            Log.Out("FileStore", string.Format("Started download for {0}", resource.Name), LogCode.Log);
        }
        private bool IsNewer(Resource resourceInFileStore, Resource requestedResource)
        {
            return false;
        }
        public void AddFile(Resource resource, Stream stream, FileSource source)
        {
            AddFile(resource, GetBytesFromStream(resource, stream), source);
        }
        public void UpdateFile(Resource resource,Stream stream,FileSource source)
        {
            UpdateFile(resource, GetBytesFromStream(resource, stream), source);
        }
        public void UpdateFile(Resource resource, byte[] fileInBytes, FileSource source)
        {
            SaveToDisk(fileInBytes, resource);
            _files[resource.Id] = resource;

            switch (source)
            {
                case FileSource.ActivityCloud:
                    if (FileDownloadedFromCloud != null)
                        FileDownloadedFromCloud(this, new FileEventArgs(resource));
                    break;
                case FileSource.ActivityManager:
                    if (FileChanged != null)
                        FileChanged(this, new FileEventArgs(resource));
                    break;
            }
            Log.Out("FileStore", string.Format("Updated file {0} to store", resource.Name), LogCode.Log);
        }
        public void RemoveFile(Resource resource)
        {
            _files.Remove(resource.Id);
            File.Delete(BasePath+resource.RelativePath);
            if (FileRemoved != null)
                FileRemoved(this, new FileEventArgs(resource));
            Log.Out("FileStore", string.Format("FileStore: Removed file {0} from store", resource.Name), LogCode.Log);
        }
        public bool LookUp(Guid id)
        {
            lock(_lookUpLock) 
                return _files.ContainsKey(id);
        }
        public Stream GetStreamFromFile(Resource resource)
        {
            lock(_fileLock)
                return new FileStream(BasePath + resource.RelativePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        public byte[] GetBytesFromFile(Resource resource)
        {
            var fi = new FileInfo(BasePath + resource.RelativePath);
            var buffer = new byte[fi.Length];
            
            lock (_fileLock)
                using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    fs.Read(buffer, 0, (int)fs.Length);

            return buffer;
        }
        public void Updatefile(Resource resource, byte[] fileInBytes)
        {
            Task.Factory.StartNew(
                   delegate
                   {
                        _files[resource.Id] = resource;
                        SaveToDisk(fileInBytes, resource);
                        if (FileChanged != null)
                            FileChanged(this, new FileEventArgs(resource));
                        Log.Out("FileStore", string.Format("FileStore: Updated file {0} in store", resource.Name), LogCode.Log);
                    });
        }

        /// <summary>
        /// Initializes a directory for future file saving. It uses the implicitly
        /// called ToString() method to convert the object to a path
        /// </summary>
        /// <param name="relative"> </param>
        public void IntializePath(object relative)
        {
            var path = Path.Combine(BasePath, relative.ToString());
            //In case the activity path does not exist yet, we'll create one
            if (!Directory.Exists(path))
            {
                var dInfo = Directory.CreateDirectory(path);
            }
        }
        public void CleanUp(string path)
        {
            if(Directory.Exists(Path.Combine(BasePath,path)))
                Directory.Delete(Path.Combine(BasePath, path), true);
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
            var path = Path.Combine(BasePath, resource.RelativePath);
            var dir = Path.GetDirectoryName(path);
            if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            lock (_fileLock)
            {
                if(!File.Exists(@path))
                {
                    using (var fileToupload = new FileStream(@path, FileMode.OpenOrCreate))
                    {
                        fileToupload.Write(fileInBytes, 0, fileInBytes.Length);
                        fileToupload.Close();
                        fileToupload.Dispose();

                        //File.SetCreationTimeUtc(path, DateTime.Parse(resource.CreationTime));
                        //File.SetLastWriteTimeUtc(path, DateTime.Parse(resource.LastWriteTime));
                        Console.WriteLine("FileStore: Saved file {0} to disk at {1}", resource.Name,
                                          path);
                        Log.Out("FileStore", string.Format("FileStore: Saved file {0} to disk at {1}", resource.Name,
                                          path), LogCode.Log);
                    }
                }
                else
                    Log.Out("FileStore", string.Format("FileStore: file {0} already in store", resource.Name,
                                          path), LogCode.Log);

            }
        }
        #endregion
    }
    internal class DownloadState
    {
        public Resource Resource { get; set; }
        public FileSource FileSource { get; set; }

        public DownloadState(Resource resource, FileSource fileSource)
        {
            Resource = resource;
            FileSource = fileSource;
        }
    }
}