using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace Extra
{
    [Serializable]
    public class KnowledgeBase
    {
        int Weight(int count) => Pattern.COUNT - (int)Math.Log(count, 3);

        public string Name { get; }
        int[] counts = new int[Pattern.COMBINATIONS];

        public KnowledgeBase()
        {
            Name = "Unnamed";
        }

        public KnowledgeBase(string name)
        {
            Name = name;
        }

        public int InformationSum => counts.Sum();

        public IContinuousDistribution[] QueryAll(Pattern pattern)
        {
            var result = new IContinuousDistribution[Pattern.COMBINATIONS];
            var valid = new List<int>();
            int sum = 0;
            for (var index = 0; index < Pattern.COMBINATIONS; ++index)
                if (pattern.IsRealization(Pattern.FromIndex(index)))
                {
                    valid.Add(index);
                    sum += counts[index];
                }

            for (var index = 0; index < Pattern.COMBINATIONS; ++index)
                if (valid.Contains(index))
                    result[index] = new Beta(1 + counts[index], 1 + sum - counts[index]);
                else
                    result[index] = new Beta(1, 1 + sum);

            return result;
        }

        public Dictionary<Pattern, IContinuousDistribution> OrderedQuery(Pattern pattern, int max)
        {
            var query = QueryAll(pattern);

            if (max > Pattern.COMBINATIONS)
                max = Pattern.COMBINATIONS;

            return query.Select((d, i) => new KeyValuePair<Pattern, IContinuousDistribution>(Pattern.FromIndex(i), d))
                .OrderByDescending(x => x.Value.Mean).Take(max).ToDictionary(x => x.Key, x => x.Value);
        }

        public void Add(Pattern pattern)
        {
            var w = Weight(pattern.RealizationCount);
            if (w != 0)
                foreach (var ptn in pattern.AllRealizations())
                    counts[ptn.ToIndexOffset()] += w;
        }
    }
}
