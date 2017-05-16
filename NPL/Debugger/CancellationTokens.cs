﻿// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Diagnostics;
using System.Threading;

namespace NPLTools.Debugger
{
    public static class CancellationTokens
    {
        public static CancellationToken GetToken(TimeSpan delay)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return CancellationToken.None;
            }
            return new CancellationTokenSource(delay).Token;
        }

        public static CancellationToken After60s => GetToken(TimeSpan.FromSeconds(60));
        public static CancellationToken After15s => GetToken(TimeSpan.FromSeconds(15));
        public static CancellationToken After5s => GetToken(TimeSpan.FromSeconds(5));
        public static CancellationToken After2s => GetToken(TimeSpan.FromSeconds(2));
        public static CancellationToken After1s => GetToken(TimeSpan.FromSeconds(1));
        public static CancellationToken After500ms => GetToken(TimeSpan.FromMilliseconds(500));
        public static CancellationToken After100ms => GetToken(TimeSpan.FromMilliseconds(100));
    }
}
