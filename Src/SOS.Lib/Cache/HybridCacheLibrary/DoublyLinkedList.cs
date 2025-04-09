namespace SOS.Lib.Cache.HybridCacheLibrary;

internal class DoublyLinkedList<K, V>
{
    private Node<K, V> _head;
    private Node<K, V> _tail;
    private readonly object _lockObject = new object();

    public DoublyLinkedList()
    {
        _head = new Node<K, V>();
        _tail = new Node<K, V>();
        _head.Next = _tail;
        _tail.Prev = _head;
    }

    public void AddFirst(Node<K, V> node)
    {
        lock (_lockObject)
        {
            node.Next = _head.Next;
            node.Prev = _head;
            _head.Next.Prev = node;
            _head.Next = node;
        }
    }

    public void Remove(Node<K, V> node)
    {
        lock (_lockObject)
        {
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
        }
    }

    public Node<K, V> RemoveLast()
    {
        lock (_lockObject)
        {
            if (IsEmpty())
            {
                return null;
            }

            var node = _tail.Prev;
            Remove(node);
            return node;
        }
    }

    public bool IsEmpty()
    {
        return _head.Next == _tail;
    }
}
