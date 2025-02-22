﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace DotNetRu.Auditor.Data.Model
{
    [SuppressMessage("Naming", "CA1716: Identifiers should not match keywords")]
    public sealed class Friend : IDocument
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? Description { get; set; }

        public int GetContentChecksum()
        {
            return HashCode.Combine(Id, Name, Url, Description);
        }
    }
}
