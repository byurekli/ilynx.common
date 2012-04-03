using System.Collections.Generic;
namespace Hasherer
{
    /// <summary>
    /// This interface is used to find external <see cref="AsyncHashProvider"/>s
    /// </summary>
    public interface IProviderInstantiator
    {
        /// <summary>
        /// When implemented, should return a list of <see cref="AsyncHashProvider"/>s
        /// </summary>
        /// <returns></returns>
        IEnumerable<AsyncHashProvider> Instantiate();
    }
}