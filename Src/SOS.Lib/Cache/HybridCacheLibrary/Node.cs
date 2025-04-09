namespace SOS.Lib.Cache.HybridCacheLibrary;

internal class Node<K, V>
{
    public K Key { get; set; }
    public V Value { get; set; }
    public int Frequency { get; set; }
    public Node<K, V> Prev { get; set; }
    public Node<K, V> Next { get; set; }

    public Node() { }

    public Node(K key, V value)
    {
        Key = key;
        Value = value;
        Frequency = 1;
    }
}
