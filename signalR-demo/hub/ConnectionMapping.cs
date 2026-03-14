namespace signalR_demo.hub;

public class ConnectionMapping<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<string>?> _connections = new();

    public int Count
    {
        get
        {
            lock (_connections)
            {
                return _connections.Count;
            }
        }
    }

    public void Add(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
            {
                connections = [];
                _connections.Add(key, connections);
            }

            connections ??= [];
            lock (connections)
            {
                connections.Add(connectionId);
            }
        }
    }

    public IEnumerable<string> GetConnections(T key)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(key, out var connections))
            {
                return connections ?? [];
            }
        }

        return [];
    }
    
    public IEnumerable<string> GetConnections()
    {
        lock (_connections)
        {
            return _connections.Values.SelectMany(x => x).ToList();
        }
    }

    public void Remove(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
            {
                return;
            }

            if (connections == null || connections.Count == 0) return;
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _connections.Remove(key);
                }
            }
        }
    }
}