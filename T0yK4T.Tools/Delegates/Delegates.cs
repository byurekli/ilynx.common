using System;
using System.Collections.Generic;

namespace T0yK4T.Tools
{
	/// <summary>
	/// A Generic delegate with no return type
	/// </summary>
	/// <typeparam name="TParam1">The type of the parameter</typeparam>
	/// <param name="param1">The parameter itself</param>
	public delegate void GenericVoid<TParam1>(TParam1 param1);

    /// <summary>
    /// A Generic delegate with no return type and two arguments
    /// </summary>
    /// <typeparam name="TParam1">The type of the first parameter</typeparam>
    /// <typeparam name="TParam2">The type of the second parameter</typeparam>
    /// <param name="param1">The actual first parameter</param>
    /// <param name="param2">The actual second parameter</param>
    public delegate void GenericVoid<TParam1, TParam2>(TParam1 param1, TParam2 param2);

	/// <summary>
	/// a "Completely" generic delegate with a generic return type
	/// </summary>
	/// <typeparam name="T">Specifies both the return type and parameter type</typeparam>
	/// <param name="param">The parameter itself</param>
	/// <returns>T</returns>
	public delegate T GenericDelegate<T>(T param);

	/// <summary>
	/// Simple empty delegate, no return type, no parameters
	/// </summary>
	public delegate void VoidDelegate();
}