using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.Distributions;

namespace Extra
{
    public static class UI
    {
        public static ConsoleColor MeanColor { get; set; } = ConsoleColor.Red;

        public class CancelMenu : Exception { }

        public static void PlotPDF(this IContinuousDistribution self, int x, int y, int width, int height, CornerType corners, string label)
        {
            self.PlotPDF(x, y, width, height, corners, label, TextAlign.Left);
        }

        public static void PlotPDF(this IContinuousDistribution self, int x, int y, int width, int height, CornerType corners, string label, TextAlign align)
        {
            DrawWindow(x, y, width, height, corners, label, align);
            self.PlotPDF(x + 1, y + 1, width - 2, height - 2);
        }

        public static void PlotPDF(this IContinuousDistribution self, int x, int y, int width, int height, CornerType corners)
        {
            DrawWindow(x, y, width, height, corners);
            self.PlotPDF(x + 1, y + 1, width - 2, height - 2);
        }

        public static void PlotPDF(this IContinuousDistribution self, int x, int y, int width, int height)
        {
            if (x < 0 || y < 0 || x + width > Console.BufferWidth || y + height > Console.BufferHeight)
                throw new ArgumentOutOfRangeException();

            var dx = 1d / width;
            var xs = new double[width];
            for (int i = 0; i < width; ++i)
                xs[i] = dx * (i + .5d);

            var ys = new double[width];
            int xmean = -1;
            double ymax = -1;
            for (int i = 0; i < width; ++i)
            {
                ys[i] = self.Density(xs[i]);
                if (ys[i] > ymax)
                {
                    xmean = i;
                    ymax = ys[i];
                }
            }
            var dys = new int[width];
            for (int i = 0; i < width; ++i)
                dys[i] = (int)(height * ys[i] / ymax);

            for (int i = 0; i < width; ++i)
            {
                if (dys[i] > 0)
                {
                    Console.SetCursorPosition(x + i, y + height - dys[i]);
                    if (i < xmean)
                    {
                        if (dys[i + 1] > dys[i])
                            Console.Write('/');
                        else
                            Console.Write('-');
                    }
                    else if (i > xmean)
                    {
                        if (dys[i - 1] > dys[i])
                            Console.Write('\\');
                        else
                            Console.Write('-');
                    }
                    else
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = MeanColor;
                        Console.Write("*");
                        Console.ForegroundColor = color;
                    }
                }
            }
        }

