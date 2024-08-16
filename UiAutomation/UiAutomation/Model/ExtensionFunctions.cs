// Copyright 2013-2024 Rik Essenius
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
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace UiAutomation.Model;

internal static class ExtensionFunctions
{
    internal static int TimeoutInMilliseconds { get; set; } = 3000;

    internal static bool Exit(this Process process, bool force)
    {
        if (process == null) return true;
        process.Refresh();
        if (process.HasExited) return true;
        if (process.CloseMainWindow()) return true;

        if (!force) return false;
        process.Kill();

        return true;
    }

    public static string StripUnicodeCharacters(this string input) => Encoding.ASCII.GetString(
        Encoding.Convert(
            Encoding.UTF8,
            Encoding.GetEncoding(Encoding.ASCII.EncodingName,
                new EncoderReplacementFallback(string.Empty),
                new DecoderExceptionFallback()
            ),
            Encoding.UTF8.GetBytes(input)
        )
    );

    /// <summary>
    ///     Unpack a buffer containing potentially multiple zero-terminating strings into a list of strings
    /// </summary>
    /// <param name="buffer">the buffer containing the strings</param>
    /// <returns>the list of strings</returns>
    public static List<string> Unpack(this char[] buffer)
    {
        var result = new List<string>();
        var bufferIndex = 0;
        var entry = new StringBuilder();
        while (bufferIndex < buffer.Length && buffer[bufferIndex] != 0)
        {
            entry.Append(buffer[bufferIndex++]);
            if (buffer[bufferIndex] != 0) continue;
            result.Add(entry.ToString());
            entry.Clear();
            bufferIndex++;
        }

        return result;
    }

    internal static bool WaitForExit(this Process process, bool force)
    {
        if (process.WaitForExit(TimeoutInMilliseconds)) return true;
        if (!force) return false;
        process.Kill();
        return true;
    }

    internal static bool WaitWithTimeoutTill<T>(this T target, Func<T, bool> conditionFunction)
    {
        // use stopwatch to make resilient to slow functions such as FindControl. Use Sleep to make resilient to fast ones.
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (stopwatch.ElapsedMilliseconds < TimeoutInMilliseconds)
        {
            if (conditionFunction(target)) return true;
            Thread.Sleep(100);
        }

        return false;
    }
}