﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.ContentManagement {
    public class ContentIdentity {
        private readonly Dictionary<string, string> _dictionary;
        private int _currentIdentityPriority = int.MinValue; //initialise to lowest possible priority
        private string _encodedIdentity = null;

        public ContentIdentity() {
            _dictionary = new Dictionary<string, string>();
        }

        public ContentIdentity(string identity) {
            _dictionary = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(identity)) {
                var identityEntries = GetIdentityEntries(identity);
                foreach (var identityEntry in identityEntries) {
                    var keyValuePair = GetIdentityKeyValue(identityEntry);
                    if (keyValuePair != null) {
                        Add(keyValuePair.Value.Key, UnencodeIdentityValue(keyValuePair.Value.Value));
                    }
                }
            }
        }

        public void Add(string name, string value) {
            Add(name, value, 0/*default priority*/);
        }

        public void Add(string name, string value, int priority) {
            if (priority < _currentIdentityPriority)
                return; //lower priority, so ignore
            if (priority > _currentIdentityPriority)
                _dictionary.Clear(); //higher, so override and delete existing

            //save the current highest priority
            _currentIdentityPriority = priority;

            //if equal or higher priority add to identity collection
            if (_dictionary.ContainsKey(name)) {
                _dictionary[name] = value;
            }
            else {
                _dictionary.Add(name, value);
            }
            _encodedIdentity = null;
        }

        public string Get(string name) {
            return _dictionary.ContainsKey(name) ? _dictionary[name] : null;
        }

        public override string ToString() {
            if (_encodedIdentity != null)
                return _encodedIdentity;

            var stringBuilder = new StringBuilder();
            foreach (var key in _dictionary.Keys.OrderBy(key => key)) {
                var escapedIdentity = EncodeIdentityValue(_dictionary[key]);
                stringBuilder.Append("/" + key + "=" + escapedIdentity);
            }
            _encodedIdentity = stringBuilder.ToString();
            return _encodedIdentity;
        }

        private static string EncodeIdentityValue(string identityValue) {
            if(String.IsNullOrEmpty(identityValue)) {
                return "";
            }

            var stringBuilder = new StringBuilder();
            foreach (var ch in identityValue.ToCharArray()) {
                switch (ch) {
                    case '\\':
                        stringBuilder.Append('\\');
                        stringBuilder.Append('\\');
                        break;
                    case '/':
                        stringBuilder.Append('\\');
                        stringBuilder.Append('/');
                        break;
                    default:
                        stringBuilder.Append(ch);
                        break;
                }
            }
            return stringBuilder.ToString();
        }

        private static string UnencodeIdentityValue(string identityValue) {
            var stringBuilder = new StringBuilder();
            var identityChars = identityValue.ToCharArray();
            var length = identityChars.Length;
            for (int i = 0; i < length; i++) {
                switch (identityChars[i]) {
                    case '\\':
                        if (i + 1 < length) {
                            if (identityChars[i + 1] == '\\') {
                                stringBuilder.Append('\\');
                                i++;
                            }
                        }
                        else {
                            stringBuilder.Append('\\');
                        }
                        break;
                    default:
                        stringBuilder.Append(identityChars[i]);
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        private static IEnumerable<string> GetIdentityEntries(string identity) {
            var identityEntries = new List<string>();
            var stringBuilder = new StringBuilder();
            var escaping = false;
            foreach (var ch in identity.ToCharArray()) {
                if (escaping) {
                    stringBuilder.Append(ch);
                    escaping = false;
                }
                else {
                    if (ch == '/') {
                        if (stringBuilder.Length > 0) {
                            identityEntries.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                        stringBuilder.Append(ch);
                    }
                    else {
                        if (ch == '\\') {
                            escaping = true;
                        }
                        stringBuilder.Append(ch);
                    }
                }
            }
            identityEntries.Add(stringBuilder.ToString());

            return identityEntries;
        }

        private static KeyValuePair<string, string>? GetIdentityKeyValue(string identityEntry) {
            if (String.IsNullOrWhiteSpace(identityEntry)) return null;
            if (!identityEntry.StartsWith("/")) return null;
            var indexOfEquals = identityEntry.IndexOf("=");
            if (indexOfEquals < 0) return null;

            var key = identityEntry.Substring(1, indexOfEquals - 1);
            var value = identityEntry.Substring(indexOfEquals + 1);

            return new KeyValuePair<string, string>(key, value);
        }


        public class ContentIdentityEqualityComparer : IEqualityComparer<ContentIdentity> {
            public bool Equals(ContentIdentity contentIdentity1, ContentIdentity contentIdentity2) {
                return contentIdentity1.ToString().Equals(contentIdentity2.ToString());
            }

            public int GetHashCode(ContentIdentity contentIdentity) {
                return contentIdentity.ToString().GetHashCode();
            }
        }

    }
}
