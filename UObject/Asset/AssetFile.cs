﻿using System;
using System.Collections.Generic;
using DragonLib.IO;
using JetBrains.Annotations;
using UObject.Generics;

namespace UObject.Asset
{
    [PublicAPI]
    public class AssetFile
    {
        public AssetFile(Span<byte> uasset, Span<byte> uexp, int unrealVersion = 524)
        {
            var cursor = 0;
            Summary = new PackageFileSummary();
            Summary.Deserialize(uasset, this, unrealVersion, ref cursor);

            cursor = Summary.NameOffset;
            Names = ObjectSerializer.DeserializeProperties<NameEntry>(uasset, this, Summary.NameCount, ref cursor);
            cursor = Summary.ImportOffset;
            Imports = ObjectSerializer.AllocateProperties<ObjectImport>(Summary.ImportCount);
            ObjectSerializer.DeserializeProperties(uasset, this, Imports, ref cursor);
            cursor = Summary.ExportOffset;
            Exports = ObjectSerializer.AllocateProperties<ObjectExport>(Summary.ExportCount);
            ObjectSerializer.DeserializeProperties(uasset, this, Exports, ref cursor);
            cursor = Summary.PreloadDependencyOffset;
            PreloadDependencies = SpanHelper.ReadStructArray<PreloadDependencyIndex>(uasset, Summary.PreloadDependencyCount, ref cursor);

            foreach (var export in Exports) ExportObjects[export.ObjectName] = ObjectSerializer.DeserializeObject(this, export, uasset, uexp);
        }

        public AssetFile() { }

        public PackageFileSummary Summary { get; set; } = new PackageFileSummary();
        public NameEntry[] Names { get; set; } = new NameEntry[0];
        public ObjectImport[] Imports { get; set; } = new ObjectImport[0];
        public ObjectExport[] Exports { get; set; } = new ObjectExport[0];
        public PreloadDependencyIndex[] PreloadDependencies { get; set; } = new PreloadDependencyIndex[0];

        public Dictionary<string, ISerializableObject> ExportObjects { get; set; } = new Dictionary<string, ISerializableObject>();
    }
}
