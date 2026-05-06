using BManagedClient.BMsrv;
using System;
using System.ServiceModel;

namespace BManagedClient
{
    public static class ServiceGateway
    {
        // Performance fix (May 2026): every action used to open + close a fresh
        // Service1Client, paying TCP handshake + WCF channel-open overhead per
        // call. Pages that call several SOAP ops back-to-back were burning
        // hundreds of ms on plumbing alone. We now keep ONE long-lived client
        // and only rebuild it if it faults.
        private static Service1Client _shared;
        private static readonly object _lock = new object();

        private static Service1Client GetClient()
        {
            lock (_lock)
            {
                if (_shared == null
                    || _shared.State == CommunicationState.Faulted
                    || _shared.State == CommunicationState.Closed
                    || _shared.State == CommunicationState.Closing)
                {
                    if (_shared != null)
                    { try { _shared.Abort(); } catch { } _shared = null; }
                    _shared = new Service1Client();
                }
                return _shared;
            }
        }

        public static T Use<T>(Func<Service1Client, T> action)
        {
            try { return action(GetClient()); }
            catch
            {
                // Force a rebuild on the next call so a transient channel fault
                // doesn't permanently break the app.
                lock (_lock)
                {
                    if (_shared != null) { try { _shared.Abort(); } catch { } }
                    _shared = null;
                }
                throw;
            }
        }

        public static void Use(Action<Service1Client> action)
        {
            Use(client => { action(client); return true; });
        }

        // Optional: open the channel ahead of time so the first user-visible
        // call doesn't pay the cold-start cost. Call this once after login.
        public static void Warm()
        {
            try
            {
                var c = GetClient();
                if (c.State == CommunicationState.Created) c.Open();
            }
            catch { /* best-effort */ }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                if (_shared != null) { try { _shared.Abort(); } catch { } }
                _shared = null;
            }
        }
    }

    /// <summary>
    /// Holds the currently signed-in user's identity + role for the WPF session.
    /// Roles in B-Managed: Owner, Employee, Client.
    /// </summary>
    public static class ClientSession
    {
        public static int CurrentUserId => LogIn.sign?.Id ?? -1;
        public static string Role => LogIn.sign?.Role ?? "";
        public static bool IsOwner    => Role == "Owner";
        public static bool IsEmployee => Role == "Employee";
        public static bool IsClient   => Role == "Client";
        public static bool IsLoggedIn => CurrentUserId > 0 && !string.IsNullOrEmpty(Role);
    }
}
