// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.CacheController
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Synchronization.ClientServices {
    public class CacheController {
        private object lockObject = new object();
        private OfflineSyncProvider localProvider;
        private Uri serviceUri;
        private CacheControllerBehavior controllerBehavior;
        private HttpCacheRequestHandler cacheRequestHandler;
        private Guid changeSetId;
        private bool beginSessionComplete;

        public static void DebugMemory(string categoryName) {
        }

        public CacheControllerBehavior ControllerBehavior {
            get {
                return this.controllerBehavior;
            }
        }

        public CacheController(Uri serviceUri, string scopeName, OfflineSyncProvider localProvider) {
            if (serviceUri == (Uri)null)
                throw new ArgumentNullException(nameof(serviceUri));
            if (string.IsNullOrEmpty(scopeName))
                throw new ArgumentNullException(nameof(scopeName));
            if (!serviceUri.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase) && !serviceUri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase))
                throw new ArgumentException("Uri must be http or https schema", nameof(serviceUri));
            if (localProvider == null)
                throw new ArgumentNullException(nameof(localProvider));
            this.serviceUri = serviceUri;
            this.localProvider = localProvider;
            this.controllerBehavior = new CacheControllerBehavior();
            this.controllerBehavior.ScopeName = scopeName;
        }

        internal async Task<CacheRefreshStatistics> SynchronizeAsync() {
            CacheRefreshStatistics refreshStatistics = await this.SynchronizeAsync(CancellationToken.None);
            return refreshStatistics;
        }

        internal async Task<CacheRefreshStatistics> SynchronizeAsync(
          CancellationToken cancellationToken) {
            CacheRefreshStatistics statistics = new CacheRefreshStatistics();
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();
            statistics.StartTime = DateTime.Now;
            try {
                this.cacheRequestHandler = new HttpCacheRequestHandler(this.serviceUri, this.controllerBehavior);
                await this.localProvider.BeginSession();
                this.beginSessionComplete = true;
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                statistics = await this.EnqueueUploadRequest(statistics, cancellationToken);
                if (statistics.Error != null)
                    throw new Exception("Error occured during Upload request.", statistics.Error);
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                statistics = await this.EnqueueDownloadRequest(statistics, cancellationToken);
                statistics.EndTime = DateTime.Now;
                if (this.beginSessionComplete)
                    this.localProvider.EndSession();
            } catch (OperationCanceledException ex) {
                statistics.EndTime = DateTime.Now;
                statistics.Cancelled = true;
                statistics.Error = (Exception)ex;
                if (this.beginSessionComplete)
                    this.localProvider.EndSession();
            } catch (Exception ex) {
                statistics.EndTime = DateTime.Now;
                statistics.Error = ex;
                if (this.beginSessionComplete)
                    this.localProvider.EndSession();
            } finally {
                this.ResetAsyncWorkerManager();
            }
            return statistics;
        }

        private void ResetAsyncWorkerManager() {
            lock (this.lockObject) {
                this.cacheRequestHandler = (HttpCacheRequestHandler)null;
                this.controllerBehavior.Locked = false;
                this.beginSessionComplete = false;
            }
        }

        private async Task<CacheRefreshStatistics> EnqueueUploadRequest(
          CacheRefreshStatistics statistics,
          CancellationToken cancellationToken) {
            this.changeSetId = Guid.NewGuid();
            try {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                DateTime durationStartDate = DateTime.Now;
                ChangeSet changeSet = await this.localProvider.GetChangeSet(this.changeSetId);
                if (changeSet == null || changeSet.Data == null || changeSet.Data.Count == 0)
                    return statistics;
                CacheRequest request = new CacheRequest() {
                    RequestId = this.changeSetId,
                    Format = this.ControllerBehavior.SerializationFormat,
                    RequestType = CacheRequestType.UploadChanges,
                    Changes = changeSet.Data,
                    KnowledgeBlob = changeSet.ServerBlob,
                    IsLastBatch = changeSet.IsLastBatch
                };
                durationStartDate = DateTime.Now;
                CacheRequestResult requestResult = await this.cacheRequestHandler.ProcessCacheRequestAsync(request, (object)changeSet.IsLastBatch, cancellationToken);
                CacheRefreshStatistics refreshStatistics = await this.ProcessCacheRequestResults(statistics, requestResult, cancellationToken);
                statistics = refreshStatistics;
                refreshStatistics = (CacheRefreshStatistics)null;
                changeSet = (ChangeSet)null;
                request = (CacheRequest)null;
                requestResult = (CacheRequestResult)null;
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                if (ExceptionUtility.IsFatal(ex))
                    throw;
                else
                    statistics.Error = ex;
            }
            return statistics;
        }

        private async Task<CacheRefreshStatistics> EnqueueDownloadRequest(
          CacheRefreshStatistics statistics,
          CancellationToken cancellationToken) {
            try {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                bool isLastBatch = false;
                while (!isLastBatch) {
                    if (cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();
                    CacheRequest request = new CacheRequest() {
                        Format = this.ControllerBehavior.SerializationFormat,
                        RequestType = CacheRequestType.DownloadChanges,
                        KnowledgeBlob = this.localProvider.GetServerBlob()
                    };
                    DateTime durationStartDate = DateTime.Now;
                    CacheRequestResult requestResult = await this.cacheRequestHandler.ProcessCacheRequestAsync(request, (object)null, cancellationToken);
                    CacheRefreshStatistics refreshStatistics = await this.ProcessCacheRequestResults(statistics, requestResult, cancellationToken);
                    statistics = refreshStatistics;
                    refreshStatistics = (CacheRefreshStatistics)null;
                    if (requestResult.ChangeSet == null || requestResult.ChangeSet.IsLastBatch)
                        isLastBatch = true;
                    request = (CacheRequest)null;
                    requestResult = (CacheRequestResult)null;
                }
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                if (ExceptionUtility.IsFatal(ex))
                    throw;
                else
                    statistics.Error = ex;
            }
            return statistics;
        }

        private async Task<CacheRefreshStatistics> ProcessCacheRequestResults(
          CacheRefreshStatistics statistics,
          CacheRequestResult cacheRequestResult,
          CancellationToken cancellationToken) {
            try {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                if (cacheRequestResult.Error != null) {
                    if (cacheRequestResult.ChangeSetResponse != null && cacheRequestResult.HttpStep == HttpState.End)
                        await this.localProvider.OnChangeSetUploaded(cacheRequestResult.Id, cacheRequestResult.ChangeSetResponse);
                    statistics.Error = cacheRequestResult.Error;
                    return statistics;
                }
                if (cacheRequestResult.ChangeSetResponse != null) {
                    if (cacheRequestResult.ChangeSetResponse.Error == null && cacheRequestResult.HttpStep == HttpState.End)
                        await this.localProvider.OnChangeSetUploaded(cacheRequestResult.Id, cacheRequestResult.ChangeSetResponse);
                    if (cacheRequestResult.ChangeSetResponse.Error != null) {
                        statistics.Error = cacheRequestResult.ChangeSetResponse.Error;
                        return statistics;
                    }
                    ++statistics.TotalChangeSetsUploaded;
                    statistics.TotalUploads += cacheRequestResult.BatchUploadCount;
                    foreach (Conflict conflict in cacheRequestResult.ChangeSetResponse.ConflictsInternal) {
                        Conflict e1 = conflict;
                        if (e1 is SyncConflict)
                            ++statistics.TotalSyncConflicts;
                        else
                            ++statistics.TotalSyncErrors;
                        e1 = (Conflict)null;
                    }
                    return statistics;
                }
                Debug.Assert(cacheRequestResult.ChangeSet != null, "Completion is not for a download request.");
                if (cacheRequestResult.ChangeSet != null && cacheRequestResult.ChangeSet.Data != null && cacheRequestResult.ChangeSet.Data.Count > 0) {
                    ++statistics.TotalChangeSetsDownloaded;
                    statistics.TotalDownloads += (uint)cacheRequestResult.ChangeSet.Data.Count;
                    await this.localProvider.SaveChangeSet(cacheRequestResult.ChangeSet);
                }
                return statistics;
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                if (ExceptionUtility.IsFatal(ex))
                    throw;
                else
                    statistics.Error = ex;
            }
            return statistics;
        }
    }
}
