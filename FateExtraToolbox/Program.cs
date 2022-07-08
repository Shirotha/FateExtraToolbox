using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extra
{
    class Program
    {
        static string DataLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data.dat");

        const int WINDOW_WIDTH = 163;
        const int WINDOW_HEIGHT = 50;

        const int LIST_X = 4;
        const int LIST_Y = 1;
        const int NAME_Y = 10;
        const int NAME_WIDTH = 100;

        const int PATTERN_ROWS = 4;
        const int PATTERN_COLUMNS = 2;
        const int PATTERN_HEIGHT = 11;
        const int PATTERN_COUNT = PATTERN_COLUMNS * PATTERN_ROWS;
        const int PATTERN_X = 1;
        const int PATTERN_Y = 1;

        static void Main(string[] args)
        {
            if (args.Length > 0)
                DataLocation = args[0];
            if (File.Exists(DataLocation))
                DataManager.Load(DataLocation);

            AppDomain.CurrentDomain.ProcessExit += delegate (object sender, EventArgs e)
            {
                DataManager.Save(DataLocation);
            };

            Console.SetWindowSize(WINDOW_WIDTH, WINDOW_HEIGHT);
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            Console.CursorVisible = false;
            try
            {
                while (true)
                {
                    Console.Clear();
                    var collection = SelectCollection();
                    if (collection is null)
                        continue;
                    try
                    {
                        while (true)
                        {
                            Console.Clear();
                            var entry = SelectEntry(collection);
                            if (entry is null)
                                continue;
                            try
                            {
                                while (true)
                                {
                                    Console.Clear();
                                    var w = Math.Max(Pattern.COUNT, Lang.KnowledgeInputTitle.Length) + 2;
                                    var pattern = UI.TextInput((Console.BufferWidth - w) / 2, 1, w, UI.CornerType.Rounded,
                                        Lang.KnowledgeInputTitle, s => s.Length == Pattern.COUNT,
                                        s => UI.DisplayNotification(1, Lang.InvalidPatternError));

                                    if (pattern is null)
                                        throw new UI.CancelMenu();

                                    Console.SetCursorPosition((Console.BufferWidth - Pattern.COUNT) / 2, 0);
                                    Console.Write(pattern);

                                    var query = entry.OrderedQuery(Pattern.FromString(pattern), PATTERN_COUNT);
                                    w = (Console.BufferWidth - 2 * PATTERN_X + 1) / PATTERN_COLUMNS;
                                    int i = 0;
                                    foreach (var q in query)
                                    {
                                        q.Value.PlotPDF(PATTERN_X + w * (i % PATTERN_COLUMNS), PATTERN_Y + PATTERN_HEIGHT * (i / PATTERN_COLUMNS), w, PATTERN_HEIGHT,
                                            UI.CornerType.Normal, q.Key.ToString());
                                        ++i;
                                    }

                                    var y = PATTERN_Y + PATTERN_HEIGHT * PATTERN_ROWS + 1;
                                    w = Math.Max(Pattern.COUNT, Lang.InputPlayerActionsTitle.Length) + 4;
                                    pattern = UI.TextInput((Console.BufferWidth - w) / 2, y, w, UI.CornerType.Rounded,
                                        Lang.InputPlayerActionsTitle, s => s.Length == Pattern.COUNT,
                                        s => UI.DisplayNotification(y, Lang.InvalidPatternError));

                                    if (pattern is null)
                                        throw new UI.CancelMenu();

                                    var reactionsString = UI.TextInput((Console.BufferWidth - w) / 2, y, w, UI.CornerType.Rounded,
                                        Lang.RecactinsInputTitle, s => s.Length == Pattern.COUNT,
                                        s => UI.DisplayNotification(y, Lang.InvalidPatternError));

                                    if (reactionsString is null)
                                        throw new UI.CancelMenu();

                                    var reactions = reactionsString.Select(delegate (char c)
                                    {
                                        switch (c)
                                        {
                                            case '+': return Result.Win;
                                            case '-': return Result.Lose;
                                            case '#': return Result.Even;
                                            default: return Result.Unknown;
                                        }
                                    }).ToList();

                                    entry.Add(Pattern.FromReactions(Pattern.FromString(pattern), reactions));
                                }
                            }
                            catch (UI.CancelMenu) { }
                        }
                    }
                    catch (UI.CancelMenu) { }
                }
            }
            catch (UI.CancelMenu) { }
        }

        static List<KnowledgeBase> SelectCollection()
        {
            var name = SelectOrNew(DataManager.Data.Keys, delegate
            {
                var n = UI.TextInput((Console.BufferWidth - NAME_WIDTH) / 2, NAME_Y, NAME_WIDTH, UI.CornerType.Rounded, Lang.CollectionNamingTitle,
                    s => !DataManager.Data.ContainsKey(s), 
                    s => UI.DisplayNotification(NAME_Y, Lang.NamingError));

                if (n is null)
                    return null;

                DataManager.Data.Add(n, new List<KnowledgeBase>());
                DataManager.Sort();

                return n;
            }, Lang.CollectionsTitle);

            if (name is null)
                return null;

            return DataManager.Data[name];
        }

        static KnowledgeBase SelectEntry(List<KnowledgeBase> collection)
        {
            var name = SelectOrNew(collection.Select(k => k.Name), delegate
            {
                var n = UI.TextInput((Console.BufferWidth - NAME_WIDTH) / 2, NAME_Y, NAME_WIDTH, UI.CornerType.Rounded, Lang.EntryNamingTitle,
                    s => collection.Find(k => k.Name == s) is null,
                    s => UI.DisplayNotification(NAME_Y, Lang.NamingError));

                if (n is null)
                    return null;

                collection.Add(new KnowledgeBase(n));
                DataManager.Sort();

                return n;
            }, Lang.EntriesTitle);

            if (name is null)
                return null;

            return collection.Find(c => c.Name == name);
        }

        static string SelectOrNew(IEnumerable<string> list, Func<string> creator, string title)
        {
            var entries = list.Append(Lang.AddNewCollection).ToList();
            var selected = UI.SelectItemCursor(LIST_X, LIST_Y, Console.BufferWidth - 2 * LIST_X + 1, entries, UI.CornerType.Rounded, title);
            if (selected == entries.Count - 1)
                return creator();
            else
                return entries[selected];
        }

    }
}
