﻿using PCRE;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Upgrade
{
	internal static class ReplacementPattern
	{
		public static Func<PcreMatch, string> Parse(string replacementPattern)
		{
			if (replacementPattern == null)
				throw new ArgumentNullException(nameof(replacementPattern));

			if (replacementPattern.Length == 0)
				return _ => string.Empty;

#if NETCOREAPP
			var placeholderIndex = replacementPattern.IndexOf('$', StringComparison.Ordinal);
#else
			var placeholderIndex = replacementPattern.IndexOf('$');
#endif

			if (placeholderIndex < 0)
				return _ => replacementPattern;

			var parts = new List<ReplacementPart>();
			var idx = 0;

			if (placeholderIndex > 0)
			{
				idx = placeholderIndex;
				parts.Add(new LiteralPart(replacementPattern, 0, idx));
			}

			while (true)
			{
				if (idx >= replacementPattern.Length)
					break;

				if (replacementPattern[idx] == '$')
				{
					++idx;

					if (idx >= replacementPattern.Length)
					{
						parts.Add(LiteralPart.Dollar);
						break;
					}

					switch (replacementPattern[idx])
					{
					case '$':
						parts.Add(LiteralPart.Dollar);
						++idx;
						break;

					case '&':
						parts.Add(IndexedGroupPart.FullMatch);
						++idx;
						break;

					case '`':
						parts.Add(PreMatchPart.Instance);
						++idx;
						break;

					case '\'':
						parts.Add(PostMatchPart.Instance);
						++idx;
						break;

					case '_':
						parts.Add(FullInputPart.Instance);
						++idx;
						break;

					case '+':
						parts.Add(LastMatchedGroupPart.Instance);
						++idx;
						break;

					case '{':
						{
							var startIdx = idx;
							while (idx < replacementPattern.Length && replacementPattern[idx] != '}')
								++idx;

							if (idx < replacementPattern.Length && replacementPattern[idx] == '}' && idx > startIdx + 1)
							{
								var groupName = replacementPattern.Substring(startIdx + 1, idx - startIdx - 1);
								var fallback = new LiteralPart(replacementPattern, startIdx - 1, idx - startIdx + 2);
								parts.Add(new NamedGroupPart(groupName, fallback));
								++idx;
								break;
							}

							parts.Add(new LiteralPart(replacementPattern, startIdx - 1, idx - startIdx + 1));
							break;
						}

					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						{
							var startIdx = idx;
							while (idx < replacementPattern.Length && replacementPattern[idx] >= '0' && replacementPattern[idx] <= '9')
								++idx;

							var fallback = new LiteralPart(replacementPattern, startIdx - 1, idx - startIdx + 1);

#if NETCOREAPP
							var groupIndexString = replacementPattern.AsSpan(startIdx, idx - startIdx);
#else
							var groupIndexString = replacementPattern.Substring(startIdx, idx - startIdx);
#endif

							if (int.TryParse(groupIndexString, NumberStyles.None, CultureInfo.InvariantCulture, out var groupIndex))
							{
								parts.Add(new IndexedGroupPart(groupIndex, fallback));
								break;
							}

							parts.Add(fallback);
							break;
						}

					default:
						parts.Add(new LiteralPart(replacementPattern, idx - 1, 2));
						++idx;
						break;
					}
				}
				else
				{
					var startIdx = idx;
					while (idx < replacementPattern.Length && replacementPattern[idx] != '$')
						++idx;

					parts.Add(new LiteralPart(replacementPattern, startIdx, idx - startIdx));
				}
			}

			return match =>
			{
				var sb = new StringBuilder();
				foreach (var part in parts)
					part.Append(match, sb);
				return sb.ToString();
			};
		}

		private abstract class ReplacementPart
		{
			private static PropertyInfo[] props_ = typeof(PcreMatch).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
			private static MethodInfo subject_ = props_.First((m) => m.Name == "Subject").GetGetMethod(true);

			protected string Subject(PcreMatch match)
			{
				return subject_.Invoke(match, null) as string;
			}

			public abstract void Append(PcreMatch match, StringBuilder sb);
			public abstract override string ToString();
		}

		private sealed class LiteralPart : ReplacementPart
		{
			public static readonly LiteralPart Dollar = new LiteralPart("$");

			private readonly string _text;
			private readonly int _startIndex;
			private readonly int _length;

			private LiteralPart(string text)
			{
				_text = text;
				_startIndex = 0;
				_length = _text.Length;
			}

			public LiteralPart(string text, int startIndex, int length)
			{
				_text = text;
				_startIndex = startIndex;
				_length = length;
			}

			public override void Append(PcreMatch match, StringBuilder sb)
				=> sb.Append(_text, _startIndex, _length);

			public override string ToString()
				=> $"Literal: {_text.Substring(_startIndex, _length)}";
		}

		private sealed class IndexedGroupPart : ReplacementPart
		{
			public static readonly IndexedGroupPart FullMatch = new IndexedGroupPart(0, null);

			private readonly int _index;
			private readonly ReplacementPart? _fallback;

			public IndexedGroupPart(int index, ReplacementPart? fallback)
			{
				_index = index;
				_fallback = fallback;
			}

			public override void Append(PcreMatch match, StringBuilder sb)
			{
				if (match.TryGetGroup(_index, out var group))
				{
					if (group.Success)
						sb.Append(Subject(match), group.Index, group.Length);
				}
				else
				{
					_fallback?.Append(match, sb);
				}
			}

			public override string ToString()
				=> _index == 0
					? "Full match"
					: $"Group: #{_index}";
		}

		private sealed class NamedGroupPart : ReplacementPart
		{
			private readonly string _name;
			private readonly int _index;
			private readonly ReplacementPart? _fallback;

			public NamedGroupPart(string name, ReplacementPart? fallback)
			{
				_name = name;
				_fallback = fallback;

				if (!int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out _index))
					_index = -1;
			}

			public override void Append(PcreMatch match, StringBuilder sb)
			{
				if (match.TryGetGroup(_name, out var group) || match.TryGetGroup(_index, out group))
				{
					if (group.Success)
						sb.Append(Subject(match), group.Index, group.Length);
				}
				else
				{
					_fallback?.Append(match, sb);
				}
			}

			public override string ToString()
				=> $"Group: {_name}";
		}

		private sealed class PreMatchPart : ReplacementPart
		{
			public static readonly PreMatchPart Instance = new PreMatchPart();

			public override void Append(PcreMatch match, StringBuilder sb)
				=> sb.Append(Subject(match), 0, match.Index);

			public override string ToString()
				=> "Pre match";
		}

		private sealed class PostMatchPart : ReplacementPart
		{
			public static readonly PostMatchPart Instance = new PostMatchPart();

			public override void Append(PcreMatch match, StringBuilder sb)
			{
				var endOfMatch = match.EndIndex;
				sb.Append(Subject(match), endOfMatch, Subject(match).Length - endOfMatch);
			}

			public override string ToString()
				=> "Post match";
		}

		private sealed class FullInputPart : ReplacementPart
		{
			public static readonly FullInputPart Instance = new FullInputPart();

			public override void Append(PcreMatch match, StringBuilder sb)
				=> sb.Append(Subject(match));

			public override string ToString()
				=> "Full input";
		}

		private sealed class LastMatchedGroupPart : ReplacementPart
		{
			public static readonly LastMatchedGroupPart Instance = new LastMatchedGroupPart();

			public override void Append(PcreMatch match, StringBuilder sb)
			{
				for (var i = match.CaptureCount; i > 0; --i)
				{
					if (match.TryGetGroup(i, out var group) && group.Success)
					{
						sb.Append(Subject(match), group.Index, group.Length);
						return;
					}
				}
			}

			public override string ToString()
				=> "Last matched group";
		}
	}
}
