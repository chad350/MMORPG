using System;
using System.Collections.Generic;

namespace DummyClient.Session
{
    public class SessionManager
    {
        public static SessionManager Instance { get; } = new SessionManager();

        private HashSet<ServerSession> _sessions = new HashSet<ServerSession>();
        private object _lock = new object();
        private int _dummyId = 1;

        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                session.DummyId = _dummyId;
                _dummyId++;

                _sessions.Add(session);
                Console.WriteLine($"Connected - {_sessions.Count} Plauyers");
                return session;
            }
        }

        public void Remove(ServerSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
                Console.WriteLine($"Connected - {_sessions.Count} Plauyers");
            }
        }
    }
}