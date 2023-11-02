﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace var_browser
{
    /// <summary>
    /// 自定义的var包id扫描器
    /// 相比于正则表达式，有几十倍的性能提升
    /// </summary>
    class VarNameParser
    {
        static StringBuilder s_TempBuilder = new StringBuilder();
        static HashSet<string> s_TempResult = new HashSet<string>();
        public static HashSet<string> Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            s_TempResult.Clear();
            //(creater).(varname).(version):
            for (int i = 0; i < text.Length - 5;)
            {
                //清空
                s_TempBuilder.Length = 0;
                int createrLen = ReadString(s_TempBuilder, text, ref i, 5);
                if (createrLen > 0)
                {
                    if (ReadDot(s_TempBuilder, text, ref i))
                    {
                        int varNameLen = ReadString(s_TempBuilder, text, ref i, 3);
                        if (varNameLen > 0)
                        {
                            if (ReadDot(s_TempBuilder, text, ref i))
                            {
                                //versionId或者latest
                                int versionLen = ReadVersion(s_TempBuilder, text, ref i, 1);
                                if (versionLen > 0)
                                {
                                    if (ReadColon(text, ref i))
                                    {
                                        string uid = s_TempBuilder.ToString();// string.Format("{0}.{1}.{2}", creater, varName, version);
                                        s_TempResult.Add(uid);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return s_TempResult;
        }
        static int ReadString(StringBuilder builder, string text, ref int idx, int leastLeftCntToRead)
        {
            char peek = text[idx];
            if (peek == '\\' || peek == '/' || peek == ':' || peek == '*' || peek == '?' || peek == '"'
                    || peek == '<' || peek == '>' || peek == '.' || peek == '\n' || peek == '\r')
            {
                idx++;
                return 0;
            }

            int cnt = 0;
            while (true)
            {
                if (peek == '\\' || peek == '/' || peek == ':' || peek == '*' || peek == '?' || peek == '"'
                    || peek == '<' || peek == '>' || peek == '.' || peek == '\n' || peek == '\r')
                {
                    break;
                }
                builder.Append(peek);
                cnt++;
                //至少要预留这么多字节，否则后面也没法解析
                if (text.Length <= leastLeftCntToRead + idx)
                    break;
                idx++;
                peek = text[idx];
            }
            return cnt;
        }
        static bool ReadDot(StringBuilder builder, string text, ref int idx)
        {
            if (text[idx] == '.')
            {
                idx++;
                builder.Append('.');
                return true;
            }
            return false;
        }
        static bool ReadColon(string text, ref int idx)
        {
            if (text[idx] == ':')
            {
                idx++;
                return true;
            }
            return false;
        }
        static int ReadVersion(StringBuilder builder, string text, ref int idx, int leastLeftCntToRead)
        {
            if (text.Length > idx + 6 + leastLeftCntToRead)//预留读取latest:
            {
                if (text[idx] == 'l'
                    && text[idx + 1] == 'a'
                    && text[idx + 2] == 't'
                    && text[idx + 3] == 'e'
                    && text[idx + 4] == 's'
                    && text[idx + 5] == 't')
                {
                    idx += 6;
                    builder.Append("latest");
                    return 6;
                }
            }

            return ReadVersionNumber(builder, text, ref idx, leastLeftCntToRead);
        }
        static int ReadVersionNumber(StringBuilder builder, string text, ref int idx, int leastLeftCntToRead)
        {
            int cnt = 0;
            char peek = text[idx];
            //版本号不可能0开头
            if (peek == '0')
            {
                idx++;
                return cnt;
            }
            while (peek >= '0' && peek <= '9')
            {
                builder.Append(peek);
                cnt++;
                //至少要预留这么多字节，否则后面也没法解析
                if (text.Length <= leastLeftCntToRead + idx++)
                    break;
                peek = text[idx];
            }
            return cnt;
        }
    }
}
