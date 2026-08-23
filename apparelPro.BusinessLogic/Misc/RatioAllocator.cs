namespace apparelPro.BusinessLogic.Misc
{
    // Splits a whole-piece target quantity across a set of relative weights
    // (ratios like 1:2:4, or 33:33:33 - there is no requirement that weights
    // sum to 100; a plain division by the sum handles both notations
    // identically). Plain division alone leaves rounding drift (e.g. 100
    // split 1:1:1 gives 33.33/33.33/33.33, summing to 99.99, not 100) - the
    // largest-remainder method used here guarantees the returned quantities
    // always sum EXACTLY to the target, by giving each item its floor share
    // and then handing out the leftover whole pieces to the items with the
    // largest fractional remainder first.
    public static class RatioAllocator
    {
        public static int[] Allocate(int targetTotal, IReadOnlyList<decimal> weights)
        {
            if (weights.Count == 0)
                return Array.Empty<int>();

            var totalWeight = weights.Sum();
            if (totalWeight <= 0)
                throw new InvalidOperationException("Every ratio weight must be greater than zero.");

            var exactShares = new decimal[weights.Count];
            var floorShares = new int[weights.Count];
            for (var i = 0; i < weights.Count; i++)
            {
                exactShares[i] = targetTotal * weights[i] / totalWeight;
                floorShares[i] = (int)Math.Floor(exactShares[i]);
            }

            var allocated = floorShares.Sum();
            var remainder = targetTotal - allocated;

            // Hand out the leftover whole pieces to whichever rows were
            // rounded down the most (largest fractional remainder first),
            // breaking ties by original order for determinism.
            var order = Enumerable.Range(0, weights.Count)
                .OrderByDescending(i => exactShares[i] - floorShares[i])
                .ThenBy(i => i)
                .ToList();

            for (var i = 0; i < remainder; i++)
            {
                floorShares[order[i % order.Count]] += 1;
            }

            return floorShares;
        }
    }
}
