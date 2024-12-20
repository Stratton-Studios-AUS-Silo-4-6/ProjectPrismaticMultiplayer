namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Interface implemented by UI objects to be displayed by <see cref="ListView{T,T2}"/>
    /// </summary>
    /// <typeparam name="T">
    /// The type for the data object to be displayed by this entry.
    /// </typeparam>
    public interface IListViewEntry<T>
    {
        /// <summary>
        /// Called when this entry is added to a <see cref="ListView{T,T2}"/>.
        /// </summary>
        /// <param name="data"></param>
        public void OnAdd(T data);
        
        /// <summary>
        /// Called when this entry is removed from a <see cref="ListView{T,T2}"/>.
        /// </summary>
        public void OnRemove();
    }
}