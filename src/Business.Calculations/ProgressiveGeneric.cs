using System.Numerics;

namespace Business.Calculations;

public class Progressive<T, TMultiplier>
    where T :
        ISubtractionOperators<T, T, T>,
        IAdditionOperators<T, T, T>,
        IMultiplyOperators<T, TMultiplier, T>,
        IComparisonOperators<T, T, bool>,
        new()
    where TMultiplier : IMultiplyOperators<TMultiplier, T, T>
{
    private readonly List<Bracket> brackets = new List<Bracket>();

    private static T Zero = new T();

    public record Bracket
    {
        public T From { get; set; }

        public T To { get; set; }

        public TMultiplier Multiplier { get; set; }

        public bool ToInfinity { get; internal set; }

        internal T Size => To - From;

        public T Calculate(T value) => value * Multiplier;
    }

    public RateBuilder UpTo(T toValue) => new BracketRateBuilder(brackets).UpTo(Enforce.GreaterThanZero(toValue));

    public ToBuilder From(T fromValue) => new BracketRateBuilder(brackets).From(Enforce.GreaterThanOrZero(fromValue));

    public Progressive<T, TMultiplier> AddBracket(T fromValue, T toValue, TMultiplier multiplier)
    {
        brackets.Add(new Bracket { From = fromValue, To = toValue, Multiplier = multiplier });
        return this;
    }

    public class BracketRateBuilder
    {
        private List<Bracket> brackets;

        public BracketRateBuilder(List<Bracket> brackets)
        {
            this.brackets = brackets;
        }

        public ToBuilder From(T fromValue)
        {
            return new ToBuilder(brackets, fromValue);
        }

        public RateBuilder UpTo(T toValue) => From(Zero).UpTo(toValue);
    }

    public class FromBuilder
    {
        private List<Bracket> brackets;

        public FromBuilder(List<Bracket> brackets)
        {
            this.brackets = brackets;
        }

        public ToBuilder From(T fromValue)
        {
            return new ToBuilder(brackets, fromValue);
        }

        public RateBuilder UpTo(T toValue) => From(brackets.Last().To).UpTo(toValue);

        public ResultBuilder ReminderMultiplier(TMultiplier multipier)
        {
            From(brackets.Last().To).UpToInfinity().MultiplyBy(multipier);

            return new ResultBuilder(brackets);
        }

        public ResultBuilder Capped()
        {
            return new ResultBuilder(brackets);
        }
    }

    public class ResultBuilder
    {
        private List<Bracket> brackets;

        public ResultBuilder(List<Bracket> brackets)
        {
            this.brackets = brackets;
        }

        public BracketCalculationResult Calculate(T value)
        {
            BracketsAccumulator seed = new()
            {
                Remaining = value
            };

            return brackets.Aggregate(seed, Allocate, Calculate);

            static BracketsAccumulator Allocate(BracketsAccumulator accumulator, Bracket currentBracket)
            {
                T allocationAmmount = accumulator.Remaining < currentBracket.Size || currentBracket.ToInfinity ?
                    accumulator.Remaining : currentBracket.Size;

                accumulator.BracketsAllocation.Add(new BracketsAccumulator.BracketAllocation
                {
                    Bracket = currentBracket,
                    Amount = allocationAmmount
                });

                accumulator.Remaining -= allocationAmmount;

                return accumulator;
            }

            static BracketCalculationResult Calculate(BracketsAccumulator accumulator)
            {
                var breakdown = accumulator.BracketsAllocation
                    .Select(allocation => new BracketResult
                    {
                        Amount = allocation.Bracket.Calculate(allocation.Amount),
                        Bracket = allocation.Bracket
                    }).ToArray();

                return new BracketCalculationResult
                {
                    Amount = SumAmounts(breakdown),
                    Breakdown = breakdown
                };

                static T SumAmounts(BracketResult[] values)
                {
                    T aggregate = Zero;

                    foreach (BracketResult value in values)
                        aggregate += value.Amount;

                    return aggregate;
                }
            }
        }
    }

    public class BracketsAccumulator
    {
        public T Remaining;
        public List<BracketAllocation> BracketsAllocation = new();

        public class BracketAllocation
        {
            public Bracket Bracket { get; set; }
            public T Amount { get; set; }
        }
    }

    public class BracketCalculationResult
    {
        public T Amount { get; set; }

        public BracketResult[] Breakdown { get; set; }
    }

    public class BracketResult
    {
        public Bracket Bracket { get; set; }

        public T Amount { get; set; }
    }

    public class ToBuilder
    {
        private readonly T fromValue;
        private readonly List<Bracket> brackets;

        public ToBuilder(List<Bracket> brackets, T fromValue)
        {
            this.fromValue = fromValue;
            this.brackets = brackets;
        }

        internal RateBuilder UpTo(T toValue)
        {
            return new RateBuilder(brackets, fromValue, toValue, false);
        }

        internal RateBuilder UpToInfinity()
        {
            return new RateBuilder(brackets, fromValue, new T(), true);
        }
    }

    public class RateBuilder
    {
        private readonly T fromValue;
        private readonly T toValue;
        private readonly List<Bracket> brackets;
        private readonly bool toInfinity;

        public RateBuilder(List<Bracket> brackets, T fromValue, T toValue, bool toInfinity)
        {
            this.fromValue = fromValue;
            this.toValue = toValue;
            this.toInfinity = toInfinity;
            this.brackets = brackets;
        }

        public FromBuilder MultiplyBy(TMultiplier multiplier)
        {
            brackets.Add(new Bracket { From = fromValue, To = toValue, Multiplier = multiplier, ToInfinity = toInfinity });
            return new FromBuilder(brackets);
        }
    }

    private class Enforce
    {
        internal static T GreaterThanOrZero(T fromValue)
        {
            throw new NotImplementedException();
        }

        internal static T GreaterThanZero(T toValue)
        {
            throw new NotImplementedException();
        }
    }
}