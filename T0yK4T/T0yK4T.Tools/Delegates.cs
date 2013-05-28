
namespace T0yK4T.Tools
{
    /// <summary>
    /// This delegate is used to notify a receiver of an event that something has happened
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="val">The even data</param>
    /// <param name="sender">The sender of the event</param>
    public delegate void GenericEventHandler<TSource, T>(TSource sender, T val);

    /// <summary>
    /// This delegate is used to notify a receiver of an event that something has happened
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="sender">The sender of this event</param>
    public delegate void GenericEventHandler<TSource>(TSource sender);
}