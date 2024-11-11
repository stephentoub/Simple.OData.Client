using Simple.OData.Client.Extensions;

namespace Simple.OData.Client;

public static class ODataNameMatchResolver
{
	public static INameMatchResolver Strict = new ExactMatchResolver();
	public static INameMatchResolver Alpahumeric = new ExactMatchResolver(true);
	public static INameMatchResolver AlpahumericCaseInsensitive = new ExactMatchResolver(true, StringComparison.InvariantCultureIgnoreCase);
	public static INameMatchResolver NotStrict = new BestMatchResolver();

	internal static string LastPostDotSegment(string input)
	{
		int lastDot = input.LastIndexOf('.');
		return lastDot >= 0 ? input.Substring(lastDot + 1) : input;
	}
}

public static class Pluralizers
{
	public static IPluralizer Simple = new SimplePluralizer();
	public static IPluralizer Cached = new CachedPluralizer(Simple);
}

public class ExactMatchResolver(bool alphanumComparison = false, StringComparison stringComparison = StringComparison.InvariantCulture) : INameMatchResolver
{
	private readonly StringComparison _stringComparison = stringComparison;
	private readonly bool _alphanumComparison = alphanumComparison;

	public bool IsMatch(string actualName, string requestedName)
	{
		actualName = ODataNameMatchResolver.LastPostDotSegment(actualName);
		requestedName = ODataNameMatchResolver.LastPostDotSegment(requestedName);
		if (_alphanumComparison)
		{
			actualName = actualName.Homogenize();
			requestedName = requestedName.Homogenize();
		}

		return actualName.Equals(requestedName, _stringComparison);
	}
}

public class BestMatchResolver : INameMatchResolver
{
	private readonly IPluralizer _pluralizer;

	public BestMatchResolver()
	{
		_pluralizer = Pluralizers.Cached;
	}

	public bool IsMatch(string actualName, string requestedName)
	{
		actualName = ODataNameMatchResolver.LastPostDotSegment(actualName).Homogenize();
		requestedName = ODataNameMatchResolver.LastPostDotSegment(requestedName).Homogenize();

		return actualName == requestedName ||
			   (actualName == _pluralizer.Singularize(requestedName) ||
				actualName == _pluralizer.Pluralize(requestedName) ||
				_pluralizer.Singularize(actualName) == requestedName ||
				_pluralizer.Pluralize(actualName) == requestedName);
	}
}
