using Microsoft.Extensions.ObjectPool;

namespace SOS.Lib.Cache.HybridCacheLibrary;

internal class NodePool<K, V>
{
    private readonly ObjectPool<Node<K, V>> _nodePool;

    public NodePool()
    {
        _nodePool = new DefaultObjectPool<Node<K, V>>(new DefaultPooledObjectPolicy<Node<K, V>>());
    }

    public Node<K, V> Get(K key, V value)
    {
        var node = _nodePool.Get();
        InitializeNode(node, key, value);
        return node;
    }

    public void Return(Node<K, V> node)
    {
        if (node != null)
        {
            ResetNode(node);
            _nodePool.Return(node);
        }
    }

    private void InitializeNode(Node<K, V> node, K key, V value)
    {
        node.Key = key;
        node.Value = value;
        node.Frequency = 1;
        node.Prev = null;
        node.Next = null;
    }

    private void ResetNode(Node<K, V> node)
    {
        node.Key = default;
        node.Value = default;
        node.Frequency = 0;
        node.Prev = null;
        node.Next = null;
    }
}
