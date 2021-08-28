// Copyright 2013-2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UiAutomation.Model
{
    [Serializable]
    internal class Mapping<T> : Dictionary<string, T>
    {
        private readonly string _name;

        public Mapping(string newName) : base(StringComparer.OrdinalIgnoreCase) => _name = newName;

        protected Mapping(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _name = (string) info.GetValue("MappingName", typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("MappingName", _name);
            base.GetObjectData(info, context);
        }

        public T Map(string keyToFind)
        {
            if (TryGetValue(keyToFind.Replace(" ", string.Empty), out var returnValue)) return returnValue;
            throw new ArgumentException(keyToFind + " is an unrecognized " + _name);
        }
    }
}
