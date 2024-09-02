namespace SOS.Lib.Models.Cache;
public class CacheEntry<T>
{
    public byte[] Data { get; set; }
    public int Count { get; set; }  
}