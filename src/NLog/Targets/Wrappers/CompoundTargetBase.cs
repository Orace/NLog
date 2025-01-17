// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

namespace NLog.Targets.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NLog.Common;

    /// <summary>
    /// A base class for targets which wrap other (multiple) targets
    /// and provide various forms of target routing.
    /// </summary>
    public abstract class CompoundTargetBase : Target
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundTargetBase" /> class.
        /// </summary>
        /// <param name="targets">The targets.</param>
        protected CompoundTargetBase(params Target[] targets)
        {
            Targets = new List<Target>(targets);
        }

        /// <summary>
        /// Gets the collection of targets managed by this compound target.
        /// </summary>
        public IList<Target> Targets { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _tostring ?? (_tostring = GenerateTargetToString());
        }

        private string GenerateTargetToString()
        {
            if (string.IsNullOrEmpty(Name) && Targets?.Count > 0)
            {
                string separator = string.Empty;
                var sb = new StringBuilder();
                sb.Append(GenerateTargetToString(true));
                sb.Append('[');

                foreach (var t in Targets)
                {
                    sb.Append(separator);
                    sb.Append(t.ToString());
                    separator = ", ";
                }

                sb.Append(']');
                return sb.ToString();
            }

            return GenerateTargetToString(true);
        }

        /// <inheritdoc/>
        protected override void Write(LogEventInfo logEvent)
        {
            throw new NotSupportedException("This target must not be invoked in a synchronous way.");
        }

        /// <summary>
        /// Flush any pending log messages for all wrapped targets.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            AsyncHelpers.ForEachItemInParallel(Targets, asyncContinuation, (t, c) => t.Flush(c));
        }
    }
}