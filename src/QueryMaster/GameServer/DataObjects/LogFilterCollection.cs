﻿#region License

/*
Copyright (c) 2015 Betson Roy

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace QueryMaster.GameServer.DataObjects
{
    /// <summary>
    ///     Represents collection of logfilter.
    /// </summary>
    public class LogFilterCollection : IEnumerable<LogFilter>
    {
        /// <summary>
        ///     used to set lock on add/remove of filter.
        /// </summary>
        protected internal static object LockObj = new object();

        private readonly List<LogFilter> _filterList = new List<LogFilter>();

        /// <summary>
        ///     Returns an enumerator that iterates through the Filter collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<LogFilter> GetEnumerator()
        {
            foreach (var i in _filterList) yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///     Enables all filters.
        /// </summary>
        public void EnableAll()
        {
            _filterList.ForEach(x => x.Enabled = true);
        }

        /// <summary>
        ///     Enables filter of specific type.
        /// </summary>
        /// <param name="type">Filter type.</param>
        public void EnableAll(Type type)
        {
            _filterList.ForEach(x =>
            {
                if (x.GetType() == type) x.Enabled = true;
            });
        }

        /// <summary>
        ///     Disables all filters.
        /// </summary>
        public void DisableAll()
        {
            _filterList.ForEach(x => x.Enabled = false);
        }

        /// <summary>
        ///     Disables filter of specific type.
        /// </summary>
        /// <param name="type">Filter type.</param>
        public void DisableAll(Type type)
        {
            _filterList.ForEach(x =>
            {
                if (x.GetType() == type) x.Enabled = false;
            });
        }

        /// <summary>
        ///     Adds a filter to the end of the collection.
        /// </summary>
        /// <param name="filter"></param>
        public void Add(LogFilter filter)
        {
            lock (LockObj)
            {
                _filterList.Add(filter);
            }
        }

        /// <summary>
        ///     Removes specified filter from the collection.
        /// </summary>
        /// <param name="filter"></param>
        public void Remove(LogFilter filter)
        {
            lock (LockObj)
            {
                _filterList.Remove(filter);
            }
        }

        /// <summary>
        ///     Removes all filters from the collection.
        /// </summary>
        public void Clear()
        {
            _filterList.Clear();
        }
    }
}