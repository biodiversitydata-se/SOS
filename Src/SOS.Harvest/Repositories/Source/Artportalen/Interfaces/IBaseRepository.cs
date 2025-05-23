﻿using System.Data;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Base repository interface
    /// </summary>
    public interface IBaseRepository<T>
    {
        /// <summary>
        /// Query db
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic parameters, CommandType commandType);

        /// <summary>
        /// True if live data base should be used
        /// </summary>
        bool Live { get; set; }
    }
}