// Copyright 2013-2019 Rik Essenius
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.FormattableString;

namespace UiAutomation.Model
{
    internal class FileSystem
    {
        private readonly string[] _pathList;

        public FileSystem() : this(Environment.GetEnvironmentVariable("PATH"))
        {
        }

        public FileSystem(string pathList) => _pathList = (pathList ?? string.Empty).Split(';');

        private static string CanonicalPath(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;
            return null == parentDirInfo
                ? dirInfo.Name
                : Path.Combine(CanonicalPath(parentDirInfo), parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        private static string CanonicalPath(string filename)
        {
            var fileInfo = new FileInfo(filename);
            var dirInfo = fileInfo.Directory;
            Debug.Assert(dirInfo != null, nameof(dirInfo) + " != null");
            return Path.Combine(CanonicalPath(dirInfo), dirInfo.GetFiles(fileInfo.Name)[0].Name);
        }

        public string FindExecutable(string path)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(path);
            if (File.Exists(expandedPath)) return CanonicalPath(expandedPath);
            if (!string.IsNullOrEmpty(Path.GetDirectoryName(expandedPath)))
            {
                throw new FileNotFoundException(Invariant($"Could not find file with path [{expandedPath}]"));
            }

            var testPath = string.Empty;
            if (_pathList.Select(entry => entry.Trim()).Any(trimmedEntry =>
                !string.IsNullOrEmpty(trimmedEntry) && File.Exists(testPath = Path.Combine(trimmedEntry, expandedPath))))
            {
                return CanonicalPath(testPath);
            }

            throw new FileNotFoundException(Invariant($"Could not find file [{expandedPath}] in the Environment Path"));
        }
    }
}