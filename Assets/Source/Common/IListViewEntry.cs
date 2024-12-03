namespace PGALegends.UI
{
    /// <summary>
    /// Interface implemented by UI objects to be displayed by <see cref="ListView{T,T2}"/>
    /// </summary>
    /// <typeparam name="T">
    /// The type for the data object to be displayed by this entry.
    /// </typeparam>
    public interface IListViewEntry<T>
    {
        public void Display(T data);
        public void Remove();
    }
}