using System;

namespace Consul.Net.Endpoints.Session
{
  public class SessionBehavior : IEquatable<SessionBehavior>
  {
    private static readonly SessionBehavior release = new SessionBehavior() { Behavior = "release" };
    private static readonly SessionBehavior delete = new SessionBehavior() { Behavior = "delete" };

    public string Behavior { get; private set; }

    public static SessionBehavior Release
    {
      get { return release; }
    }

    public static SessionBehavior Delete
    {
      get { return delete; }
    }

    public bool Equals(SessionBehavior other)
    {
      return other != null && Behavior.Equals(other.Behavior);
    }

    public override bool Equals(object other)
    {
      // other could be a reference type, the is operator will return false if null
      var a = other as SessionBehavior;
      return a != null && Equals(a);
    }

    public override int GetHashCode()
    {
      return Behavior.GetHashCode();
    }
  }
}