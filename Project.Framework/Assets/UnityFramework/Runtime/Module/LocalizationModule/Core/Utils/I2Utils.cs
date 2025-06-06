﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace UnityFramework.Localization
{
    public static class I2Utils
    {
        public const string ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
        public const string NumberChars = "0123456789";
        public const string ValidNameSymbols = ".-_$#@*()[]{}+:?!&',^=<>~`";

        public static string ReverseText(string source)
        {
            int len = source.Length;
            char[] output = new char[len];

            char[] separators = { '\r', '\n' };
            for (int istart = 0; istart<len;)
            {
                int iend = source.IndexOfAny(separators, istart);
                if (iend < 0) iend = len;
                Reverse(istart, iend-1);

                for (istart = iend; istart < len && (source[istart] == '\r' || source[istart] == '\n'); istart++)
                {
                    output[istart] = source[istart];
                }
            }

            void Reverse(int start, int end)
            {
                for (var i = 0; i <= end-start; i++) {
                    output[end-i] = source[start+i];
                }
            }

            return new string(output);
        }


        public static string GetValidTermName( string text, bool allowCategory = false)
        {
            if (text == null)
                return null;

            // Remove Tags and NonASCII
            
            var result = new StringBuilder();
            bool insideTag = false; // Indicates if we're currently inside a tag
            bool shouldAppend; // Flag to determine whether to append the current character
            char tagType = '\0'; // Tracks the type of the opening tag
            bool skipped = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i]; // Current character being processed
                shouldAppend = true; // By default, we append the character unless a condition says otherwise

                // Check if the current character is the start of a tag
                if ((c == '{' || c == '[' || c == '<') && !insideTag)
                {
                    // Special case for '{' not followed by '['
                    if (c == '{' && (i + 1 < text.Length && text[i + 1] != '['))
                    {
                        // We'll append '{' as it's not considered a tag in this context
                    }
                    else
                    {
                        // We're now inside a tag, so don't append the current character
                        insideTag = true;
                        shouldAppend = false;
                        tagType = c; // Remember the type of the tag we're in
                    }
                }
                // Check if the current character is the end of a tag
                else if (insideTag && ((c == '}' && tagType == '{') || (c == ']' && tagType == '[') || (c == '>' && tagType == '<')))
                {
                    // We've found the end of the tag, so stop skipping characters
                    insideTag = false;
                    shouldAppend = false; // Don't append the closing tag character
                }
                else if (insideTag)
                {
                    // We're inside a tag, so don't append the current character
                    shouldAppend = false;
                }

                // Append the current character if it's not part of a tag
                if (shouldAppend)
                {
                    char new_char = ' ';
                    if (allowCategory && (c == '\\' || c == '\"' || c == '/') ||
                        char.IsLetterOrDigit(c) ||
                        ValidNameSymbols.IndexOf(c) >= 0)
                    {
                        new_char = c;
                    }

                    if (char.IsWhiteSpace(c))
                    {
                        if (!skipped)
                        {
                            if (result.Length > 0)
                                result.Append(' ');

                            skipped = true;
                        }
                    }
                    else
                    {
                        skipped = false;
                        result.Append(new_char);
                    }                    
                }
            }

            return result.ToString(); // Convert the StringBuilder to a string and return it
        }
        
        public static string SplitLine(string line, int maxCharacters)
        {
            if (maxCharacters <= 0 || line.Length < maxCharacters)
                return line;

            var chars = line.ToCharArray();
            bool insideOfLine = true;
            bool allowNewLine = false;
            for (int i = 0, nCharsInLine = 0; i < chars.Length; ++i)
            {
                if (insideOfLine)
                {
                    nCharsInLine++;
                    if (chars[i] == '\n')
                    {
                        nCharsInLine = 0;
                    }
                    if (nCharsInLine >= maxCharacters && char.IsWhiteSpace(chars[i]))
                    {
                        chars[i] = '\n';
                        insideOfLine = false;
                        allowNewLine = false;
                    }
                }
                else
                {
                    if (!char.IsWhiteSpace(chars[i]))
                    {
                        insideOfLine = true;
                        nCharsInLine = 0;
                    }
                    else
                    {
                        if (chars[i] != '\n')
                        {
                            chars[i] = (char)0;
                        }
                        else
                        {
                            if (!allowNewLine)
                                chars[i] = (char)0;
                            allowNewLine = true;
                        }
                    }
                }
            }

            return new string(chars.Where(c => c != (char)0).ToArray());
        }

        public static bool FindNextTag(string line, int iStart, out int tagStart, out int tagEnd)
        {
            tagStart = -1;
            tagEnd = -1;
            int len = line.Length;

            // Find where the tag starts
            for (tagStart = iStart; tagStart < len; ++tagStart)
                if (line[tagStart] == '[' || line[tagStart] == '(' || line[tagStart] == '{' || line[tagStart] == '<')
                    break;

            if (tagStart == len)
                return false;

            bool isArabic = false;
            for (tagEnd = tagStart + 1; tagEnd < len; ++tagEnd)
            {
                char c = line[tagEnd];
                if (c == ']' || c == ')' || c == '}' || c=='>')
                {
                    if (isArabic) return FindNextTag(line, tagEnd + 1, out tagStart, out tagEnd);
                    return true;
                }
                if (c > 255) isArabic = true;
            }

            // there is an open, but not close character
            return false;
        }

        public static string RemoveTags(string text)
        {
            return Regex.Replace(text, @"\{\[(.*?)]}|\[(.*?)]|\<(.*?)>", "");
        }

        public static bool RemoveResourcesPath(ref string sPath)
        {
            int Ind1 = sPath.IndexOf("\\Resources\\", StringComparison.Ordinal);
            int Ind2 = sPath.IndexOf("\\Resources/", StringComparison.Ordinal);
            int Ind3 = sPath.IndexOf("/Resources\\", StringComparison.Ordinal);
            int Ind4 = sPath.IndexOf("/Resources/", StringComparison.Ordinal);
            int Index = Mathf.Max(Ind1, Ind2, Ind3, Ind4);
            bool IsResource = false;
            if (Index >= 0)
            {
                sPath = sPath.Substring(Index + 11);
                IsResource = true;
            }
            else
            {
                // If its not in the Resources, then it has to be in the References
                // Therefore, the path has to be stripped and let only the name
                Index = sPath.LastIndexOfAny(LanguageSourceData.CategorySeparators);
                if (Index > 0)
                    sPath = sPath.Substring(Index + 1);
            }

            string Extension = Path.GetExtension(sPath);
            if (!string.IsNullOrEmpty(Extension))
                sPath = sPath.Substring(0, sPath.Length - Extension.Length);

            return IsResource;
        }

        public static bool IsPlaying()
        {
            if (Application.isPlaying)
                return true;
            #if UNITY_EDITOR
                return EditorApplication.isPlayingOrWillChangePlaymode;
            #else
                return false;
            #endif
        }

        public static string GetPath(this Transform tr)
        {
            var parent = tr.parent;
            if (tr == null)
                return tr.name;
            return parent.GetPath() + "/" + tr.name;
        }

