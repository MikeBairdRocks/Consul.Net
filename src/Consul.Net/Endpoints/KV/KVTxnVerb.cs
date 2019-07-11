using System;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.KV
{
  [JsonConverter(typeof(KVTxnVerbTypeConverter))]
  public class KVTxnVerb : IEquatable<KVTxnVerb>
  {
    private static readonly KVTxnVerb kvSetOp = new KVTxnVerb() {Operation = "set"};
    private static readonly KVTxnVerb kvDeleteOp = new KVTxnVerb() {Operation = "delete"};
    private static readonly KVTxnVerb kvDeleteCASOp = new KVTxnVerb() {Operation = "delete-cas"};
    private static readonly KVTxnVerb kvDeleteTreeOp = new KVTxnVerb() {Operation = "delete-tree"};
    private static readonly KVTxnVerb kvCASOp = new KVTxnVerb() {Operation = "cas"};
    private static readonly KVTxnVerb kvLockOp = new KVTxnVerb() {Operation = "lock"};
    private static readonly KVTxnVerb kvUnlockOp = new KVTxnVerb() {Operation = "unlock"};
    private static readonly KVTxnVerb kvGetOp = new KVTxnVerb() {Operation = "get"};
    private static readonly KVTxnVerb kvGetTreeOp = new KVTxnVerb() {Operation = "get-tree"};
    private static readonly KVTxnVerb kvCheckSessionOp = new KVTxnVerb() {Operation = "check-session"};
    private static readonly KVTxnVerb kvCheckIndexOp = new KVTxnVerb() {Operation = "check-index"};

    public static KVTxnVerb Set
    {
      get { return kvSetOp; }
    }

    public static KVTxnVerb Delete
    {
      get { return kvDeleteOp; }
    }

    public static KVTxnVerb DeleteCAS
    {
      get { return kvDeleteCASOp; }
    }

    public static KVTxnVerb DeleteTree
    {
      get { return kvDeleteTreeOp; }
    }

    public static KVTxnVerb CAS
    {
      get { return kvCASOp; }
    }

    public static KVTxnVerb Lock
    {
      get { return kvLockOp; }
    }

    public static KVTxnVerb Unlock
    {
      get { return kvUnlockOp; }
    }

    public static KVTxnVerb Get
    {
      get { return kvGetOp; }
    }

    public static KVTxnVerb GetTree
    {
      get { return kvGetTreeOp; }
    }

    public static KVTxnVerb CheckSession
    {
      get { return kvCheckSessionOp; }
    }

    public static KVTxnVerb CheckIndex
    {
      get { return kvCheckIndexOp; }
    }

    public string Operation { get; private set; }

    public bool Equals(KVTxnVerb other)
    {
      return Operation == other.Operation;
    }

    public override bool Equals(object other)
    {
      // other could be a reference type, the is operator will return false if null
      return other is KVTxnVerb && Equals(other as KVTxnVerb);
    }

    public override int GetHashCode()
    {
      return Operation.GetHashCode();
    }
  }
}