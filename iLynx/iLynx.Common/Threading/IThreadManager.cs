﻿using System;
using System.Threading;
using iLynx.Common.Threading.Unmanaged;

namespace iLynx.Common.Threading
{
    /// <summary>
    /// IThreadManagerService
    /// </summary>
    public interface IThreadManager
    {
        /// <summary>
        /// Starts the new.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="apartmentState">State of the apartment.</param>
        /// <returns></returns>
        IWorker StartNew(Action target, ApartmentState apartmentState = ApartmentState.MTA);

        /// <summary>
        /// Starts the new.
        /// </summary>
        /// <typeparam name="TArgs">The type of the args.</typeparam>
        /// <param name="target">The target.</param>
        /// <param name="args">The args.</param>
        /// <param name="apartmentState">State of the apartment.</param>
        /// <returns></returns>
        IParameterizedWorker<TArgs> StartNew<TArgs>(Action<TArgs> target, TArgs args, ApartmentState apartmentState = ApartmentState.MTA);

        /// <summary>
        /// Starts the new.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="target">The target.</param>
        /// <param name="apartmentState">State of the apartment.</param>
        /// <returns></returns>
        IResultWorker<TResult> StartNew<TResult>(Func<TResult> target, ApartmentState apartmentState = ApartmentState.MTA);

        /// <summary>
        /// Starts the new.
        /// </summary>
        /// <typeparam name="TArgs">The type of the args.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="target">The target.</param>
        /// <param name="args">The args.</param>
        /// <param name="apartmentState">State of the apartment.</param>
        /// <returns></returns>
        IParameterizedResultWorker<TArgs, TResult> StartNew<TArgs, TResult>(Func<TArgs, TResult> target, TArgs args, ApartmentState apartmentState = ApartmentState.MTA);
    }
}
