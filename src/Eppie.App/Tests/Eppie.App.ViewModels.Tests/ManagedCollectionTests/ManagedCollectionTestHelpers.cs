// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System.ComponentModel;
using Tuvi.App.ViewModels;

namespace Eppie.App.ViewModels.Tests.ManagedCollectionTests
{
    internal static class ManagedCollectionTestHelpers
    {
        internal sealed class StartsWithFilter : ISearchFilter<string>
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            private string _searchText = string.Empty;
            public string SearchText
            {
                get => _searchText;
                set
                {
                    if (!string.Equals(_searchText, value, StringComparison.Ordinal))
                    {
                        _searchText = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                    }
                }
            }

            public bool ItemPassedFilter(string item)
            {
                if (item is null)
                {
                    return false;
                }

                return string.IsNullOrEmpty(SearchText) || item.StartsWith(SearchText, StringComparison.Ordinal);
            }
        }

        internal sealed class StubExtendedComparer : IExtendedComparer<string>
        {
            public int Compare(string? x, string? y) => string.Compare(x, y, StringComparison.Ordinal);
            public bool Equals(string? x, string? y) => string.Equals(x, y, StringComparison.Ordinal);
            public int GetHashCode(string obj) => obj?.GetHashCode(StringComparison.Ordinal) ?? 0;
        }

        internal sealed class ReverseStubExtendedComparer : IExtendedComparer<string>
        {
            public int Compare(string? x, string? y) => string.Compare(y, x, StringComparison.Ordinal);
            public bool Equals(string? x, string? y) => string.Equals(x, y, StringComparison.Ordinal);
            public int GetHashCode(string obj) => obj?.GetHashCode(StringComparison.Ordinal) ?? 0;
        }

        internal sealed class PredicateFilter : IFilter<string>
        {
            private readonly Func<string, bool> _predicate;
            public PredicateFilter(Func<string, bool> predicate) => _predicate = predicate;
            public bool ItemPassedFilter(string item) => _predicate(item);
        }

        internal sealed class TestItem
        {
            public string Name { get; set; }
            public TestItem(string name) => Name = name;
            public override string ToString() => Name;
        }

        internal sealed class TestItemFilter : IFilter<TestItem>
        {
            private readonly Func<TestItem, bool> _predicate;
            public TestItemFilter(Func<TestItem, bool> predicate) => _predicate = predicate;
            public bool ItemPassedFilter(TestItem item) => _predicate(item);
        }

        internal sealed class TestItemComparer : IExtendedComparer<TestItem>
        {
            public int Compare(TestItem? x, TestItem? y)
                => string.Compare(x?.Name, y?.Name, StringComparison.Ordinal);

            public bool Equals(TestItem? x, TestItem? y)
                => string.Equals(x?.Name, y?.Name, StringComparison.Ordinal);

            public int GetHashCode(TestItem obj)
                => obj.Name?.GetHashCode(StringComparison.Ordinal) ?? 0;
        }

        internal sealed class ProbeFilter : ISearchFilter<string>
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public string SearchText { get; set; } = string.Empty;

            public bool AllowAll { get; set; } = true;

            public bool ItemPassedFilter(string item) => AllowAll;

            public void RaiseChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal class ReconcileTestItemFilter : IFilter<ReconcileTestItem>
        {
            private readonly Func<int, bool> _predicate;
            public ReconcileTestItemFilter(Func<int, bool> predicate) => _predicate = predicate;
            public bool ItemPassedFilter(ReconcileTestItem item) => _predicate(item.Id);
        }

        internal class ReconcileTestItemFilterString : IFilter<ReconcileTestItem>
        {
            private readonly Func<string, bool> _predicate;
            public ReconcileTestItemFilterString(Func<string, bool> predicate) => _predicate = predicate;
            public bool ItemPassedFilter(ReconcileTestItem item) => _predicate(item.Name ?? string.Empty);
        }

        internal class ReconcileTestItemComparer : IExtendedComparer<ReconcileTestItem>
        {
            public int Compare(ReconcileTestItem? x, ReconcileTestItem? y) => string.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
            public System.ComponentModel.ListSortDirection SortDirection { get; set; }
            public bool Equals(ReconcileTestItem? x, ReconcileTestItem? y) => (x?.Id ?? 0) == (y?.Id ?? 0);
            public int GetHashCode(ReconcileTestItem obj) => obj?.Id ?? 0;
        }

        internal class ReconcileStringComparer : IExtendedComparer<string>
        {
            public int Compare(string? x, string? y) => string.Compare(x, y, StringComparison.Ordinal);
            public System.ComponentModel.ListSortDirection SortDirection { get; set; }
            public bool Equals(string? x, string? y) => string.Equals(x, y, StringComparison.Ordinal);
            public int GetHashCode(string obj) => obj?.GetHashCode(StringComparison.Ordinal) ?? 0;
        }

        internal class TestSearchFilter : ISearchFilter<string>
        {
            public string SearchText { get; set; } = string.Empty;

            public event PropertyChangedEventHandler? PropertyChanged;

            public bool ItemPassedFilter(string item)
            {
                if (string.IsNullOrEmpty(SearchText)) return true;
                return item != null && item.Contains(SearchText, StringComparison.Ordinal);
            }

            public void OnSearchTextChanged()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
            }
        }

        internal class ReconcileTestItem : IEquatable<ReconcileTestItem>
        {
            public int Id { get; set; }
            public string? Name { get; set; }

            public bool Equals(ReconcileTestItem? other)
            {
                if (other is null) return false;
                return this.Id == other.Id; // Identity based on ID
            }

            public override bool Equals(object? obj) => Equals(obj as ReconcileTestItem);
            public override int GetHashCode() => Id;
            public override string ToString() => $"{Id}:{Name}";
        }

        internal sealed class Token
        {
            public string Key { get; }

            public Token(string key)
            {
                Key = key;
            }

            public override string ToString()
            {
                return Key;
            }
        }

        internal sealed class EquatableToken : IEquatable<EquatableToken>
        {
            public string EmailKey { get; }
            public string Display { get; set; } = string.Empty;
            public string Payload { get; set; } = string.Empty;

            public EquatableToken(string emailKey)
            {
                EmailKey = emailKey;
            }

            public bool Equals(EquatableToken? other)
            {
                return other != null && string.Equals(EmailKey, other.EmailKey, StringComparison.OrdinalIgnoreCase);
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as EquatableToken);
            }

            public override int GetHashCode()
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(EmailKey);
            }

            public override string ToString()
            {
                return $"{Display}<{EmailKey}>";
            }
        }
    }
}
