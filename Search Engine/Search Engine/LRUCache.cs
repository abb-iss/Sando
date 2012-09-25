using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.SearchEngine
{
	//define the generic cache class; in our setting, K is searchCriteria, V is list<searchResult>.
	//LRUCache provides O(1) time complexcity in put() and get() operation. 
	public class LRUCache<K, V>
	{
		private int capacity; //the max size of the cacheItem
		private Dictionary<K, LinkedListNode<CacheItem<K, V>>> lruMap; //stores the <Key, LinkedListNode>
		private LinkedList<CacheItem<K, V>> lruList; // maitains the order of LRU items, lruList.first is 
		                                             // least recently used, lruList.last is most recently used

        //constructor
		public LRUCache(int capacity)
		{
			this.capacity = capacity;
			this.lruMap = new Dictionary<K, LinkedListNode<CacheItem<K, V>>>();
			this.lruList = new LinkedList<CacheItem<K,V>>();
		}
		//return the value related to key if key is in lruMap, and update its order to most recently used; 
		//return default(V) if key is not in lruMap
		public V Get(K key)
		{
			lock(lruList)
			{
				LinkedListNode<CacheItem<K, V>> node;
				//check whether the key is in lruMap,
				if(lruMap.TryGetValue(key, out node))
				{
					//update the order to most recently used
					lruList.Remove(node);
					lruList.AddLast(node);
					return node.Value.value;
				}
				else
				{
					//return null
					return default(V);
				}
			}

		}

		// add the <Key, value> into the lruMap and lruList
		public void Put(K key, V value)
		{
			lock(lruList)
			{
				LinkedListNode<CacheItem<K, V>> node;
				//if the key is already in lruMap
				if (lruMap.TryGetValue(key, out node))
				{
					//remove the old node of key in the lruList
					lruList.Remove(node);
					//update the value and the order to most recently used
					node.Value.value = value;
					lruList.AddLast(node);
					return;
				}
				//if the cache is full
				if (lruMap.Count() == this.capacity)
				{
					//remove the least recently used node
					LinkedListNode<CacheItem<K, V>> firstNode = lruList.First;
					lruList.RemoveFirst();
					lruMap.Remove(firstNode.Value.key);
				}
				//add the <key, value>
				CacheItem<K, V> item = new CacheItem<K, V>(key, value);
				LinkedListNode<CacheItem<K, V>> newnode = new LinkedListNode<CacheItem<K, V>>(item);
				lruList.AddLast(newnode);
				lruMap.Add(key, newnode);
			}
		}
	}

	internal class CacheItem<K, V>
	{
		//define the key and value in cache
		public K key;
		public V value;

		public CacheItem(K key, V value)
		{
			this.key = key;
			this.value = value;
		}
	}
}