        public static void FillRect(int x, int y, int width, int height, char chr)
        {
            if (x < 0 || y < 0 || x + width > Console.BufferWidth || y + height > Console.BufferHeight)
                throw new ArgumentOutOfRangeException();

            var line = new string(chr, width);
            for (int i = 0; i < height; ++i)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(line);
            }
        }

        public enum CornerType
        {
            Normal,
            Rounded
        }

        public enum TextAlign
        {
            Left,
            Center,
            Right
        }

        public static void DrawWindow(int x, int y, int width, int height, CornerType corners, string title)
        {
            DrawWindow(x, y, width, height, corners, title, TextAlign.Center);
        }

        public static void DrawWindow(int x, int y, int width, int height, CornerType corners, string title, TextAlign align)
        {
            if (title.Length > width - 4)
                title = title.Substring(0, width - 4);
            DrawWindow(x, y, width, height, corners);
            switch (align)
            {
                case TextAlign.Left: Console.SetCursorPosition(x + 2, y); break;
                case TextAlign.Center: Console.SetCursorPosition(x + (width - title.Length) / 2, y); break;
                case TextAlign.Right: Console.SetCursorPosition(x + width - 2 - title.Length, y); break;
            }
            Console.Write(title);
        }

        public static void DrawWindow(int x, int y, int width, int height, CornerType corners)
        {
            if (x < 0 || y < 0 || x + width > Console.BufferWidth || y + height > Console.BufferHeight)
                throw new ArgumentOutOfRangeException();

            if (width < 2 || height < 2)
                throw new ArgumentOutOfRangeException();

            var sb = new StringBuilder();
            switch (corners)
            {
                case CornerType.Normal: _ = sb.Append('+'); break;
                case CornerType.Rounded: _ = sb.Append('/'); break;
            }
            _ = sb.Append('-', width - 2);
            switch (corners)
            {
                case CornerType.Normal: _ = sb.Append('+'); break;
                case CornerType.Rounded: _ = sb.Append('\\'); break;
            }
            Console.SetCursorPosition(x, y);
            Console.Write(sb);

            _ = sb.Clear();
            _ = sb.Append('|').Append(' ', width - 2).Append('|');
            for (int i = 1; i < height - 1; ++i)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(sb);
            }

            _ = sb.Clear();
            switch (corners)
            {
                case CornerType.Normal: _ = sb.Append('+'); break;
                case CornerType.Rounded: _ = sb.Append('\\'); break;
            }
            _ = sb.Append('-', width - 2);
            switch (corners)
            {
                case CornerType.Normal: _ = sb.Append('+'); break;
                case CornerType.Rounded: _ = sb.Append('/'); break;
            }
            Console.SetCursorPosition(x, y + height - 1);
            Console.Write(sb);
        }

        public static int SelectItemCursor(int x, int y, int width, IList<string> labels, CornerType corners, string title)
        {
            return SelectItemCursor(x, y, width, labels, corners, title, TextAlign.Center);
        }

        public static int SelectItemCursor(int x, int y, int width, IList<string> labels, CornerType corners, string title, TextAlign align)
        {
            DrawWindow(x, y, width, labels.Count + 2, corners, title, align);
            return SelectItemCursor(x + 1, y + 1, width - 2, labels);
        }

        public static int SelectItemCursor(int x, int y, int width, IList<string> labels, CornerType corners)
        {
            DrawWindow(x, y, width, labels.Count + 2, corners);
            return SelectItemCursor(x + 1, y + 1, width - 2, labels);
        }

        public static int SelectItemCursor(int x, int y, int width, IList<string> labels)
        {
            int selected = 0;

            for (int i = 0; i < labels.Count; ++i)
                DrawMenuItem(x, y + i, width, labels[i], i == selected);

            bool end = false;
            while (!end)
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.Enter: end = true; break;
                    case ConsoleKey.DownArrow: UpdateSelection(selected + 1); break;
                    case ConsoleKey.UpArrow: UpdateSelection(selected - 1); break;
                    case ConsoleKey.Escape: throw new CancelMenu();
                }

            return selected;

            void UpdateSelection(int select)
            {
                while (select < 0)
                    select += labels.Count;
                select %= labels.Count;

                if (selected == select)
                    return;

                DrawMenuItem(x, y + selected, width, labels[selected], false);
                DrawMenuItem(x, y + select, width, labels[select], true);

                selected = select;
            }
        }

        public static void DisplayNotification(int y, string message, CornerType corners)
        {
            DrawWindow((Console.BufferWidth - message.Length) / 2 - 1, y, message.Length + 2, 3, corners);
            DisplayNotification(y, message);
        }

        public static void DisplayNotification(int y, string message)
        {
            DisplayNotification((Console.BufferWidth - message.Length) / 2, y, message.Length, message);
        }

        public static void DisplayNotification(int x, int y, int width, string message, CornerType corners)
        {
            DrawWindow(x, y, width, 3, corners);
            DisplayNotification(x + 1, y + 1, width - 2, message);
        }

        public static void DisplayNotification(int x, int y, int width, string message)
        {
            Console.SetCursorPosition(x, y);
            if (message.Length > width)
                message = message.Substring(0, width);
            Console.Write(message);
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        }

        public static string TextInput(int x, int y, int width, CornerType corners, string title, Func<string, bool> validator, Action<string> errorResponse)
        {
            return TextInput(x, y, width, corners, title, TextAlign.Center, validator, errorResponse);
        }

        public static string TextInput(int x, int y, int width, CornerType corners, string title, TextAlign align, Func<string, bool> validator, Action<string> errorResponse)
        {
            var value = TextInput(x, y, width, corners, title, align);
            while (!((value is null) || validator(value)))
            {
                errorResponse(value);
                value = TextInput(x, y, width, corners, title, align);
            }

            return value;
        }

        public static string TextInput(int x, int y, int width, CornerType corners, string title)
        {
            return TextInput(x, y, width, corners, title, TextAlign.Center);
        }

        public static string TextInput(int x, int y, int width, CornerType corners, string title, TextAlign align)
        {
            DrawWindow(x, y, width, 3, corners, title, align);
            return TextInput(x + 1, y + 1, width - 2);
        }

        public static string TextInput(int x, int y, int width, CornerType corners)
        {
            DrawWindow(x, y, width, 3, corners);
            return TextInput(x + 1, y + 1, width - 2);
        }

        public static string TextInput(int x, int y, int width)
        {
            var sb = new StringBuilder();
            int position = 0;
            Console.SetCursorPosition(x, y);
            InvertColors();
            Console.Write(' ');
            InvertColors();

            bool end = false;
            try
            {
                while (!end)
                {
                    var e = Console.ReadKey(true);
                    switch (e.Key)
                    {
                        case ConsoleKey.Enter: end = true; break;
                        case ConsoleKey.Backspace: DeleteCharacter(); break;
                        case ConsoleKey.Escape: throw new CancelMenu();
                        default: AddCharacter(e.KeyChar); break;
                    }
                }

                return sb.ToString();
            }
            catch (CancelMenu)
            {
                return null;
            }
            void AddCharacter(char chr)
            {
                if (position >= width)
                    return;

                _ = sb.Append(chr);
                Console.SetCursorPosition(x + position, y);
                Console.Write(chr);
                ++position;
                
                if (position >= width)
                    return;

                InvertColors();
                Console.Write(' ');
                InvertColors();
            }
            void DeleteCharacter()
            {
                if (position == 0)
                    return;

                _ = sb.Remove(position - 1, 1);
                --position;
                Console.SetCursorPosition(x + position, y);
                InvertColors();
                Console.Write(' ');
                InvertColors();

                if (position >= width - 1)
                    return;

                Console.Write(' ');
            }
        }

        public static void DrawMenuItem(int x, int y, int width, string label, bool selected = false)
        {
            Console.SetCursorPosition(x, y);
            if (selected)
                InvertColors();

            if (label.Length > width)
                label = label.Take(width).ToString();
            else
                label += new string(' ', width - label.Length);

            Console.Write(label);

            if (selected)
                InvertColors();
        }

        public static void DrawResponse(int x, int y, int width, int height, int columns, int rows, KnowledgeBase data, Pattern pattern)
        {
            var query = data.OrderedQuery(pattern, columns * rows);
            int cellWidth = width / columns, cellHeight = height / rows;

            int k = 0;
            foreach (var q in query)
            {
                int dx = cellWidth * (k % columns), dy = cellHeight * (k / columns);
                q.Value.PlotPDF(x + dx, y + dy, cellWidth, cellHeight, CornerType.Normal, q.Key.ToString());
                ++k;
            }
        }

        public static void InvertColors()
        {
            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = tmp;
        }
    }
}
