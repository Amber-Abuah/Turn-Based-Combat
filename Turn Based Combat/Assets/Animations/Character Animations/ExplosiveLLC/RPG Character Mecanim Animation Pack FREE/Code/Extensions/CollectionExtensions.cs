using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGCharacterAnims.Extensions
{
    public static class CollectionExtensions
    {
        public static T TakeRandom<T>(this ICollection<T> collection)
        { return collection.ElementAt(Random.Range(0, collection.Count)); }
        
        public static T TakeRandom<T>(this T[] array)
        { return array[Random.Range(0, array.Length)]; }
    }
}