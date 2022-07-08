using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Utils
{
    /// <summary>
    /// Attempts to decode a form from an http request body
    /// </summary>
    public class FormDecoder : IFormDecoder
    {
        /// <summary>
        /// Attempt to decode the form from the body
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public IFormCollection Decode(Stream body)
        {
            var result = new FakeFormCollection();
            var flag = new OneWayFlag();
            var collected = new List<string>();
            using var enumerator = body.ReadLines(
                eol => flag.WasSet = eol == Eol.CrLf
            ).GetEnumerator();
            while (enumerator.MoveNext())
            {
                collected.Add(enumerator.Current);
                if (enumerator.Current is null)
                {
                    break;
                }

                var trimmed = enumerator.Current.Trim();
                if (trimmed == MultiPartBodyEncoder.BOUNDARY)
                {
                    ContinueDecodingMultipart(result, enumerator, flag);
                    break;
                }

                var parts = trimmed.Split('&');
                foreach (var part in parts)
                {
                    var sub = part.Split('=');
                    var key = WebUtility.UrlDecode(sub[0]);
                    var value = WebUtility.UrlDecode(
                        string.Join("=", sub.Skip(1))
                    );
                    result.FormValues[key] = value;
                }
            }
            if (IsJson(collected))
            {
                return new FakeFormCollection();
            }

            return result;
        }

        private bool IsJson(List<string> collected)
        {
            var joined = string.Join("\n", collected);
            try
            {
                JsonSerializer.Deserialize<object>(joined);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private struct MultiPartBits
        {
            public string Name;
            public string FileName;
            public string MimeType;
            public List<string> StoredLines;

            public MultiPartBits()
            {
                Name = null;
                FileName = null;
                MimeType = null;
                StoredLines = new List<string>();
            }

            public void Clear()
            {
                Name = null;
                FileName = null;
                MimeType = null;
                StoredLines.Clear();
            }

            public void RemoveLastStoredLineIfEmpty()
            {
                if (StoredLines.LastOrDefault() == "")
                {
                    StoredLines.RemoveAt(StoredLines.Count - 1);
                }
            }
        }

        private void ContinueDecodingMultipart(
            FakeFormCollection result,
            IEnumerator<string> enumerator,
            OneWayFlag hasCrLf
        )
        {
            var bits = new MultiPartBits();
            var state = MultiPartState.None;
            var skipNextEmpty = false;
            while (enumerator.MoveNext())
            {
                var line = enumerator.Current;
                if (string.IsNullOrWhiteSpace(line) && skipNextEmpty)
                {
                    skipNextEmpty = false;
                    continue;
                }

                // ReSharper disable once PossibleNullReferenceException
                if (line.StartsWith(MultiPartBodyEncoder.BOUNDARY))
                {
                    StoreLast();
                    continue;
                }

                var (key, value) = SplitKeyValue(line);
                var (isContentDisposition, isContentType, isContent)
                    = ClassifyKey(key, state);

                if (isContent)
                {
                    bits.StoredLines.AddIf(state != MultiPartState.None, line);
                    continue;
                }

                if (isContentDisposition)
                {
                    (bits.Name, bits.FileName) = ParseContentDisposition(value);
                    if (bits.Name is null)
                    {
                        throw new ArgumentException(
                            $"Malformed Content-Disposition line: '{line}' (no name found)"
                        );
                    }

                    state = bits.FileName is null
                        ? MultiPartState.InField
                        : MultiPartState.InFile;
                    skipNextEmpty = true;
                }
                else if (isContentType)
                {
                    bits.MimeType = value;
                }
            }

            StoreLast();

            void StoreLast()
            {
                bits.RemoveLastStoredLineIfEmpty();
                switch (state)
                {
                    case MultiPartState.InField:
                        var fieldValue = string.Join(Environment.NewLine, bits.StoredLines);
                        result[bits.Name] = fieldValue;
                        break;
                    case MultiPartState.InFile:
                        var value = string.Join(
                            hasCrLf.WasSet
                                ? "\r\n"
                                : "\n",
                            bits.StoredLines);
                        result.AddFile(new FakeFormFile(value, bits.Name, bits.FileName, bits.MimeType));
                        break;
                }

                state = MultiPartState.None;
                bits.Clear();
            }
        }

        private static (bool isContentDisposition, bool isContentType, bool isContent)
            ClassifyKey(string line, MultiPartState state)
        {
            var isContentDisposition = state == MultiPartState.None &&
                line.StartsWith(MultiPartBodyEncoder.CONTENT_DISPOSITION);
            var isContentType = state != MultiPartState.None && line.StartsWith(MultiPartBodyEncoder.CONTENT_TYPE);
            var isContentLength =
                state != MultiPartState.None && line.StartsWith(MultiPartBodyEncoder.CONTENT_LENGTH);
            var isContent = !(isContentDisposition || isContentType || isContentLength);
            return (isContentDisposition, isContentType, isContent);
        }

        private static (string name, string fileName) ParseContentDisposition(
            string value
        )
        {
            string name = null;
            string fileName = null;
            var parts = value.Split(';');
            foreach (var part in parts)
            {
                var sub = part.Split('=');
                if (sub.Length < 2)
                {
                    continue;
                }

                var first = sub[0].Trim();
                var second = WebUtility.UrlDecode(sub[1].Trim(new[] { '"', ' ' }));
                if (first == MultiPartBodyEncoder.NAME)
                {
                    name = second;
                }
                else if (first == MultiPartBodyEncoder.FILE_NAME)
                {
                    fileName = second;
                }
            }

            return (name, fileName);
        }

        private static (string key, string value) SplitKeyValue(string str)
        {
            var parts = str.Split(':');
            return (
                parts[0].Trim(),
                string.Join(":", parts.Skip(1)).Trim()
            );
        }

        private enum MultiPartState
        {
            None,
            InField,
            InFile
        }
    }
}