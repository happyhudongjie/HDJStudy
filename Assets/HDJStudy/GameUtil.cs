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
}
