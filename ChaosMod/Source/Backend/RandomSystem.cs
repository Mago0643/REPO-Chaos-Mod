using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lego.ChaosMod.Backend;

// i wanted to make this as util that everybody can use, so here it is.

/// <summary>
/// The system generates pseudo-random values according to the following logic:  <para>
/// 1. Checks if the newly generated item exists within a list of maximum size <c>n</c>.  </para>
/// 2. If the item already exists in the list, it re-picks a new value. <para>
/// 3. If it is not present, it appends the item to the list and returns it. </para>
/// 4. To simulate true randomness, there is an <c>N%</c> probability that it skips the duplication check and directly returns the value.
/// </summary>
public class RandomSystem<T> {
    /// <summary>
    /// The full pool of items to pick from. <para>
    /// Assigning a new list copies it and clamps <see cref="maxItemSize"/> to the new list's size. </para>
    /// </summary>
    public IReadOnlyList<T> Items {
        get => _items.AsReadOnly();
        set {
            _items = value.ToList();
            _maxItemSize = Mathf.Clamp(_maxItemSize, 0, _items.Count);
            Reset();
        }
    }

    /// <summary>
    /// The number of recent results remembered for duplicate avoidance. <para>
    /// Automatically clamped to the range [0, number of items]. </para>
    /// </summary>
    public int maxItemSize {
        get => _maxItemSize;
        set => _maxItemSize = Mathf.Clamp(value, 0, _items.Count);
    }

    /// <summary>
    /// The probability (0.0 to 1.0) that the duplicate check is skipped and a value is returned as-is.
    /// Out-of-range values are clamped. Defaults to 0.25 (25%)
    /// </summary>
    public double randomChance {
        get => _randomChance;
        set => _randomChance = Math.Min(Math.Max(value, 0), 1);
    }
    
    private List<T> _items;
    private Queue<int> _results = new();
    private int _maxItemSize = 10;
    private double _randomChance = .25;
    private System.Random rnd;

    /// <summary>
    /// Creates a new <see cref="RandomSystem{T}"/>.
    /// </summary>
    /// <param name="items">The pool of items to pick from.</param>
    /// <param name="maxItemSize">How many recent results to remember when avoiding duplicates.</param>
    /// <param name="randomChance">
    /// The probability (0.0 to 1.0) of skipping the duplicate check. Defaults to 0.25.
    /// </param>
    public RandomSystem(List<T> items, int maxItemSize, double randomChance = 0.25) {
        _items = items;
        this.maxItemSize = maxItemSize;
        this.randomChance = randomChance;

        rnd = new System.Random();
    }

    /// <summary>
    /// Returns a random item that is not among the most recent results.
    /// With probability <see cref="randomChance"/>, the duplicate check is skipped
    /// and the item may repeat a recent result.
    /// </summary>
    /// <returns>
    /// The picked item, or <c>default</c> if the item pool is empty
    /// (an error is logged in that case).
    /// </returns>
    public T? GetValue(List<T> excludes = null!) {
        int idx = GetValueAsInt(excludes);
        return idx == -1 ? default : _items[idx];
    }

    /// <summary>
    /// <see cref="GetValue"/>, but It returns List's index.
    /// </summary>
    /// <returns>Item index</returns>
    public int GetValueAsInt(List<T> excludes = null!) {
        if (_items.Count == 0) {
            Logging.Error("Item list is empty.");
            return -1;
        }
        if (excludes != null && excludes.Count >= _items.Count) {
            Logging.Error("No item to choose. (excludes.Count >= Items.Count)");
            return -1;
        }

        while  (_results.Count > 0 && _results.Count >= _maxItemSize)
            _results.Dequeue();

        int index;
        int loops = 0;
        while (true) {
            if (rnd.NextDouble() <= _randomChance || _results.Count == 0) {
                index = rnd.Next(_items.Count);
            } else {
                HashSet<int> excluded = [.. _results];
                int nope = _items.Count - excluded.Count;
                if (nope <= 0)
                    index = rnd.Next(_items.Count);
                else {
                    int pick = rnd.Next(nope);
                    index = 0;
                    for (int i = 0; i < _items.Count; i++) {
                        if (excluded.Contains(i)) continue;
                        if (pick-- == 0) {
                            index = i;
                            break;
                        }
                    }
                }
            }

            if (excludes == null || !excludes.Contains(_items[index])) break;
            loops++;
            if (loops >= 1000) {
                index = rnd.Next(_items.Count);
                Logging.Warn("Random System: Too many loops");
                break;
            }
        }

        if (_maxItemSize > 0) _results.Enqueue(index);
        return index;
    }

    /// <summary>
    /// Clears the history of recent results, so every item is eligible again.
    /// </summary>
    public void Reset() {
        _results.Clear();
    }
}