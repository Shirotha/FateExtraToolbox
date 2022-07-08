using System.Collections.Generic;

namespace Extra
{
    public enum Action : byte
    {
        Unknown = 0,
        Attack = 1,
        Break = 2,
        Guard = 3
    }

    public enum Result : byte
    {
        Even = 0,
        Win = 1,
        Lose = 2,
        Unknown = 3
    }

    public static class ActionUtil
    {
        public static Result CompareTo(this Action self, Action other)
        {
            if (self == Action.Unknown || other == Action.Unknown)
                return Result.Unknown;
            if (self == other)
                return Result.Even;
            if ((self == Action.Attack && other == Action.Break) ||
                (self == Action.Break && other == Action.Guard) ||
                (self == Action.Guard && other == Action.Attack))
                return Result.Win;
            if ((self == Action.Attack && other == Action.Guard) ||
                (self == Action.Break && other == Action.Attack) ||
                (self == Action.Guard && other == Action.Break))
                return Result.Lose;
            return Result.Unknown;
        }

        public static bool IsWellDefined(this Action self) => self == Action.Attack || self == Action.Break || self == Action.Guard;
        public static bool IsUnknown(this Action self) => self == Action.Unknown;

        public static int ToIndex(this IList<Action> self)
        {
            int result = 0;
            int power = 1;
            for (int i = 0; i < self.Count; ++i)
            {
                result += power * (byte)self[i];
                power *= 3;
            }

            return result;
        }
        public static unsafe int ToIndex(byte* self, int count)
        {
            int result = 0;
            int power = 1;
            for (int i = 0; i < count; ++i)
            {
                result += power * self[i];
                power *= 3;
            }

            return result;
        }

        public static List<Action> FromIndex(int index)
        {
            var result = new List<Action>();
            while (index > 0)
            {
                result.Add((Action)((index - 1) % 3 + 1));
                index = (index - 1) / 3;
            }

            return result;
        }
        public static List<Action> FromIndexOffset(int index) => FromIndex(index + Pattern.INDEX_OFFSET);
        public static List<Action> ToActions(this int self) => FromIndex(self);
        public static List<Action> ToActionsOffset(this int self) => FromIndexOffset(self);

        public static Action GetRelated(this Action self, Result result)
        {
            if (!self.IsWellDefined())
                return Action.Unknown;
            if ((self == Action.Attack && result == Result.Even) ||
                (self == Action.Break && result == Result.Win) ||
                (self == Action.Guard && result == Result.Lose))
                return Action.Attack;
            if ((self == Action.Attack && result == Result.Lose) ||
                (self == Action.Break && result == Result.Even) ||
                (self == Action.Guard && result == Result.Win))
                return Action.Break;
            if ((self == Action.Attack && result == Result.Win) ||
                (self == Action.Break && result == Result.Lose) ||
                (self == Action.Guard && result == Result.Even))
                return Action.Guard;
            return Action.Unknown;
        }

        public static Result Reverse(this Result self)
        {
            if (self == Result.Win)
                return Result.Lose;
            if (self == Result.Lose)
                return Result.Win;
            return self;
        }

        public static List<Action> ToActions(this string s)
        {
            var result = new List<Action>();
            foreach (var c in s)
                switch (c)
                {
                    case 'A': result.Add(Action.Attack); break;
                    case 'B': result.Add(Action.Break); break;
                    case 'G': result.Add(Action.Guard); break;
                    default: result.Add(Action.Unknown); break;
                }

            return result;
        }
    }
}
