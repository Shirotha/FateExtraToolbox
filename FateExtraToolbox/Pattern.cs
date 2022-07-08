using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Extra
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Pattern
    {
        public const int COUNT = 6;
        public static readonly int COMBINATIONS = (int)Math.Pow(3, COUNT);
        public static readonly int INDEX_OFFSET = ((int)Math.Pow(3, COUNT) - 1) / 2;

        fixed byte actions[COUNT];

        public static Pattern FromIndex(int index)
        {
            var actions = index.ToActionsOffset();

            if (actions.Count != COUNT)
                throw new ArgumentException();
            
            var result = new Pattern();

            for (int i = 0; i < COUNT; ++i)
                result[i] = actions[i];

            return result;
        }

        public static Pattern FromString(string s)
        {
            if (s.Length != COUNT)
                throw new ArgumentException();

            var result = new Pattern();

            var actions = s.ToActions();
            for (int i = 0; i < COUNT; ++i)
                result[i] = actions[i];

            return result;
        }

        public static Pattern FromReactions(Pattern original, IList<Result> reactions)
        {
            if (reactions.Count != COUNT)
                throw new ArgumentException();

            var result = new Pattern();
            for (int i = 0; i < COUNT; ++i)
                result[i] = original[i].GetRelated(reactions[i].Reverse());

            return result;
        }

        public List<Action> ToList()
        {
            var result = new List<Action>();
            for (int i = 0; i < COUNT; ++i)
                result.Add((Action)actions[i]);

            return result;
        }

        public Action this[int i]
        {
            get
            {
                if (i < 0 || i >= COUNT)
                    throw new ArgumentOutOfRangeException();

                fixed (byte* a = actions)
                    return (Action)a[i];
            }
            set
            {
                if (i < 0 || i >= COUNT)
                    throw new ArgumentOutOfRangeException();

                fixed (byte* a = actions)
                    a[i] = (byte)value;
            }
        }

        public bool IsWellDefined()
        {
            for (int i = 0; i < COUNT; ++i)
                if (!((Action)actions[i]).IsWellDefined())
                    return false;

            return true;
        }

        public int ToIndex()
        {
            fixed (byte* actions = this.actions)
                return ActionUtil.ToIndex(actions, COUNT);
        }

        public int ToIndexOffset()
        {
            fixed (byte* actions = this.actions)
                return ActionUtil.ToIndex(actions, COUNT) - INDEX_OFFSET;
        }

        public Pattern GetRelated(Result result)
        {
            switch (result)
            {
                case Result.Even: return this;
                case Result.Win:
                case Result.Lose:
                    var ptn = new Pattern();
                    for (int i = 0; i < COUNT; ++i)
                        ptn[i] = ptn[i].GetRelated(result);
                    return ptn;
                default: throw new ArgumentException();
            }
        }

        public int RealizationCount
        {
            get
            {
                int result = 1;
                for (int i = 0; i < COUNT; ++i)
                    if (this[i].IsUnknown())
                        result *= 3;

                return result;
            }
        }

        public IEnumerable<Pattern> AllRealizations()
        {
            for (int index = 0; index < COMBINATIONS; ++index)
            {
                var pattern = FromIndex(index);
                if (IsRealization(pattern))
                    yield return pattern;
            }
        }

        public bool IsRealization(Pattern pattern)
        {
            if (!pattern.IsWellDefined())
                throw new ArgumentException();

            for (int i = 0; i < COUNT; ++i)
                if (this[i] != Action.Unknown && this[i] != pattern[i])
                    return false;

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < COUNT; ++i)
                switch (this[i])
                {
                    case Action.Attack: _ = sb.Append('A'); break;
                    case Action.Break: _ = sb.Append('B'); break;
                    case Action.Guard: _ = sb.Append('G'); break;
                    default: _ = sb.Append('?'); break;
                }

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Pattern ptn)
            {
                for (int i = 0; i < COUNT; ++i)
                    if (actions[i] != ptn.actions[i])
                        return false;
            }
            else
                return false;

            return true;
        }

        public override int GetHashCode() => ToString().GetHashCode();
    }
}
