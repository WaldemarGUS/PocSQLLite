// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.CacheControllerBehavior
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace Microsoft.Synchronization.ClientServices {
    public class CacheControllerBehavior
  {
    private object _lockObject = new object();
    private bool _locked;
    private List<Type> _knownTypes;
    private ICredentials _credentials;
    private SerializationFormat _serFormat;
    private string _scopeName;
    private Action<HttpWebRequest, Action<HttpWebRequest>> _beforeSendingRequestHandler;
    private Action<HttpWebResponse> _afterSendingResponse;
    private Dictionary<string, string> _scopeParameters;

    public ReadOnlyCollection<Type> KnownTypes
    {
      get
      {
        return new ReadOnlyCollection<Type>((IList<Type>) this._knownTypes);
      }
    }

    public Dictionary<string, string>.Enumerator ScopeParameters
    {
      get
      {
        return this._scopeParameters.GetEnumerator();
      }
    }

    public ICredentials Credentials
    {
      get
      {
        return this._credentials;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException(nameof (value));
        lock (this._lockObject)
        {
          this.CheckLockState();
          this._credentials = value;
        }
      }
    }

    public SerializationFormat SerializationFormat
    {
      get
      {
        return this._serFormat;
      }
      set
      {
        lock (this._lockObject)
        {
          this.CheckLockState();
          this._serFormat = value;
        }
      }
    }

    public string ScopeName
    {
      get
      {
        return this._scopeName;
      }
      internal set
      {
        this._scopeName = value;
      }
    }

    public Action<HttpWebRequest, Action<HttpWebRequest>> BeforeSendingRequest
    {
      get
      {
        return this._beforeSendingRequestHandler;
      }
      set
      {
        lock (this._lockObject)
        {
          this.CheckLockState();
          this._beforeSendingRequestHandler = value;
        }
      }
    }

    public Action<HttpWebResponse> AfterReceivingResponse
    {
      get
      {
        return this._afterSendingResponse;
      }
      set
      {
        lock (this._lockObject)
        {
          this.CheckLockState();
          this._afterSendingResponse = value;
        }
      }
    }

    public void AddType<T>() where T : IOfflineEntity
    {
      lock (this._lockObject)
      {
        this.CheckLockState();
        this._knownTypes.Add(typeof (T));
      }
    }

    public void AddScopeParameters(string key, string value)
    {
      if (key == null)
        throw new ArgumentNullException(nameof (key));
      if (string.IsNullOrEmpty(key))
        throw new ArgumentException("key cannot be empty", nameof (key));
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      lock (this._lockObject)
      {
        this.CheckLockState();
        this._scopeParameters.Add(key, value);
      }
    }

    private void CheckLockState()
    {
      if (this._locked)
        throw new CacheControllerException("Cannot modify CacheControllerBehavior when sync is in progress.");
    }

    internal Dictionary<string, string> ScopeParametersInternal
    {
      get
      {
        return this._scopeParameters;
      }
    }

    internal bool Locked
    {
      set
      {
        lock (this._lockObject)
          this._locked = value;
      }
    }

    internal CacheControllerBehavior()
    {
      this._knownTypes = new List<Type>();
      this._serFormat = SerializationFormat.ODataAtom;
      this._scopeParameters = new Dictionary<string, string>();
    }
  }
}
