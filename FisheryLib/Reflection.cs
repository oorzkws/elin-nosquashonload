// Copyright (c) 2022 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FisheryLib;

//[PublicAPI]
public static class Reflection
{
	public static bool AreAssignableFrom(this IEnumerable<Type> types, IEnumerable<Type> targetTypes)
		=> targetTypes.AreAssignableTo(types);

	public static bool AreAssignableTo(this IEnumerable<Type> types, IEnumerable<Type> targetTypes)
	{
		if (types is null) throw new ArgumentNullException();
		if (targetTypes is null) throw new ArgumentNullException();

		using var typesEnumerator = types.GetEnumerator();
		using var targetTypesEnumerator = targetTypes.GetEnumerator();

		while (typesEnumerator.MoveNext())
		{
			if (!targetTypesEnumerator.MoveNext()
				|| !targetTypesEnumerator.Current!.IsAssignableFrom(typesEnumerator.Current))
				return false;
		}

		return !targetTypesEnumerator.MoveNext();
	}
	
	/// <summary>
	/// Finds a constructor with parameters that are either assignable to or assignable from the types in the provided
	/// parameters array
	/// </summary>
	/// <param name="type">The type of the object to construct</param>
	/// <param name="parameters">
	/// An array of parameter types that are either assignable to or from the parameters of a constructor for the
	/// target type
	/// </param>
	/// <param name="searchForStatic">Search for static constructors instead of instance constructors</param>
	/// <param name="throwOnFailure">Throw an exception on failure to find a matching constructor</param>
	/// <returns>
	/// A ConstructorInfo that best matches the provided arguments. Null on failure if throwOnFailure is set to false
	/// </returns>
	public static ConstructorInfo? MatchingConstructor(Type type, Type[]? parameters = null,
		bool searchForStatic = false, bool throwOnFailure = true)
	{
		if (type is null) throw new ArgumentNullException();

		parameters ??= Array.Empty<Type>();
		var flags = searchForStatic ? AccessTools.all & ~BindingFlags.Instance : AccessTools.all & ~BindingFlags.Static;

		return TryGetConstructor(type, flags, paramTypes => paramTypes.AreAssignableFrom(parameters))
			?? TryGetConstructor(type, flags, paramTypes => paramTypes.AreAssignableTo(parameters))
			?? (throwOnFailure
				? throw new InvalidOperationException(
					$"No constructor found for type: {type}, parameters: {
						parameters.ToStringSafeEnumerable()}, static: {searchForStatic}, found constructors: {
						type.GetConstructors(flags).ToStringSafeEnumerable()}")
				: null);
	}

	private static ConstructorInfo? TryGetConstructor(Type type, BindingFlags flags,
		Predicate<IEnumerable<Type>> predicate)
		=> type.GetConstructors(flags).FirstOrDefault(c
			=> predicate(c.GetParameters().Select(static p => p.ParameterType)));
	
	[SecuritySafeCritical]
	public static string FullDescription(this MemberInfo? memberInfo)
		=> memberInfo switch
		{
			MethodBase methodBase => GeneralExtensions.FullDescription(methodBase),
			Type type => GeneralExtensions.FullDescription(type),
			_ => FullDescriptionInternal(memberInfo)
		};
	
	private static string FullDescriptionInternal(MemberInfo? memberInfo)
	{
		if (memberInfo is null)
			return "NULL";
		
		var stringBuilder = new StringBuilder();

		switch (memberInfo)
		{
			case FieldInfo fieldInfo:
				AppendFieldPrefix(fieldInfo, stringBuilder);
				break;
			case PropertyInfo propertyInfo:
				AppendPropertyPrefix(propertyInfo, stringBuilder);
				break;
			case EventInfo eventInfo:
				AppendEventPrefix(eventInfo, stringBuilder);
				break;
		}

		if (memberInfo.DeclaringType != null)
			stringBuilder.AppendType(memberInfo.DeclaringType).Append("::");
		
		stringBuilder.Append(memberInfo.Name);

		if (memberInfo is PropertyInfo propertyInfo1)
			AppendPropertyPostfix(stringBuilder, propertyInfo1);

		return stringBuilder.ToString();
	}

	private static void AppendFieldPrefix(FieldInfo fieldInfo, StringBuilder stringBuilder)
	{
		if (fieldInfo.IsPublic)
			stringBuilder.Append("public ");

		if (fieldInfo is { IsLiteral: true, IsInitOnly: false })
			stringBuilder.Append("const ");
		else if (fieldInfo.IsStatic)
			stringBuilder.Append("static ");

		stringBuilder.AppendType(fieldInfo.FieldType).Append(' ');
	}

	private static void AppendPropertyPrefix(PropertyInfo propertyInfo, StringBuilder stringBuilder)
	{
		if ((propertyInfo.GetMethod?.IsPublic ?? false)
			|| (propertyInfo.SetMethod?.IsPublic ?? false))
		{
			stringBuilder.Append("public ");
		}

		if ((propertyInfo.GetMethod?.IsStatic ?? true)
			&& (propertyInfo.SetMethod?.IsStatic ?? true))
		{
			stringBuilder.Append("static ");
		}

		stringBuilder.AppendType(propertyInfo.PropertyType).Append(' ');
	}

	private static void AppendPropertyPostfix(StringBuilder stringBuilder, PropertyInfo propertyInfo)
	{
		stringBuilder.Append(" { ");

		var getMethod = propertyInfo.GetMethod;
		var setMethod = propertyInfo.SetMethod;
		
		if (getMethod is not null)
		{
			if (getMethod.IsPrivate && !(setMethod?.IsPrivate ?? true))
				stringBuilder.Append("private ");

			stringBuilder.Append("get; ");
		}

		if (setMethod is not null)
		{
			if (setMethod.IsPrivate && !(getMethod?.IsPrivate ?? true))
				stringBuilder.Append("private ");

			stringBuilder.Append("set; ");
		}

		stringBuilder.Append('}');
	}

	private static void AppendEventPrefix(EventInfo eventInfo, StringBuilder stringBuilder)
	{
		if ((eventInfo.AddMethod?.IsPublic ?? true)
			&& (eventInfo.RemoveMethod?.IsPublic ?? true))
		{
			stringBuilder.Append("public ");
		}

		if ((eventInfo.AddMethod?.IsStatic ?? true)
			&& (eventInfo.RemoveMethod?.IsStatic ?? true))
		{
			stringBuilder.Append("static ");
		}

		stringBuilder.Append("event ");

		stringBuilder.AppendType(eventInfo.EventHandlerType).Append(' ');
	}

	private static StringBuilder AppendType(this StringBuilder stringBuilder, Type type)
		=> stringBuilder.Append(type.FullDescription());
}