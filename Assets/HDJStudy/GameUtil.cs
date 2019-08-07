using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameUtil  {

	public static bool Any<T> (this IEnumerable<T> dataset, Func<T,bool> predicate){
		if (dataset == null)
			return false;
		if (predicate == null)
			return false;
		foreach (var data in dataset) {
			if (predicate (data))
				return true;
		}
		return false;
	}

	/// <summary>
	/// 将<see cref="T:System.Collections.IEnumerable"/> 的元素转换为指定的类型
	/// </summary>
	/// 
	/// <returns>
	/// 一个<see cref="T:System.Collections.Generic.IEnumrable^1">,包含已转换为指定类型的源序列的每个元素。
	/// 
	/// </returns>
	/// <param name="source">Source.</param>
	/// <typeparam name="TResult">The 1st type parameter.</typeparam>
	public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source){
	
		IEnumerable<TResult> enumerable = source as IEnumerable<TResult>;
		if (enumerable != null)
			return enumerable;
		if (source == null)
			throw new ArgumentNullException ("source");
		return CastIterator<TResult> (source);
	}

	private static IEnumerable<TResult> CastIterator<TResult>(IEnumerable source){
		foreach (object obj in source) {
			yield return (TResult)obj;
		}
	}

	/// <summary>
	/// 返回序列中满足条件的第一个原色；如果未找到这样的元素，则返回默认值
	/// </summary>
	/// <returns>The or default.</returns>
	/// <param name="source">Source.</param>
	/// <param name="predicate">Predicate.</param>
	/// <typeparam name="TSource">The 1st type parameter.</typeparam>
	public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source,Func<TSource,bool> predicate){
		if (source == null) 
			throw new ArgumentNullException ("source");
		if(predicate == null)
			throw new ArgumentNullException ("predicate");
		foreach (TSource sourcel in source) {
			if (predicate (sourcel))
				return sourcel;
		}
		return default(TSource);
	}
}

