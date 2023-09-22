namespace CodedThought.Core.Business {

	/// <summary>Summary description for HolidayCalculator. Created by: bartlek Created on: 05/21/2007 13:07:09 Modified: 10/15/2007 - Added the daylight savings dates.</summary>
	public class HolidayCalculator {

		#region Declarations

		private List<HolidayItem> m_Holidays;
		private long m_lngCurrentYear;

		/// <summary>Holiday Enumerator</summary>
		public enum Holidays {

			/// <summary>A Non-Holiday</summary>
			NonHoliday = 0,

			/// <summary>New Years Day</summary>
			NewYearsDay,

			/// <summary>Martin Luther King Day</summary>
			MartinLutherKingDay,

			/// <summary>Washingtons Birthday</summary>
			WashingtonBirthday,

			/// <summary>Armed Forces Day</summary>
			ArmedForcesDay,

			/// <summary>Memorial Day</summary>
			MemorialDay,

			/// <summary>Flag Day</summary>
			FlagDay,

			/// <summary>Independence Day</summary>
			IndependenceDay,

			/// <summary>Labor Day</summary>
			LaborDay,

			/// <summary>Columbus Day</summary>
			ColumbusDay,

			/// <summary>Election Day</summary>
			ElectionDay,

			/// <summary>Veterans Day</summary>
			VeteransDay,

			/// <summary>Thangsgiving Day</summary>
			ThanksgivingDay,

			/// <summary>Christmas Day</summary>
			ChristmasDay,

			/// <summary>Custom Holiday</summary>
			CustomHoliday,

			/// <summary>Daylight Savings Begins</summary>
			DaylightSavingsBegins,

			/// <summary>Daylight Savings Ends</summary>
			DaylighSavingsEnd
		}

		/// <summary>Fiscal Quarters of the year based on November 1st as the beginning of the fiscal year.</summary>
		public enum Quarter {

			/// <summary>First Quarter</summary>
			First = 1,

			/// <summary>Second Quarter</summary>
			Second = 2,

			/// <summary>Third Quarter</summary>
			Third = 3,

			/// <summary>Fourth Quarter</summary>
			Fourth = 4
		}

		/// <summary>Months of the year</summary>
		public enum Month {

			/// <summary>January</summary>
			January = 1,

			/// <summary>February</summary>
			February = 2,

			/// <summary>March</summary>
			March = 3,

			/// <summary>April</summary>
			April = 4,

			/// <summary>May</summary>
			May = 5,

			/// <summary>June</summary>
			June = 6,

			/// <summary>July</summary>
			July = 7,

			/// <summary>August</summary>
			August = 8,

			/// <summary>September</summary>
			September = 9,

			/// <summary>October</summary>
			October = 10,

			/// <summary>November</summary>
			November = 11,

			/// <summary>December</summary>
			December = 12
		}

		/// <summary>Holiday Item internal structure.</summary>
		public struct HolidayItem {

			/// <summary>Holiday Enumerator</summary>
			public Holidays Holiday;

			/// <summary>Date of the holiday</summary>
			public DateTime HolidayDate;

			/// <summary>Year of the holiday</summary>
			public long HolidayYear;
		}

		#endregion Declarations

		#region Properties

		/// <summary>Gets the current holiday list.</summary>
		/// <value>The current holiday list.</value>
		public List<HolidayItem> CurrentHolidayList {
			get { return m_Holidays; }
		}

		#endregion Properties

		#region Methods

		/// <summary>Determines whether the specified dt current date is holiday.</summary>
		/// <param name="dtCurrentDate">   The dt current date.</param>
		/// <param name="strReturnHoliday">The STR return holiday.</param>
		/// <returns></returns>
		public Holidays IsHoliday(DateTime dtCurrentDate, ref string strReturnHoliday) {
			// ------------------------------------- IsHoliday
			// Purpose:   To determine if the date passed in is a holiday, and return which holiday it is.
			// Arguements:  IN: Date to be evaluated
			// OUT: True/False ByRef OUT: Holiday's name as a string -------------------------------------
			Holidays intHoliday = 0;
			HolidayItem thisHoliday = new();
			m_lngCurrentYear = dtCurrentDate.Year;

			if (m_Holidays.Count == 0) {
				LoadHolidayList();
			}
			// --------------------------------------------------- Check the collection of holidays ---------------------------------------------------
			thisHoliday = GetHolidayItemHash(dtCurrentDate);
			if (thisHoliday.Holiday != Holidays.NonHoliday) {
				intHoliday = thisHoliday.Holiday;
			}
			// '--------------------------------------------------- ' There are 13 Federal Holidays '--------------------------------------------------- If dtCurrentDate =
			// GetNewYearsDay(m_lngCurrentYear) Then intHoliday = Holidays.NewYearsDay ElseIf dtCurrentDate = GetMartinLutherKingDay(m_lngCurrentYear) Then intHoliday = Holidays.MartinLutherKingDay
			// ElseIf dtCurrentDate = GetWashingtonBirthDay(m_lngCurrentYear) Then intHoliday = Holidays.WashingtonBirthday ElseIf dtCurrentDate = GetArmedForcesDay(m_lngCurrentYear) Then intHoliday =
			// Holidays.ArmedForcesDay ElseIf dtCurrentDate = GetMemorialDay(m_lngCurrentYear) Then intHoliday = Holidays.MemorialDay ElseIf dtCurrentDate = GetFlagDay(m_lngCurrentYear) Then intHoliday
			// = Holidays.FlagDay ElseIf dtCurrentDate = GetIndependenceDay(m_lngCurrentYear) Then intHoliday = Holidays.IndependenceDay ElseIf dtCurrentDate = GetLaborDay(m_lngCurrentYear) Then
			// intHoliday = Holidays.LaborDay ElseIf dtCurrentDate = GetColumbusDay(m_lngCurrentYear) Then intHoliday = Holidays.ColumbusDay ElseIf dtCurrentDate = GetElectionDay(m_lngCurrentYear)
			// Then intHoliday = Holidays.ElectionDay ElseIf dtCurrentDate = GetVeteransDay(m_lngCurrentYear) Then intHoliday = Holidays.VeteransDay ElseIf dtCurrentDate =
			// GetThanksgivingDay(m_lngCurrentYear) Then intHoliday = Holidays.ThanksgivingDay ElseIf dtCurrentDate = GetChristmasDay(m_lngCurrentYear) Then intHoliday = Holidays.ChristmasDay End If

			switch (intHoliday) {
				case Holidays.NewYearsDay:
					strReturnHoliday = "New Year's Day";
					break;

				case Holidays.MartinLutherKingDay:
					strReturnHoliday = "Martin Luther King Day";
					break;

				case Holidays.WashingtonBirthday:
					strReturnHoliday = "Washington's Birthday";
					break;

				case Holidays.ArmedForcesDay:
					strReturnHoliday = "Armed Forces Day";
					break;

				case Holidays.MemorialDay:
					strReturnHoliday = "Memorial Day";
					break;

				case Holidays.FlagDay:
					strReturnHoliday = "Flag Day";
					break;

				case Holidays.IndependenceDay:
					strReturnHoliday = "Independence Day";
					break;

				case Holidays.LaborDay:
					strReturnHoliday = "Labor Day";
					break;

				case Holidays.ColumbusDay:
					strReturnHoliday = "Columbus Day";
					break;

				case Holidays.ElectionDay:
					strReturnHoliday = "Election Day";
					break;

				case Holidays.VeteransDay:
					strReturnHoliday = "Veterans Day";
					break;

				case Holidays.ThanksgivingDay:
					strReturnHoliday = "Thanksgiving Day";
					break;

				case Holidays.ChristmasDay:
					strReturnHoliday = "Christmas Day";
					break;

				case Holidays.DaylightSavingsBegins:
					strReturnHoliday = "Daylight Savings Begins";
					break;

				case Holidays.DaylighSavingsEnd:
					strReturnHoliday = "Daylight Savings Ends";
					break;

				case Holidays.NonHoliday:
					strReturnHoliday = string.Empty;
					break;
			}

			return intHoliday;
		}

		/// <summary>Gets the new years day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetNewYearsDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;

			dtTestDate = DateTime.Parse("01/01/" + p_lngYear);
			dtTestDate = CompensateForWeekend(dtTestDate);

			AddHolidayToHashtable(Holidays.NewYearsDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the martin luther king day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetMartinLutherKingDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			int intTick = 0;

			dtTestDate = DateTime.Parse("01/01/" + p_lngYear);
			intTick = 1;
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Monday & intTick == 3) {
					break; /* TRANSWARNING: check that break is in correct scope */
				} else if (dtTestDate.DayOfWeek == DayOfWeek.Monday) {
					intTick = intTick + 1;
				}
				dtTestDate = dtTestDate.AddDays(1);
			} while (true);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.MartinLutherKingDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the washington birth day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetWashingtonBirthDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			int intTick = 0;

			dtTestDate = DateTime.Parse("02/01/" + p_lngYear);
			intTick = 1;
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Monday & intTick == 3) {
					break; /* TRANSWARNING: check that break is in correct scope */
				} else if (dtTestDate.DayOfWeek == DayOfWeek.Monday) {
					intTick = intTick + 1;
				}
				dtTestDate = dtTestDate.AddDays(1);
			} while (true);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.WashingtonBirthday, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the armed forces day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetArmedForcesDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			int intTick = 0;

			dtTestDate = DateTime.Parse("05/01/" + p_lngYear);
			intTick = 1;
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Saturday & intTick == 3) {
					break; /* TRANSWARNING: check that break is in correct scope */
				} else if (dtTestDate.DayOfWeek == DayOfWeek.Saturday) {
					intTick = intTick + 1;
				}
				dtTestDate = dtTestDate.AddDays(1);
			} while (true);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.ArmedForcesDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the memorial day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetMemorialDay(long p_lngYear) {
			// Test for Memorial Day (Last Monday in May
			DateTime dtTestDate = DateTime.MinValue;

			dtTestDate = DateTime.Parse("05/01/" + p_lngYear);
			DateTime dtLastMonday = DateTime.MinValue;
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Monday) {
					dtLastMonday = dtTestDate;
				}
				if (dtTestDate.Month != 5) {
					break; /* TRANSWARNING: check that break is in correct scope */
				}
				dtTestDate = dtTestDate.AddDays(1);
			} while (true);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.MemorialDay, dtTestDate);
			return dtLastMonday;
		}

		/// <summary>Gets the flag day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetFlagDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			dtTestDate = DateTime.Parse("06/14/" + p_lngYear);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.FlagDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the independence day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetIndependenceDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			dtTestDate = DateTime.Parse("07/04/" + p_lngYear);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.IndependenceDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the labor day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetLaborDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;

			dtTestDate = DateTime.Parse("09/01/" + p_lngYear);
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Monday) {
					break; /* TRANSWARNING: check that break is in correct scope */
				} else {
					dtTestDate = dtTestDate.AddDays(1);
				}
			} while (true);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.LaborDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the columbus day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetColumbusDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			int intTick = 0;

			dtTestDate = DateTime.Parse("10/01/" + p_lngYear);
			intTick = 1;
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Monday & intTick == 2) {
					break; /* TRANSWARNING: check that break is in correct scope */
				} else if (dtTestDate.DayOfWeek == DayOfWeek.Monday) {
					intTick = intTick + 1;
				}
				dtTestDate = dtTestDate.AddDays(1);
			} while (true);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.ColumbusDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the election day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetElectionDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;

			dtTestDate = DateTime.Parse("11/01/" + p_lngYear);
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Tuesday & dtTestDate >= DateTime.Parse("11/02/" + p_lngYear)) {
					break; /* TRANSWARNING: check that break is in correct scope */
				}
				dtTestDate = dtTestDate.AddDays(1);
			} while (true);

			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.ElectionDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the veterans day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetVeteransDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			dtTestDate = DateTime.Parse("11/11/" + p_lngYear);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.VeteransDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the thanksgiving day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetThanksgivingDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			int intTick = 0;

			dtTestDate = DateTime.Parse("11/01/" + p_lngYear);
			intTick = 1;
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Thursday & intTick == 4) {
					break; /* TRANSWARNING: check that break is in correct scope */
				} else if (dtTestDate.DayOfWeek == DayOfWeek.Thursday) {
					intTick = intTick + 1;
				}
				dtTestDate = dtTestDate.AddDays(1);
			} while (true);

			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.ThanksgivingDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the christmas day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetChristmasDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			dtTestDate = DateTime.Parse("12/25/" + p_lngYear);
			dtTestDate = CompensateForWeekend(dtTestDate);
			AddHolidayToHashtable(Holidays.ChristmasDay, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the daylight savings starting day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetDaylightSavingStartingDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			int intTick = 0;

			dtTestDate = DateTime.Parse("03/01/" + p_lngYear);
			intTick = 1;
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Sunday & intTick == 2) {
					break;
				} else if (dtTestDate.DayOfWeek == DayOfWeek.Sunday) {
					intTick = intTick + 1;
				}
				dtTestDate = dtTestDate.AddDays(1);
			} while (true);

			AddHolidayToHashtable(Holidays.DaylightSavingsBegins, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Gets the daylight saving ending day.</summary>
		/// <param name="p_lngYear">The P_LNG year.</param>
		/// <returns></returns>
		public DateTime GetDaylightSavingEndingDay(long p_lngYear) {
			DateTime dtTestDate = DateTime.MinValue;
			int intTick = 0;

			dtTestDate = DateTime.Parse("11/01/" + p_lngYear);
			intTick = 1;
			do {
				if (dtTestDate.DayOfWeek == DayOfWeek.Sunday & intTick == 1) {
					break;
				} else if (dtTestDate.DayOfWeek == DayOfWeek.Sunday) {
					intTick = intTick + 1;
				}
				dtTestDate = dtTestDate.AddDays(1);
			} while (true);

			AddHolidayToHashtable(Holidays.DaylighSavingsEnd, dtTestDate);
			return dtTestDate;
		}

		/// <summary>Adds the holiday to hashtable.</summary>
		/// <param name="intHoliday">   The int holiday.</param>
		/// <param name="p_HolidayDate">The p_ holiday date.</param>
		private void AddHolidayToHashtable(Holidays intHoliday, DateTime p_HolidayDate) {
			HolidayItem newHolidayItem = new();
			newHolidayItem.Holiday = intHoliday;
			newHolidayItem.HolidayDate = p_HolidayDate;
			newHolidayItem.HolidayYear = p_HolidayDate.Year;

			m_Holidays.Add(newHolidayItem);
		}

		/// <summary>Compensates for weekend.</summary>
		/// <param name="p_Date">The p_ date.</param>
		/// <returns></returns>
		private DateTime CompensateForWeekend(DateTime p_Date) {
			// ---------------------------------------- Check if the holiday lands on a Weekend. All federal holidays that land on a weekend are observed on the following monday. ----------------------------------------
			if (p_Date.DayOfWeek == DayOfWeek.Saturday) {
				p_Date = p_Date.AddDays(2);
			}
			if (p_Date.DayOfWeek == DayOfWeek.Sunday) {
				p_Date = p_Date.AddDays(1);
			}

			return p_Date;
		}

		/// <summary>Loads the holiday hashtable.</summary>
		private void LoadHolidayList() {
			m_Holidays = new List<HolidayItem>();
			GetNewYearsDay(m_lngCurrentYear);
			GetMartinLutherKingDay(m_lngCurrentYear);
			GetWashingtonBirthDay(m_lngCurrentYear);
			GetArmedForcesDay(m_lngCurrentYear);
			GetMemorialDay(m_lngCurrentYear);
			GetFlagDay(m_lngCurrentYear);
			GetIndependenceDay(m_lngCurrentYear);
			GetLaborDay(m_lngCurrentYear);
			GetColumbusDay(m_lngCurrentYear);
			GetElectionDay(m_lngCurrentYear);
			GetVeteransDay(m_lngCurrentYear);
			GetThanksgivingDay(m_lngCurrentYear);
			GetChristmasDay(m_lngCurrentYear);
			GetDaylightSavingStartingDay(m_lngCurrentYear);
			GetDaylightSavingEndingDay(m_lngCurrentYear);
		}

		// private HolidayItem GetHolidayItem( DateTime p_Date ) { HolidayItem thisHoliday = new HolidayCalc.HolidayItem();
		//
		// foreach( HolidayItem transTemp0 in m_colHolidays ) { thisHoliday = transTemp0; /* TRANSWARNING: check temp variable in foreach */ if( thisHoliday.HolidayDate.ToString( "MM-dd-yyyy" ) ==
		// p_Date.ToString( "MM-dd-yyyy" ) ) { break; /* TRANSWARNING: check that break is in correct scope */ } } return thisHoliday; }

		/// <summary>Gets the holiday item hash.</summary>
		/// <param name="p_Date">The p_ date.</param>
		/// <returns></returns>
		private HolidayItem GetHolidayItemHash(DateTime p_Date) {
			HolidayItem thisHoliday = new();

			thisHoliday.Holiday = Holidays.NonHoliday;
			foreach (HolidayItem item in m_Holidays) {
				if (item.HolidayDate == p_Date) {
					thisHoliday = item;
					break;
				}
			}
			return thisHoliday;
		}

		/// <summary>Gets the quarters for passed year.</summary>
		/// <param name="year">The year.</param>
		/// <returns></returns>
		public List<DateTime> GetQuarters(Int32 year) {
			List<DateTime> quarters = new();
			Int32 previousYear = year - 1;

			quarters.Add(DateTime.Parse("11/1/" + previousYear.ToString()));
			quarters.Add(DateTime.Parse("2/1/" + year.ToString()));
			quarters.Add(DateTime.Parse("5/1/" + year.ToString()));
			quarters.Add(DateTime.Parse("8/1/" + year.ToString()));

			return quarters;
		}

		/// <summary>Gets the quarters for the passed date.</summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public List<DateTime> GetQuarters(DateTime date) {
			return GetQuarters(date.Year);
		}

		/// <summary>Gets the current quarter.</summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public Quarter GetCurrentQuarter(DateTime date) {
			Quarter currentQtr = Quarter.First;

			switch ((Month)date.Month) {
				case Month.November:
				case Month.December:
				case Month.January:
					currentQtr = Quarter.First;
					break;

				case Month.February:
				case Month.March:
				case Month.April:
					currentQtr = Quarter.Second;
					break;

				case Month.May:
				case Month.June:
				case Month.July:
					currentQtr = Quarter.Third;
					break;

				case Month.August:
				case Month.September:
				case Month.October:
					currentQtr = Quarter.Fourth;
					break;
			}
			return currentQtr;
		}

		/// <summary>Gets the quarter start and end date.</summary>
		/// <param name="quarter">The quarter.</param>
		/// <returns></returns>
		public List<DateTime> GetQuarterStartAndEndDate(Quarter quarter, Int32 year) {
			List<DateTime> dates = new();
			DateTime date = DateTime.Parse("1/1/" + year);
			switch (quarter) {
				case Quarter.First:
					dates.Add(DateTime.Parse("11/1/" + (date.Year - 1)));
					dates.Add(DateTime.Parse("1/31/" + date.Year));
					break;

				case Quarter.Second:
					dates.Add(DateTime.Parse("2/1/" + date.Year));
					dates.Add(DateTime.Parse("4/30/" + date.Year));
					break;

				case Quarter.Third:
					dates.Add(DateTime.Parse("5/1/" + date.Year));
					dates.Add(DateTime.Parse("7/31/" + date.Year));
					break;

				case Quarter.Fourth:
					dates.Add(DateTime.Parse("8/1/" + date.Year));
					dates.Add(DateTime.Parse("10/31/" + date.Year));
					break;
			}
			return dates;
		}

		/// <summary>Gets the current quarter start and end dates.</summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public List<DateTime> GetQuarterStartAndEndDate(DateTime date) {
			Quarter currentQuarter = GetCurrentQuarter(date);
			return GetQuarterStartAndEndDate(currentQuarter, date.Year);
		}

		/// <summary>Gets all the quarter start and end dates for the passed date.</summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public List<DateTime> GetAllQuarterStartAndEndDates(DateTime date) {
			return GetAllQuarterStartAndEndDates(date.Year);
		}

		/// <summary>Gets all quarter start and end dates for the passed year.</summary>
		/// <param name="year">The year.</param>
		/// <returns></returns>
		public List<DateTime> GetAllQuarterStartAndEndDates(Int32 year) {
			DateTime date = DateTime.Parse("1/1/" + year);
			List<DateTime> quarterDates = new();
			List<DateTime> dates = new();
			quarterDates = GetQuarterStartAndEndDate(Quarter.First, year);
			dates.Add(quarterDates[0]);
			dates.Add(quarterDates[1]);
			quarterDates = GetQuarterStartAndEndDate(Quarter.Second, year);
			dates.Add(quarterDates[0]);
			dates.Add(quarterDates[1]);
			quarterDates = GetQuarterStartAndEndDate(Quarter.Third, year);
			dates.Add(quarterDates[0]);
			dates.Add(quarterDates[1]);
			quarterDates = GetQuarterStartAndEndDate(Quarter.Fourth, year);
			dates.Add(quarterDates[0]);
			dates.Add(quarterDates[1]);
			return dates;
		}

		#endregion Methods

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="HolidayCalculator" /> class.</summary>
		public HolidayCalculator() {
			m_Holidays = new List<HolidayItem>();
		}

		/// <summary>Initializes a new instance of the <see cref="HolidayCalculator" /> class.</summary>
		/// <param name="currentYear">The current year.</param>
		public HolidayCalculator(long currentYear) {
			m_lngCurrentYear = currentYear;
			LoadHolidayList();
		}

		#endregion Constructors
	}
}