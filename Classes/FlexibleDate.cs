/*
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace FoxHollow.FHM.Shared.Classes;

/// <summary>
///     FlexibleDate provies a mechanism to specify a date, or a range of dates,
///     in a format much more flexible than typically allowed by computer programs.
///     
///     Here are a number of examples that can be parsed by this class:
///        - about 1983
///        - abt  1967
///        - abt. 1922
///        - abt. 1922-1924
///        - 1941
///        - 1935-1938
///        - spring 1945
///        - 2022-02-21
///        - spring 1996
///        - ~2002
///        - about 1985
///        - abt. 1960
///        - 1930s
///        - 3 Dec 1965
///        - 22 Nov 1976
///        - Nov. 1913
///        - 1989-04
///        - 1984-07-21
///        - between 1974-1977
/// 
///     All of the above dates can be parsed and converted into an approximate
///     representation. To accomodate the flexible, range-based nature of this sort
///     of date, all parsed dates are broken down into a range of "begin" and "end"
///     dates. If the provided date is an exact date (including year, month, and day)
///     then the begin and end dates will match, otherwise the range will be the best
///     approximation of begin and end date range, based on the provided input string.
/// </summary>
public class FlexibleDate
{
    /// <summary>
    ///     Begin date of the range represented by this FlexibleDate
    /// </summary>
    public DateOnly DateRangeBegin { get; private set; }

    /// <summary>
    ///     End date of the range represented by this FlexibleDate
    /// </summary>
    public DateOnly DateRangeEnd { get; private set; }

    /// <summary>
    ///     The day that falls exactly half-way between the start and end dates
    /// </summary>
    public DateOnly TimelineDate { get; private set; }

    /// <summary>
    ///     The season specified by this flexible date, null if no season specified
    /// </summary>
    public FlexibleDateSeason? Season { get; private set; }

    /// <summary>
    ///     If true, the specified date is to be considered an estimate. 
    ///     Indicated by prefix "Abt."
    /// </summary>
    public bool IsEstimate { get; private set; } = false;

    /// <summary>
    ///     If true, this means an exact date with year, month and day was provided, 
    ///     without an end date.
    /// </summary>
    public bool IsExact { get; private set; } = false;

    public string InputDateString { get; private set; }

    /// <summary>
    ///     Creater a new FlexibleDate by providing a date string
    /// </summary>
    /// <param name="fDate">String to initialize the date from</param>
    public FlexibleDate(string fDate)
    {
        this.ParseDate(fDate);
    }

    /// <summary>
    ///     Convert the FlexibleDate back into a string representation
    /// </summary>
    /// <returns>FlexibleDate as a string</returns>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        if (this.IsExact)
            sb.Append(this.DateRangeBegin.ToString("d MMM yyyy"));
        else
        {
            if (this.IsEstimate)
                sb.Append("Abt. ");

            if (this.Season != null)
                sb.Append($"{this.Season.ToString()} {this.DateRangeBegin.Year}");
            else
            {
                if (this.DateRangeBegin.Year == this.DateRangeEnd.Year)
                {
                    // this is to handle year and month output, without day
                    if (this.DateRangeBegin.Month == this.DateRangeEnd.Month)
                        sb.Append(this.DateRangeBegin.ToString("MMM yyyy"));

                    // this is to handle an entire year
                    else if (this.DateRangeBegin.Month == 1 && this.DateRangeBegin.Day == 1 && this.DateRangeEnd.Month == 12 && this.DateRangeEnd.Day == 31)
                        sb.Append(this.DateRangeBegin.ToString("yyyy"));
                }
                else
                {
                    if (this.DateRangeBegin.Year % 10 == 0 && this.DateRangeEnd.Year - this.DateRangeBegin.Year == 9)
                        sb.Append(this.DateRangeBegin.ToString("yyyy") + "s");
                    else if (this.DateRangeBegin.Month == 1 && this.DateRangeBegin.Day == 1 && this.DateRangeEnd.Month == 12 && this.DateRangeEnd.Day == 31)
                        sb.Append(this.DateRangeBegin.ToString("yyyy") + "-" + this.DateRangeEnd.ToString("yyyy"));
                }
            }
        }

        return sb.ToString();
    }

    private void ParseDate(string fDate)
    {
        fDate = fDate.Trim();

        // Future: Add handling for other languages

        var regex = new Regex(
            @"^" + 
            @"(?<isEstimate>about|abt\.?|~)?\s*" +
            @"(?<isBetween>between|betw?\.?)?\s*" +
            @"(?<season>spring|summer|fall|autumn|winter)?\s*" + 
            @"(?<dayNum1>\d{1,2})?\s*" +
            @"(?<monthName1>" +
                @"(?<monthNameFull>January|February|March|April|May|June|July|August|September|October|November|December)" +
                @"|" +
                @"(?<monthNameAbbrev>Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\.?" +
            @")?\s*" +
            @"((?<year>\d{4})(?<decadeRange>s)?)\s*" + 
            @"([\-|and|\&]\s*(?<endYear>\d{4}))?" + 
            @"(\-" + 
                @"(?<monthNum>\d{2})?" +
                @"(\-(?<dayNum2>\d{2}))?" +
            @")?" +
            @"$");

        var match = regex.Match(fDate);

        if (!match.Success)
            throw new FlexibleDateParseException($"Failed to parse FlexibleDate: {fDate}");


        var estimateMatch = match.Groups["isEstimate"];
        var seasonMatch = match.Groups["season"];
        var dayNum1Match = match.Groups["dayNum1"];
        var yearMatch = match.Groups["year"];
        var decadeRangeMatch = match.Groups["decadeRange"];
        var monthName1Match = match.Groups["monthName1"];
        var monthNumMatch = match.Groups["monthNum"];
        var endYearMatch = match.Groups["endYear"];
        var dayNum2Match = match.Groups["dayNum2"];

        if (dayNum1Match.Success && dayNum2Match.Success)
            throw new FlexibleDateParseException("Failed to parse date. Day value provided twice!");

        if (monthName1Match.Success && monthNumMatch.Success)
            throw new FlexibleDateParseException("Failed to parse date. Month value provided twice!");

        this.IsEstimate = estimateMatch.Success;

        if (seasonMatch.Success)
        {
            if (String.Equals(seasonMatch.Value, "winter", StringComparison.InvariantCultureIgnoreCase))
                this.Season = FlexibleDateSeason.Winter;
            else if (String.Equals(seasonMatch.Value, "spring", StringComparison.InvariantCultureIgnoreCase))
                this.Season = FlexibleDateSeason.Spring;
            else if (String.Equals(seasonMatch.Value, "summer", StringComparison.InvariantCultureIgnoreCase))
                this.Season = FlexibleDateSeason.Summer;
            else if (String.Equals(seasonMatch.Value, "autumn", StringComparison.InvariantCultureIgnoreCase))
                this.Season = FlexibleDateSeason.Autumn;
            else if (String.Equals(seasonMatch.Value, "fall", StringComparison.InvariantCultureIgnoreCase))
                this.Season = FlexibleDateSeason.Autumn;
        }

        int year = 0;
        int month = 0;
        int day = 0;

        int endYear = 0;
        int endMonth = 0;
        int endDay = 0;

        if (monthName1Match.Success)
            month = this.ParseMonthName(monthName1Match.Value);
        else if (monthNumMatch.Success)
            month = this.ParseMonthNum(monthNumMatch.Value);

        if (dayNum1Match.Success)
            day = this.ParseDay(dayNum1Match.Value);
        else if (dayNum2Match.Success)
            day = this.ParseDay(dayNum2Match.Value);

        if (yearMatch.Success)
            year = this.ParseYear(yearMatch.Value);

        if (endYearMatch.Success)
            endYear = this.ParseYear(endYearMatch.Value);
        else if (decadeRangeMatch.Success && String.Equals(decadeRangeMatch.Value, "s", StringComparison.InvariantCultureIgnoreCase))
        {
            if (year % 10 != 0)
                throw new FlexibleDateParseException($"Decade range provided with 's' suffix, but provided year is not a multiple of 0: {year}");

            endYear = year + 9;
            endMonth = 12;
            endDay = 31;
        }

        this.IsExact = year != 0 && month != 0 && day != 0 && endYear == 0 && endMonth == 0 && endDay == 0;

        if (seasonMatch.Success)
        {
            if (month != 0 || day != 0)
                throw new FlexibleDateParseException($"Date provided includes both a season and a specific month/day: {fDate}");
            else
                (this.DateRangeBegin, this.DateRangeEnd) = this.GetSeasonDateRange(this.Season.Value, year);
        }
        else
        {   
            if (endYear == 0)
                endYear = year;
            
            if (endMonth == 0)
            {
                if (monthName1Match.Success || monthNumMatch.Success)
                    endMonth = month;
                else
                    endMonth = 12;
            }
            
            if (endDay == 0)
            {
                if (dayNum1Match.Success || dayNum2Match.Success)
                    endDay = day;
                else
                    endDay = this.GetLastDayOfMonth(endMonth);
            }

            // default month if no explicit month was specified
            if (month == 0)
                month = 1;
            
            // default day if no explicit day was specified
            if (day == 0)
                day = 1;


            this.DateRangeBegin = new DateOnly(year, month, day);
            this.DateRangeEnd = new DateOnly(endYear, endMonth, endDay);
        }

        this.TimelineDate = this.CalculateMidpoint(this.DateRangeBegin, this.DateRangeEnd);
        this.InputDateString = fDate;
    }

    private int GetRangeInDays(DateOnly begin, DateOnly end)
    {
        return end.DayNumber - begin.DayNumber;
    }

    private DateOnly CalculateMidpoint(DateOnly begin, DateOnly end)
    {
        if (begin == end)
            return begin;

        int rangeHalf = this.GetRangeInDays(begin, end) / 2;

        int midPointDayNumber = begin.DayNumber + rangeHalf;

        return DateOnly.FromDayNumber(midPointDayNumber);
    }

    private int GetLastDayOfMonth(int month)
    {
        switch (month)
        {
            case 1: // jan
            case 3: // march
            case 5: // may
            case 7: // july
            case 8: // august
            case 10: // october
            case 12: // december
                return 31;

            case 4:
            case 6:
            case 9:
            case 11:
                return 30;

            case 2:
                return 28;
        }

        throw new FlexibleDateParseException($"Invalid month provided: {month}");
    }

    private int ParseYear(string yearStr)
    {
        int year;

        var result = Int32.TryParse(yearStr, out year);

        if (result == false || year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            throw new FlexibleDateParseException($"Failed to parse year: {yearStr}");
        
        return year;
    }

    private int ParseMonthNum(string monthStr)
    {
        int month;

        var result = Int32.TryParse(monthStr, out month);

        if (result == false || month <= 0 || month > 12)
            throw new FlexibleDateParseException($"Failed to parse month: {monthStr}");
        
        return month;
    }

    private int ParseDay(string dayStr)
    {
        int day;

        var result = Int32.TryParse(dayStr, out day);

        if (result == false || day <= 0 || day > 31)
            throw new FlexibleDateParseException($"Failed to parse day: {dayStr}");
        
        return day;
    }

    private int ParseMonthName(string monthStr)
    {
        int month;

        DateOnly output;

        monthStr = monthStr.Trim().TrimEnd('.');

        if (DateOnly.TryParseExact(monthStr, "MMM", out output))
            month = output.Month;
        else if (DateOnly.TryParseExact(monthStr, "MMMM", out output))
            month = output.Month;
        else
            throw new FlexibleDateParseException($"Failed to parse month by name: {monthStr}");

        return month;
    }

    private (DateOnly, DateOnly) GetSeasonDateRange(FlexibleDateSeason season, int startYear)
    {
        if (startYear < DateOnly.MinValue.Year || startYear > DateOnly.MaxValue.Year)
            throw new FlexibleDateParseException($"Provided year is out of range: {startYear}");

        // Future: Add handling for southern hemisphere

        int endYear = startYear;

        int startMonth = 0;
        int endMonth = 0;

        int startDay = 0;
        int endDay = 0;

        switch (season)
        {
            case FlexibleDateSeason.Winter:
                startMonth = 12;
                startDay = 22;

                endMonth = 3;
                endDay = 20;

                endYear += 1;
                break;

            case FlexibleDateSeason.Spring:
                startMonth = 3;
                startDay = 21;

                endMonth = 6;
                endDay = 20;
                break;

            case FlexibleDateSeason.Summer:
                startMonth = 6;
                startDay = 21;

                endMonth = 9;
                endDay = 22;
                break;

            case FlexibleDateSeason.Autumn:
                startMonth = 9;
                startDay = 23;

                endMonth = 12;
                endDay = 21;
                break;
        }

        return (new DateOnly(startYear, startMonth, startDay), new DateOnly(endYear, endMonth, endDay));
    }
}

/// <summary>
///     Exception that is thrown any time an error occurs while parsing a flexible date
/// </summary>
public class FlexibleDateParseException : Exception
{
    /// <summary>
    ///     Throw a new exeption with the specified message
    /// </summary>
    /// <param name="message">Message to throw</param>
    public FlexibleDateParseException(string message) : base(message)
    {

    }
}

/// <summary>
///     Enum that lists the possible seasons that can be specified for a FlexibleDate
/// </summary>
public enum FlexibleDateSeason
{
    Winter = 0,
    Spring = 1,
    Summer = 2,
    Autumn = 3
}