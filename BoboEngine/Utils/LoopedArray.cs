using BoboEngine;

public class LoopedArray<T>
{
    private readonly List<Item> items;

    public Item[] Items => items.ToArray();
    public int Size => items.Count;

    public LoopedArray(IEnumerable<T> array)
    {
        items = [];

        int i = 0;
        foreach (var item in array) items.Add(new Item(i++, this, item));
    }

    public Item GetItem(int i) => items[i];

    public bool RemoveItem(Item item)
    {
        if (!items.Contains(item)) return false;

        items.Remove(item);

        return true;
    }
    public void RemoveItem(int i)
    {
        items.RemoveAt(i);
    }


    public class Item
    {
        public T value;
        public int index => parent.items.IndexOf(this);
        public readonly int initalIndex;
        public readonly LoopedArray<T> parent;

        internal Item(int initalIndex, LoopedArray<T> parent, T value)
        {
            this.value = value;
            this.initalIndex = initalIndex;
            this.parent = parent;
        }

        public Item Next => parent.GetItem(Maths.Mod(index + 1, parent.Size));
        public Item Previous => parent.GetItem(Maths.Mod(index - 1, parent.Size));
    }

}