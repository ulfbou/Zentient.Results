using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A simple dummy implementation of IResultStatus for test purposes.
    /// </summary>
    internal class DummyStatus : IResultStatus
    {
        public int Code { get; init; }
        public string Description { get; init; } = string.Empty;

        public override string ToString() => $"{Code} {Description}";
    }
}
