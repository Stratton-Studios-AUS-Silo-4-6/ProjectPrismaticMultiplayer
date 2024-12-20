using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Handles displaying a list
    /// </summary>
    /// <typeparam name="T">
    /// The type for the data used to populate entries
    /// </typeparam>
    /// <typeparam name="T2">
    /// The type for the entry Object itself
    /// </typeparam>
    public class ListView<T, T2> where T2 : MonoBehaviour, IListViewEntry<T>
    {
        private readonly T2 prefab;
        private readonly Transform container;
        
        private List<T2> entries = new();

        public List<T2> Entries => entries;

        public delegate void OnAdd(int index, T data, T2 display);
        public delegate void OnRemove(int index, T2 display);
        
        /// <summary>
        /// The event delegate invoked when an individual <see cref="T2"/> entry is <see cref="Add">Displayed</see>.
        /// </summary>
        public event OnAdd onAdd;
        
        /// <summary>
        /// The event delegate invoked when an individual <see cref="T2"/> entry is <see cref="Remove">Removed</see>.
        /// </summary>
        public event OnRemove onRemove;

        public ListView(T2 prefab, Transform container)
        {
            this.prefab = prefab;
            this.container = container;
        }

        /// <summary>
        /// Creates a collection of <see cref="T2">entries</see> containing data of type <see cref="T"/>.
        /// </summary>
        /// <param name="entryData">
        /// An array of <see cref="T"/> data to be displayed in a collection of <see cref="T2"/> entries.
        /// </param>
        public void Add(T[] entryData)
        {
            foreach (var data in entryData)
            {
                Add(data);
            }
        }

        /// <summary>
        /// Creates an <see cref="T2">entry</see> containing data of type <see cref="T"/>.
        /// </summary>
        /// <param name="data">
        /// The data that will be displayed on the created entry.
        /// </param>
        public void Add(T data)
        {
            var entry = Object.Instantiate(prefab, container);
            entry.OnAdd(data);
            entries.Add(entry);

            var index = entries.Count - 1;
            onAdd?.Invoke(index, data, entry);
        }

        public void Remove(int index)
        {
            var entry = entries[index];
            onRemove?.Invoke(index, entry);
            RemoveInternal(index);
            entries.RemoveAt(index);
        }
        
        /// <summary>
        /// Remove all <see cref="T2"/> entry objects.
        /// </summary>
        public void Clear()
        {
            if (entries == null)
            {
                return;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                RemoveInternal(i);
            }

            entries.Clear();
        }

        private void RemoveInternal(int index)
        {
            var entry = entries[index];
            entry.OnRemove();
            Object.Destroy(entry.gameObject);
        }
    }
}