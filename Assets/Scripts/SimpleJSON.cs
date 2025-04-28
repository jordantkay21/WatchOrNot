// Source: https://github.com/Bunny83/SimpleJSON
// Very lightweight Unity-compatible JSON parser.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SimpleJSON
{
    public static class JSON
    {
        public static JSONNode Parse(string aJSON)
        {
            return JSONNode.Parse(aJSON);
        }
    }
    public abstract class JSONNode
    {
        public virtual JSONNode this[int index] { get { return null; } set { } }
        public virtual JSONNode this[string key] { get { return null; } set { } }

        public virtual string Value { get { return ""; } set { } }
        public virtual int Count { get { return 0; } }

        public virtual void Add(string key, JSONNode item) { }
        public virtual JSONNode Remove(string key) { return null; }
        public virtual JSONNode Remove(int index) { return null; }
        public virtual JSONNode Remove(JSONNode node) { return node; }

        public virtual IEnumerable<JSONNode> Children { get { yield break; } }

        public override string ToString()
        {
            return "JSONNode";
        }

        public static implicit operator JSONNode(string s)
        {
            return new JSONString(s);
        }

        public static bool operator ==(JSONNode a, object b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(JSONNode a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static JSONNode Parse(string aJSON)
        {
            Stack<JSONNode> stack = new Stack<JSONNode>();
            JSONNode ctx = null;
            int i = 0;
            StringBuilder Token = new StringBuilder();
            string TokenName = "";
            bool QuoteMode = false;
            while (i < aJSON.Length)
            {
                char c = aJSON[i];
                if (c == '"' || c == '\'')
                {
                    QuoteMode = !QuoteMode;
                }
                else if (!QuoteMode && (c == '{'))
                {
                    var obj = new JSONObject();
                    if (ctx != null)
                    {
                        ctx.Add(TokenName, obj);
                    }
                    stack.Push(obj);
                    ctx = obj;
                    TokenName = "";
                    Token.Length = 0;
                }
                else if (!QuoteMode && (c == '['))
                {
                    var arr = new JSONArray();
                    if (ctx != null)
                    {
                        ctx.Add(TokenName, arr);
                    }
                    stack.Push(arr);
                    ctx = arr;
                    TokenName = "";
                    Token.Length = 0;
                }
                else if (!QuoteMode && (c == '}' || c == ']'))
                {
                    if (stack.Count > 0)
                    {
                        stack.Pop();
                        if (stack.Count > 0)
                            ctx = stack.Peek();
                    }
                    TokenName = "";
                    Token.Length = 0;
                }
                else if (!QuoteMode && c == ':')
                {
                    TokenName = Token.ToString();
                    Token.Length = 0;
                }
                else if (!QuoteMode && (c == ','))
                {
                    if (Token.Length > 0)
                    {
                        ctx.Add(TokenName, new JSONString(Token.ToString()));
                        Token.Length = 0;
                    }
                    TokenName = "";
                }
                else
                {
                    Token.Append(c);
                }
                i++;
            }
            if (stack.Count > 0)
                return stack.Pop();
            return new JSONString("");
        }
    }

    public class JSONArray : JSONNode, IEnumerable
    {
        private List<JSONNode> list = new List<JSONNode>();

        public override JSONNode this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        public override int Count
        {
            get { return list.Count; }
        }

        public override void Add(string key, JSONNode item)
        {
            list.Add(item);
        }

        public override IEnumerable<JSONNode> Children
        {
            get { foreach (JSONNode n in list) yield return n; }
        }

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public override string ToString()
        {
            return "JSONArray[" + list.Count + "]";
        }
    }

    public class JSONObject : JSONNode, IEnumerable
    {
        private Dictionary<string, JSONNode> dict = new Dictionary<string, JSONNode>();

        public override JSONNode this[string key]
        {
            get { return dict.ContainsKey(key) ? dict[key] : new JSONString(""); }
            set { dict[key] = value; }
        }

        public override int Count
        {
            get { return dict.Count; }
        }

        public override void Add(string key, JSONNode item)
        {
            dict[key] = item;
        }

        public override IEnumerable<JSONNode> Children
        {
            get { foreach (KeyValuePair<string, JSONNode> n in dict) yield return n.Value; }
        }

        public IEnumerator GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        public override string ToString()
        {
            return "JSONObject[" + dict.Count + "]";
        }
    }

    public class JSONString : JSONNode
    {
        private string data;

        public JSONString(string aData)
        {
            data = aData;
        }

        public override string Value
        {
            get { return data; }
            set { data = value; }
        }

        public override string ToString()
        {
            return data;
        }
    }
}