#if UNITY_5_3_OR_NEWER
        public static Transform FindObject(string objectPath)
        {
            return FindObject(SceneManager.GetActiveScene(), objectPath);
        }


        public static Transform FindObject(UnityEngine.SceneManagement.Scene scene, string objectPath)
        {
            //var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            var roots = scene.GetRootGameObjects();
            for (int i=0; i<roots.Length; ++i)
            {
                var root = roots[i].transform;
                if (root.name == objectPath)
                    return root;

                if (!objectPath.StartsWith(root.name + "/", StringComparison.Ordinal))
                    continue;

                return FindObject(root, objectPath.Substring(root.name.Length + 1));
            }
            return null;
        }

        public static Transform FindObject(Transform root, string objectPath)
        {
            for (int i=0; i<root.childCount; ++i)
            {
                var child = root.GetChild(i);
                if (child.name == objectPath)
                    return child;

                if (!objectPath.StartsWith(child.name + "/", StringComparison.Ordinal))
                    continue;

                return FindObject(child, objectPath.Substring(child.name.Length + 1));
            }
            return null;
        }
#endif

        public static H FindInParents<H>(Transform tr) where H : Component
        {
            if (!tr)
                return null;

            H comp = tr.GetComponent<H>();
            while (!comp && tr)
            {
                comp = tr.GetComponent<H>();
                tr = tr.parent;
            }
            return comp;
        }

        public static string GetCaptureMatch(Match match)
        {
            for (int i = match.Groups.Count - 1; i >= 0; --i)
                if (match.Groups[i].Success)
                {
                    return match.Groups[i].ToString();
                }
            return match.ToString();
        }

        public static void SendWebRequest(UnityWebRequest www )
        {
            #if UNITY_2017_2_OR_NEWER
                www.SendWebRequest();
            #else
                www.Send();
            #endif
        }
    }
}