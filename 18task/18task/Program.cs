using System;
using System.Collections.Generic;

public class MyTreeMap<K, V>
{
    private readonly IComparer<K> comparator;
    private Node root;
    private int size;

    private class Node
    {
        public K key;
        public V value;
        public Node left;
        public Node right;
        public Node parent;

        public Node(K key, V value, Node parent)
        {
            this.key = key;
            this.value = value;
            this.parent = parent;
        }
    }

    public class MyEntry
    {
        public K Key { get; }
        public V Value { get; }

        public MyEntry(K key, V value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Key} => {Value}";
        }
    }

    // 1) Конструктор: естественный порядок
    public MyTreeMap()
    {
        comparator = Comparer<K>.Default;
        root = null;
        size = 0;
    }

    // 2) Конструктор: с компаратором
    public MyTreeMap(IComparer<K> comparator)
    {
        if (comparator == null)
        {
            throw new ArgumentNullException(nameof(comparator), "Компаратор не должен быть null.");
        }

        this.comparator = comparator;
        root = null;
        size = 0;
    }

    public void Clear()
    {
        root = null;
        size = 0;
    }

    public bool IsEmpty()
    {
        return size == 0;
    }

    public int Size()
    {
        return size;
    }

    public bool ContainsKey(object key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        if (!(key is K typedKey))
        {
            return false;
        }

        return FindNode(typedKey) != null;
    }

    public bool ContainsValue(object value)
    {
        bool found = false;
        TraverseInOrder(root, node =>
        {
            if (!found)
            {
                if (value == null)
                {
                    if (node.value == null)
                    {
                        found = true;
                    }
                }
                else
                {
                    if (value.Equals(node.value))
                    {
                        found = true;
                    }
                }
            }
        });

        return found;
    }

    public V Get(object key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        if (!(key is K typedKey))
        {
            return default(V);
        }

        Node node = FindNode(typedKey);
        if (node == null)
        {
            return default(V);
        }

        return node.value;
    }

    public void Put(K key, V value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        if (root == null)
        {
            root = new Node(key, value, null);
            size = 1;
            return;
        }

        Node current = root;
        Node parent = null;

        while (current != null)
        {
            parent = current;
            int comparison = CompareKeys(key, current.key);

            if (comparison < 0)
            {
                current = current.left;
            }
            else if (comparison > 0)
            {
                current = current.right;
            }
            else
            {
                current.value = value;
                return;
            }
        }

        Node newNode = new Node(key, value, parent);
        if (CompareKeys(key, parent.key) < 0)
        {
            parent.left = newNode;
        }
        else
        {
            parent.right = newNode;
        }

        size++;
    }

    public V Remove(object key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        if (!(key is K typedKey))
        {
            return default(V);
        }

        Node node = FindNode(typedKey);
        if (node == null)
        {
            return default(V);
        }

        V removedValue = node.value;
        DeleteNode(node);
        size--;
        return removedValue;
    }

    public List<MyEntry> EntrySet()
    {
        List<MyEntry> entries = new List<MyEntry>();
        TraverseInOrder(root, node => entries.Add(new MyEntry(node.key, node.value)));
        return entries;
    }

    public List<K> KeySet()
    {
        List<K> keys = new List<K>();
        TraverseInOrder(root, node => keys.Add(node.key));
        return keys;
    }

    public K FirstKey()
    {
        Node node = GetFirstNode();
        if (node == null)
        {
            throw new InvalidOperationException("Отображение пустое.");
        }

        return node.key;
    }

    public K LastKey()
    {
        Node node = GetLastNode();
        if (node == null)
        {
            throw new InvalidOperationException("Отображение пустое.");
        }

        return node.key;
    }

    public MyEntry FirstEntry()
    {
        Node node = GetFirstNode();
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    public MyEntry LastEntry()
    {
        Node node = GetLastNode();
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    public MyEntry PollFirstEntry()
    {
        Node node = GetFirstNode();
        if (node == null)
        {
            return null;
        }

        MyEntry entry = new MyEntry(node.key, node.value);
        DeleteNode(node);
        size--;
        return entry;
    }

    public MyEntry PollLastEntry()
    {
        Node node = GetLastNode();
        if (node == null)
        {
            return null;
        }

        MyEntry entry = new MyEntry(node.key, node.value);
        DeleteNode(node);
        size--;
        return entry;
    }

    public MyTreeMap<K, V> HeadMap(K end)
    {
        if (end == null)
        {
            throw new ArgumentNullException(nameof(end), "Граница end не должна быть null.");
        }

        MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
        TraverseInOrder(root, node =>
        {
            if (CompareKeys(node.key, end) < 0)
            {
                result.Put(node.key, node.value);
            }
        });
        return result;
    }

    public MyTreeMap<K, V> SubMap(K start, K end)
    {
        if (start == null)
        {
            throw new ArgumentNullException(nameof(start), "Граница start не должна быть null.");
        }
        if (end == null)
        {
            throw new ArgumentNullException(nameof(end), "Граница end не должна быть null.");
        }
        if (CompareKeys(start, end) > 0)
        {
            throw new ArgumentException("start должен быть меньше или равен end.");
        }

        MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
        TraverseInOrder(root, node =>
        {
            if (CompareKeys(node.key, start) >= 0 && CompareKeys(node.key, end) < 0)
            {
                result.Put(node.key, node.value);
            }
        });
        return result;
    }

    public MyTreeMap<K, V> TailMap(K start)
    {
        if (start == null)
        {
            throw new ArgumentNullException(nameof(start), "Граница start не должна быть null.");
        }

        MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
        TraverseInOrder(root, node =>
        {
            if (CompareKeys(node.key, start) > 0)
            {
                result.Put(node.key, node.value);
            }
        });
        return result;
    }

    //максимальный ключ < key
    public MyEntry LowerEntry(K key)
    {
        Node node = FindLowerNode(key);
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    //максимальный ключ <= key
    public MyEntry FloorEntry(K key)
    {
        Node node = FindFloorNode(key);
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    //минимальный ключ > key
    public MyEntry HigherEntry(K key)
    {
        Node node = FindHigherNode(key);
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    //минимальный ключ >= key
    public MyEntry CeilingEntry(K key)
    {
        Node node = FindCeilingNode(key);
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    public K LowerKey(K key)
    {
        MyEntry entry = LowerEntry(key);
        return entry == null ? default(K) : entry.Key;
    }
    public K FloorKey(K key)
    {
        MyEntry entry = FloorEntry(key);
        return entry == null ? default(K) : entry.Key;
    }
    public K HigherKey(K key)
    {
        MyEntry entry = HigherEntry(key);
        return entry == null ? default(K) : entry.Key;
    }
    public K CeilingKey(K key)
    {
        MyEntry entry = CeilingEntry(key);
        return entry == null ? default(K) : entry.Key;
    }
    private int CompareKeys(K firstKey, K secondKey)
    {
        return comparator.Compare(firstKey, secondKey);
    }

    private Node FindNode(K key)
    {
        Node current = root;
        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison < 0)
            {
                current = current.left;
            }
            else if (comparison > 0)
            {
                current = current.right;
            }
            else
            {
                return current;
            }
        }
        return null;
    }

    private Node GetFirstNode()
    {
        Node current = root;
        if (current == null)
        {
            return null;
        }

        while (current.left != null)
        {
            current = current.left;
        }

        return current;
    }

    private Node GetLastNode()
    {
        Node current = root;
        if (current == null)
        {
            return null;
        }

        while (current.right != null)
        {
            current = current.right;
        }

        return current;
    }

    private void TraverseInOrder(Node node, Action<Node> action)
    {
        if (node == null)
        {
            return;
        }

        TraverseInOrder(node.left, action);
        action(node);
        TraverseInOrder(node.right, action);
    }

    private void DeleteNode(Node node)
    {
        // 1) если два потомка — меняем с преемником (минимум в правом поддереве)
        if (node.left != null && node.right != null)
        {
            Node successor = node.right;
            while (successor.left != null)
            {
                successor = successor.left;
            }

            node.key = successor.key;
            node.value = successor.value;

            node = successor;
        }

        // 2) теперь у node максимум один ребёнок
        Node replacement = node.left != null ? node.left : node.right;

        if (replacement != null)
        {
            // есть ребёнок
            replacement.parent = node.parent;

            if (node.parent == null)
            {
                root = replacement;
            }
            else if (node == node.parent.left)
            {
                node.parent.left = replacement;
            }
            else
            {
                node.parent.right = replacement;
            }
        }
        else
        {
            // ребёнка нет
            if (node.parent == null)
            {
                root = null;
            }
            else
            {
                if (node == node.parent.left)
                {
                    node.parent.left = null;
                }
                else
                {
                    node.parent.right = null;
                }
            }
        }
    }


    private Node FindLowerNode(K key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison <= 0)
            {
                current = current.left;
            }
            else
            {
                candidate = current;
                current = current.right;
            }
        }

        return candidate;
    }

    private Node FindFloorNode(K key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison < 0)
            {
                current = current.left;
            }
            else if (comparison > 0)
            {
                candidate = current;
                current = current.right;
            }
            else
            {
                return current;
            }
        }

        return candidate;
    }

    private Node FindHigherNode(K key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison < 0)
            {
                candidate = current;
                current = current.left;
            }
            else
            {
                current = current.right;
            }
        }

        return candidate;
    }

    private Node FindCeilingNode(K key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison <= 0)
            {
                candidate = current;
                current = current.left;
            }
            else
            {
                current = current.right;
            }
        }

        return candidate;
    }
}
public static class Program
{
    public static void Main()
    {
        MyTreeMap<int, string> map = new MyTreeMap<int, string>();

        map.Put(5, "five");
        map.Put(2, "two");
        map.Put(8, "eight");
        map.Put(6, "six");

        Console.WriteLine("Size: " + map.Size());
        Console.WriteLine("FirstKey: " + map.FirstKey());
        Console.WriteLine("LastKey: " + map.LastKey());

        Console.WriteLine("Get(6): " + map.Get(6));
        Console.WriteLine("LowerEntry(6): " + map.LowerEntry(6));
        Console.WriteLine("FloorEntry(6): " + map.FloorEntry(6));
        Console.WriteLine("HigherEntry(6): " + map.HigherEntry(6));
        Console.WriteLine("CeilingEntry(6): " + map.CeilingEntry(6));

        Console.WriteLine("PollFirstEntry: " + map.PollFirstEntry());
        Console.WriteLine("PollLastEntry: " + map.PollLastEntry());

        Console.WriteLine("Entries:");
        foreach (var entry in map.EntrySet())
        {
            Console.WriteLine(entry);
        }
    }
}