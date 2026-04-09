using BaseClasses_V2.Model;
using ObservableCollections;

namespace BaseClasses_V2.Fluent;

public static class Extensions
{
    /// <summary>
    /// Replaces the node in the database <see cref="db"/>
    /// </summary>
    /// <param name="db">database</param>
    /// <param name="entity">entity to replace</param>
    /// <exception cref="InvalidOperationException">throws if node with given hash was not found</exception>
    public static void Replace(this ObservableHashSet<Element> db, Element entity)
    {
        var numRemoved = db.RemoveWhere(x => x.Hash == entity.Hash);
        if (numRemoved == 1)
            db.Add(entity);
        else
        {
            throw new InvalidOperationException("Attempted to replace a node that does not exist");
        }
    }

    /// <summary>
    /// Replaces the relation in the database <see cref="db"/>
    /// </summary>
    /// <param name="db">database</param>
    /// <param name="entity">entity to replace</param>
    /// <exception cref="InvalidOperationException">throws if relation with given hash was not found</exception>
    public static void Replace(this ObservableHashSet<Relation> db, Relation entity)
    {
        var numRemoved = db.RemoveWhere(x => x.Hash == entity.Hash);
        if (numRemoved == 1)
            db.Add(entity);
        else
        {
            throw new InvalidOperationException("Attempted to replace a relation that does not exist");
        }
    }

    public static int RemoveWhere<T>(this ObservableHashSet<T> set, Func<T, bool> predicate) where T : notnull
    {
        var toRemove = set.Where(predicate).ToList();

        foreach (var item in toRemove)
        {
            set.Remove(item);
        }

        return toRemove.Count;
    }

    public static string Capitilize(this string str)
    {
        return str[0].ToString().ToUpper() + str[1..].ToLower();
    }
}