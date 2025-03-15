using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Billy.CodeReadability;
using Index = Billy.CodeReadability.Index;

namespace Billy.PolishReceiptRecognitionAlgorithm.Grammar
{
    //todo: maybe extracting hours should be done separately - not to duplicate it in each date regex
    public static class DateRegex
    {
        private static readonly Regex YyyyMmDdRegex = //eg. 2019-11-24
            new Regex(@"(\d{4})[ ]*[-.][ ]*(\d{2})[ ]*[-.][ ]*(\d{2})([ ]*(\d{1,2})[ ]*[:][ ]*(\d{2}))?");

        private static readonly Regex DdMmYyyyRegex = //eg. 24-11-2019
            new Regex(@"(\d{2})[ ]*[-.][ ]*(\d{2})[ ]*[-.][ ]*(\d{4})([ ]*(\d{1,2})[ ]*[:][ ]*(\d{2}))?");

        private static readonly Regex YyrMmDdRegex = //eg. 19r11.24
            new Regex(@"(\d{2})[ ]*[r][ ]*(\d{2})[ ]*[.][ ]*(\d{2})([ ]*(\d{1,2})[ ]*[:][ ]*(\d{2}))?");

        public static Option<FindResult<DateTime>> TryFindLine(IReadOnlyList<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var date = TryMatch(line);

                if (date.TryGet(out var receiptDate))
                {
                    return new FindResult<DateTime>(
                        new Index(i),
                        receiptDate);
                }
            }

            return Option<FindResult<DateTime>>.None;
        }

        public static Option<DateTime> TryMatch(string text)
        {
            try
            {
                if (TryMatchYyyyMmDd(text).TryGet(out var fromYyyyMmDd))
                    return fromYyyyMmDd;

                if (TryMatchDdMmYyyy(text).TryGet(out var fromDdMmYyyy))
                    return fromDdMmYyyy;

                if (TryMatchYyrMmDd(text).TryGet(out var fromYyrMmDd))
                    return fromYyrMmDd;
            }
            catch
            {
                // explanation: 
                // if there is anything wrong with the date inside given line we simply assume
                // we cannot do anything about it and return 'no date' result
                // the example situation when exception can be thrown
                // is invalid month number, invalid day number etc.

                return Option<DateTime>.None;
            }

            return Option<DateTime>.None;
        }

        private static Option<DateTime> TryMatchYyyyMmDd(string text)
        {
            var match = YyyyMmDdRegex.Match(text);

            if (match.Success)
            {
                var year = match.Groups[1].Value;
                var month = match.Groups[2].Value;
                var day = match.Groups[3].Value;
                var hour = match.Groups[5].Value;
                var minutes = match.Groups[6].Value;

                {
                    return new DateTime(
                        year: int.Parse(year),
                        month: int.Parse(month),
                        day: int.Parse(day),
                        hour: string.IsNullOrEmpty(hour) ? 0 : int.Parse(hour),
                        minute: string.IsNullOrEmpty(minutes) ? 0 : int.Parse(minutes),
                        second: 0);
                }
            }

            return Option<DateTime>.None;
        }

        private static Option<DateTime> TryMatchDdMmYyyy(string text)
        {
            var match = DdMmYyyyRegex.Match(text);

            if (match.Success)
            {
                var year = match.Groups[3].Value;
                var month = match.Groups[2].Value;
                var day = match.Groups[1].Value;
                var hour = match.Groups[5].Value;
                var minutes = match.Groups[6].Value;

                {
                    return new DateTime(
                        year: int.Parse(year),
                        month: int.Parse(month),
                        day: int.Parse(day),
                        hour: string.IsNullOrEmpty(hour) ? 0 : int.Parse(hour),
                        minute: string.IsNullOrEmpty(minutes) ? 0 : int.Parse(minutes),
                        second: 0);
                }
            }

            return Option<DateTime>.None;
        }

        private static Option<DateTime> TryMatchYyrMmDd(string text)
        {
            var match = YyrMmDdRegex.Match(text);

            if (match.Success)
            {
                var year = match.Groups[1].Value;
                var month = match.Groups[2].Value;
                var day = match.Groups[3].Value;
                var hour = match.Groups[5].Value;
                var minutes = match.Groups[6].Value;

                {
                    return new DateTime(
                        year: int.Parse(year) + 2000, //explanation 19r => 19 + 2000 => 2019
                        month: int.Parse(month),
                        day: int.Parse(day),
                        hour: string.IsNullOrEmpty(hour) ? 0 : int.Parse(hour),
                        minute: string.IsNullOrEmpty(minutes) ? 0 : int.Parse(minutes),
                        second: 0);
                }
            }

            return Option<DateTime>.None;
        }
    }
}
