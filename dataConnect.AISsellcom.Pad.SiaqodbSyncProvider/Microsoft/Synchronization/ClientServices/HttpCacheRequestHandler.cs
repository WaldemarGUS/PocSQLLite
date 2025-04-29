// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.HttpCacheRequestHandler
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.Services.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    internal class HttpCacheRequestHandler {
        private readonly Uri baseUri;
        private readonly ICredentials credentials;
        private readonly Type[] knownTypes;
        private readonly string scopeName;
        private readonly Dictionary<string, string> scopeParameters;
        private readonly SerializationFormat serializationFormat;

        public HttpCacheRequestHandler(Uri serviceUri, CacheControllerBehavior behaviors) {
            this.baseUri = serviceUri;
            this.serializationFormat = behaviors.SerializationFormat;
            this.scopeName = behaviors.ScopeName;
            this.credentials = behaviors.Credentials;
            this.knownTypes = new Type[behaviors.KnownTypes.Count];
            behaviors.KnownTypes.CopyTo(this.knownTypes, 0);
            this.scopeParameters = new Dictionary<string, string>((IDictionary<string, string>)behaviors.ScopeParametersInternal);
        }

        protected SerializationFormat SerializationFormat {
            get {
                return this.serializationFormat;
            }
        }

        protected string ScopeName {
            get {
                return this.scopeName;
            }
        }

        protected Uri BaseUri {
            get {
                return this.baseUri;
            }
        }

        public async Task<CacheRequestResult> ProcessCacheRequestAsync(
          CacheRequest request,
          object state,
          CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();
            HttpCacheRequestHandler.AsyncArgsWrapper wrapper = new HttpCacheRequestHandler.AsyncArgsWrapper() {
                UserPassedState = state,
                CacheRequest = request,
                Step = HttpState.Start
            };
            wrapper = await this.ProcessRequest(wrapper, cancellationToken);
            CacheRequestResult cacheRequestResult = wrapper.CacheRequest.RequestType != CacheRequestType.UploadChanges ? new CacheRequestResult(wrapper.CacheRequest.RequestId, wrapper.DownloadResponse, wrapper.Error, wrapper.Step, wrapper.UserPassedState) : new CacheRequestResult(wrapper.CacheRequest.RequestId, wrapper.UploadResponse, wrapper.CacheRequest.Changes.Count, wrapper.Error, wrapper.Step, wrapper.UserPassedState);
            return cacheRequestResult;
        }

        private async Task<HttpCacheRequestHandler.AsyncArgsWrapper> ProcessRequest(
          HttpCacheRequestHandler.AsyncArgsWrapper wrapper,
          CancellationToken cancellationToken) {
            HttpWebResponse webResponse1 = (HttpWebResponse)null;
            try {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                StringBuilder requestUri = new StringBuilder();
                requestUri.AppendFormat("{0}{1}{2}/{3}", (object)this.BaseUri, this.BaseUri.ToString().EndsWith("/") ? (object)string.Empty : (object)"/", (object)Uri.EscapeUriString(this.ScopeName), (object)wrapper.CacheRequest.RequestType.ToString());
                string prefix = "?";
                foreach (KeyValuePair<string, string> scopeParameter in this.scopeParameters) {
                    KeyValuePair<string, string> kvp = scopeParameter;
                    requestUri.AppendFormat("{0}{1}={2}", (object)prefix, (object)Uri.EscapeUriString(kvp.Key), (object)Uri.EscapeUriString(kvp.Value));
                    if (prefix.Equals("?"))
                        prefix = "&";
                    kvp = new KeyValuePair<string, string>();
                }
                HttpWebRequest webRequest;
                if (this.credentials != null) {
                    webRequest = WebRequest.CreateHttp(new Uri(requestUri.ToString()));
                    webRequest.Credentials = this.credentials;
                } else
                    webRequest = (HttpWebRequest)WebRequest.Create(requestUri.ToString());
                webRequest.Method = "POST";
                webRequest.Accept = this.SerializationFormat == SerializationFormat.ODataAtom ? "application/atom+xml" : "application/json";
                webRequest.ContentType = this.SerializationFormat == SerializationFormat.ODataAtom ? "application/atom+xml" : "application/json";
                // Beginn Änderung druch dC
                webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
                webRequest.AutomaticDecompression = DecompressionMethods.GZip;
                webRequest.Timeout = 6000000;
                webRequest.ReadWriteTimeout = 6000000;
                webRequest.ServicePoint.ConnectionLimit = 100;
                // Ende Änderung druch dC

                wrapper.Step = HttpState.WriteRequest;
                using (Stream stream = await Task.Factory.FromAsync<Stream>(new Func<AsyncCallback, object, IAsyncResult>(((WebRequest)webRequest).BeginGetRequestStream), new Func<IAsyncResult, Stream>(((WebRequest)webRequest).EndGetRequestStream), (object)null)) {
                    if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
                        this.WriteUploadRequestStream(stream, wrapper);
                    else
                        this.WriteDownloadRequestStream(stream, wrapper);
                }
                if (wrapper.Error != null)
                    return wrapper;
                if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
                    wrapper.UploadResponse = new ChangeSetResponse();
                else
                    wrapper.DownloadResponse = new ChangeSet();
                wrapper.Step = HttpState.ReadResponse;
                WebResponse webResponse2 = await Task.Factory.FromAsync<WebResponse>(new Func<AsyncCallback, object, IAsyncResult>(((WebRequest)webRequest).BeginGetResponse), new Func<IAsyncResult, WebResponse>(((WebRequest)webRequest).EndGetResponse), (object)null);
                webResponse1 = (HttpWebResponse)webResponse2;
                webResponse2 = (WebResponse)null;
                if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
                    await this.ReadUploadResponse(webResponse1, wrapper);
                else
                    await this.ReadDownloadResponse(webResponse1, wrapper);
                if (webResponse1 != null) {
                    wrapper.Step = HttpState.End;
                    webResponse1.Dispose();
                    webResponse1 = (HttpWebResponse)null;
                }
                requestUri = (StringBuilder)null;
                prefix = (string)null;
                webRequest = (HttpWebRequest)null;
            } catch (WebException webException) {
                if (webException.Response == null) {
                    wrapper.Error = (Exception)webException;
                } else {
                    using (Stream responseStream = webException.Response.GetResponseStream()) {
                        using (XmlReader reader = this.SerializationFormat == SerializationFormat.ODataAtom ? XmlReader.Create(responseStream) : (XmlReader)new XmlJsonReader(responseStream, XmlDictionaryReaderQuotas.Max)) {
                            // Beginn change durch dC - dataConnect
                            try {
                                if (reader.ReadToDescendant("ErrorDescription")) {
                                    wrapper.Error = new Exception(reader.ReadElementContentAsString());
                                } else {
                                    wrapper.Error = new Exception($"Content: {reader.ReadContentAsString()}");
                                }
                            } catch (Exception) {
                                string errorMSG = "";

                                using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8)) {
                                    errorMSG = readStream.ReadToEnd();
                                }
                                wrapper.Error = new Exception($"Response-Content: {errorMSG}", webException);
                                throw wrapper.Error;
                            }
                            // Ende Change
                        }
                    }
                }
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                if (ExceptionUtility.IsFatal(ex)) {
                    throw;
                } else {
                    wrapper.Error = ex;
                    return wrapper;
                }
            }
            return wrapper;
        }

        private void WriteUploadRequestStream(
          Stream requestStream,
          HttpCacheRequestHandler.AsyncArgsWrapper wrapper) {
            try {
                SyncWriter syncWriter = this.SerializationFormat == SerializationFormat.ODataAtom ? (SyncWriter)new ODataAtomWriter(this.BaseUri) : (SyncWriter)new ODataJsonWriter(this.BaseUri);
                syncWriter.StartFeed(wrapper.CacheRequest.IsLastBatch, wrapper.CacheRequest.KnowledgeBlob ?? new byte[0]);
                foreach (IOfflineEntity change in (IEnumerable<IOfflineEntity>)wrapper.CacheRequest.Changes) {
                    if (change != null && (!change.ServiceMetadata.IsTombstone || !string.IsNullOrEmpty(change.ServiceMetadata.Id))) {
                        string str = (string)null;
                        if (string.IsNullOrEmpty(change.ServiceMetadata.Id)) {
                            if (wrapper.TempIdToEntityMapping == null)
                                wrapper.TempIdToEntityMapping = new Dictionary<string, IOfflineEntity>();
                            str = Guid.NewGuid().ToString();
                            wrapper.TempIdToEntityMapping.Add(str, change);
                        }
                        syncWriter.AddItem(change, str);
                    }
                }
                if (this.SerializationFormat == SerializationFormat.ODataAtom)
                    syncWriter.WriteFeed(XmlWriter.Create(requestStream));
                else
                    syncWriter.WriteFeed((XmlWriter)new XmlJsonWriter(requestStream));
                requestStream.Flush();
            } catch (Exception ex) {
                if (ExceptionUtility.IsFatal(ex))
                    throw;
                else
                    wrapper.Error = ex;
            }
        }

        private void WriteDownloadRequestStream(
          Stream requestStream,
          HttpCacheRequestHandler.AsyncArgsWrapper wrapper) {
            try {
                SyncWriter syncWriter = this.SerializationFormat == SerializationFormat.ODataAtom ? (SyncWriter)new ODataAtomWriter(this.BaseUri) : (SyncWriter)new ODataJsonWriter(this.BaseUri);
                syncWriter.StartFeed(wrapper.CacheRequest.IsLastBatch, wrapper.CacheRequest.KnowledgeBlob ?? new byte[0]);
                if (this.SerializationFormat == SerializationFormat.ODataAtom)
                    syncWriter.WriteFeed(XmlWriter.Create(requestStream));
                else
                    syncWriter.WriteFeed((XmlWriter)new XmlJsonWriter(requestStream));
                requestStream.Flush();
            } catch (Exception ex) {
                if (ExceptionUtility.IsFatal(ex))
                    throw;
                else
                    wrapper.Error = ex;
            }
        }

        private async Task ReadUploadResponse(
          HttpWebResponse response,
          HttpCacheRequestHandler.AsyncArgsWrapper wrapper) {
            try {
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (Stream responseStream = response.GetResponseStream()) {
                        using (SyncReader syncReader = this.SerializationFormat == SerializationFormat.ODataAtom ? (SyncReader)new ODataAtomReader(responseStream, this.knownTypes) : (SyncReader)new ODataJsonReader(responseStream, this.knownTypes)) {
                            await Task.Factory.StartNew((Action)(() => {
                                while (syncReader.Next()) {
                                    switch (syncReader.ItemType) {
                                        case ReaderItemType.Entry:
                                            IOfflineEntity entity1 = syncReader.GetItem();
                                            IOfflineEntity offlineEntity = entity1;
                                            string tempId = (string)null;
                                            if (syncReader.HasTempId() && syncReader.HasConflictTempId())
                                                throw new CacheControllerException(string.Format("Service returned a TempId '{0}' in both live and conflicting entities.", (object)syncReader.GetTempId()));
                                            if (syncReader.HasTempId()) {
                                                tempId = syncReader.GetTempId();
                                                this.CheckEntityServiceMetadataAndTempIds(wrapper, entity1, tempId);
                                            }
                                            if (syncReader.HasConflict()) {
                                                Conflict conflict = syncReader.GetConflict();
                                                IOfflineEntity entity2 = conflict is SyncConflict ? ((SyncConflict)conflict).LosingEntity : ((SyncError)conflict).ErrorEntity;
                                                if (syncReader.HasConflictTempId()) {
                                                    tempId = syncReader.GetConflictTempId();
                                                    this.CheckEntityServiceMetadataAndTempIds(wrapper, entity2, tempId);
                                                }
                                                wrapper.UploadResponse.AddConflict(conflict);
                                                if (syncReader.HasConflictTempId() && entity1.ServiceMetadata.IsTombstone) {
                                                    entity2.ServiceMetadata.IsTombstone = true;
                                                    offlineEntity = entity2;
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(tempId)) {
                                                wrapper.UploadResponse.AddUpdatedItem(offlineEntity);
                                                break;
                                            }
                                            break;
                                        case ReaderItemType.SyncBlob:
                                            wrapper.UploadResponse.ServerBlob = syncReader.GetServerBlob();
                                            break;
                                    }
                                }
                            }));
                            if (wrapper.TempIdToEntityMapping != null && (uint)wrapper.TempIdToEntityMapping.Count > 0U) {
                                StringBuilder builder = new StringBuilder("Server did not acknowledge with a permanent Id for the following tempId's: ");
                                builder.Append(string.Join(",", wrapper.TempIdToEntityMapping.Keys.ToArray<string>()));
                                throw new CacheControllerException(builder.ToString());
                            }
                        }
                    }
                } else
                    wrapper.UploadResponse.Error = (Exception)new CacheControllerException(string.Format("Remote service returned error status. Status: {0}, Description: {1}", (object)response.StatusCode, (object)response.StatusDescription));
            } catch (Exception ex) {
                if (ExceptionUtility.IsFatal(ex))
                    throw;
                else
                    wrapper.Error = ex;
            }
        }

        private void CheckEntityServiceMetadataAndTempIds(
          HttpCacheRequestHandler.AsyncArgsWrapper wrapper,
          IOfflineEntity entity,
          string tempId) {
            if (string.IsNullOrEmpty(entity.ServiceMetadata.Id))
                throw new CacheControllerException(string.Format("Service did not return a permanent Id for tempId '{0}'", (object)tempId));
            if (entity.ServiceMetadata.IsTombstone)
                throw new CacheControllerException(string.Format("Service returned a tempId '{0}' in tombstoned entity.", (object)tempId));
            if (!wrapper.TempIdToEntityMapping.ContainsKey(tempId))
                throw new CacheControllerException("Service returned a response for a tempId which was not uploaded by the client. TempId: " + tempId);
            wrapper.TempIdToEntityMapping.Remove(tempId);
        }

        private async Task ReadDownloadResponse(
          HttpWebResponse response,
          HttpCacheRequestHandler.AsyncArgsWrapper wrapper) {
            try {
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (Stream responseStream = response.GetResponseStream()) {
                        using (SyncReader syncReader = this.SerializationFormat == SerializationFormat.ODataAtom ? (SyncReader)new ODataAtomReader(responseStream, this.knownTypes) : (SyncReader)new ODataJsonReader(responseStream, this.knownTypes))
                            await Task.Factory.StartNew((Action)(() => {
                                while (syncReader.Next()) {
                                    switch (syncReader.ItemType) {
                                        case ReaderItemType.Entry:
                                            wrapper.DownloadResponse.AddItem(syncReader.GetItem());
                                            break;
                                        case ReaderItemType.SyncBlob:
                                            wrapper.DownloadResponse.ServerBlob = syncReader.GetServerBlob();
                                            break;
                                        case ReaderItemType.HasMoreChanges:
                                            wrapper.DownloadResponse.IsLastBatch = !syncReader.GetHasMoreChangesValue();
                                            break;
                                    }
                                }
                            }));
                    }
                } else
                    wrapper.Error = (Exception)new CacheControllerException(string.Format("Remote service returned error status. Status: {0}, Description: {1}", (object)response.StatusCode, (object)response.StatusDescription));
            } catch (Exception ex) {
                if (ExceptionUtility.IsFatal(ex))
                    throw;
                else
                    wrapper.Error = ex;
            }
        }

        private class AsyncArgsWrapper {
            public CacheRequest CacheRequest;
            public ChangeSet DownloadResponse;
            public Exception Error;
            public Dictionary<string, IOfflineEntity> TempIdToEntityMapping;
            public ChangeSetResponse UploadResponse;
            public object UserPassedState;
            public HttpState Step;
        }
    }
}
