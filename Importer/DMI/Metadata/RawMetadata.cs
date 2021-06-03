using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Importer.DMI.Metadata
{
    public class RawMetadata
    {
        public RawMetadata(string metadata)
        {
            Lines = metadata.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToImmutableArray();
        }

        private ImmutableArray<string> Lines { get; }

        private int Index { get; set; }

        private bool IsComment(string line)
        {
            return line.StartsWith("#");
        }

        public bool Next()
        {
            if (Index + 1 >= Lines.Length)
            {
                return false;
            }

            Index++;
            return true;
        }

        private string Current()
        {
            return Lines[Index].Trim();
        }

        private (string key, string value)? Tuple()
        {
            var current = Current();

            if (IsComment(current))
            {
                return null;
            }

            var split = current.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return (split[0].Trim('"'), split[1].Trim('"'));
        }

        private bool TryTuple([NotNullWhen(true)] out string? key, [NotNullWhen(true)] out string? value)
        {
            var tuple = Tuple();

            if (tuple == null)
            {
                key = null;
                value = null;
                return false;
            }

            key = tuple.Value.key;
            value = tuple.Value.value;
            return true;
        }

        public bool TryVersion([NotNullWhen(true)] out Version? version)
        {
            if (!TryTuple(out var versionKey, out var versionValue) ||
                versionKey != "version" ||
                !double.TryParse(versionValue, out var versionNumber))
            {
                version = null;
                return false;
            }

            version = new Version(versionNumber);

            while (Next())
            {
                if (!TryTuple(out var key, out var value))
                {
                    break;
                }

                switch (key)
                {
                    case "width":
                        if (!int.TryParse(value, out var width))
                        {
                            continue;
                        }

                        version = version with {Width = width};
                        break;
                    case "height":
                        if (!int.TryParse(value, out var height))
                        {
                            continue;
                        }

                        version = version with {Height = height};
                        break;
                    default:
                        return true;
                }
            }

            if (!version.Valid())
            {
                version = null;
                return false;
            }

            return true;
        }

        public bool TryState([NotNullWhen(true)] out DmiState? state)
        {
            if (!TryTuple(out var stateKey, out var stateName) ||
                stateKey != "state")
            {
                state = null;
                return false;
            }

            state = new DmiState(stateName);

            while (Next())
            {
                if (!TryTuple(out var key, out var value))
                {
                    break;
                }

                switch (key)
                {
                    case "dirs":
                        if (!int.TryParse(value, out var dirsInt))
                        {
                            continue;
                        }

                        var dirs = (DirectionTypes) dirsInt;

                        if (!Enum.IsDefined(dirs))
                        {
                            continue;
                        }

                        state = state with {Directions = dirs};
                        break;
                    case "frames":
                        if (!int.TryParse(value, out var frames))
                        {
                            continue;
                        }

                        state = state with {Frames = frames};
                        break;
                    case "delay":
                        var delayString = value.Split(",");
                        var delays = new List<float>();

                        foreach (var delay in delayString)
                        {
                            if (!float.TryParse(delay, out var delayNumber))
                            {
                                continue;
                            }

                            delays.Add(delayNumber);
                        }

                        state = state with {Delay = delays};
                        break;
                }

                if (key == "state")
                {
                    Index--;
                    break;
                }
            }

            return true;
        }
    }
}
