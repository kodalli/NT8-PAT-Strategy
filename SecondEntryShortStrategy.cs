#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class SecondEntryShortStrategy : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Detect second pull back to EMA for a short entry";
				Name										= "SecondEntryShortStrategy";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				StopLoss = 8;
				ProfitTarget = 4;
				//LocalExtremaRange = 10;
				//MajorTrendRange = 20;

				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= false;
			}
			else if (State == State.Configure)
			{
				AddDataSeries("ES 03-19", Data.BarsPeriodType.Tick, 2000, Data.MarketDataType.Last);
				SetStopLoss(CalculationMode.Ticks, StopLoss); //submits stop loss order on entry
				SetProfitTarget(CalculationMode.Ticks, ProfitTarget); //submits profit target order on entry
			}
		}
		private static int BARCOUNT = 20;
		int barcountdown = BARCOUNT;
		int trades;
		bool watchingforsecondentry = false;
		// List<string> trendlines = new List<string>();
		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;
			//barcountdown--;
			//if(Bars.BarsSinceNewTradingDay >= MajorTrendRange)
			//	MinEntryLong();

			//if(Bars.BarsSinceNewTradingDay >= 20)
			//         {
			//	//DoubleBotEnterLong();
			//	SecondEntry();
			//         }
			SecondEntry();
		}

        #region
        //private void MinEntryLong()
        //      {
        //	double currentLocalLow;
        //	double currentLocalHigh;
        //	// Look for major down trend, pulling away from EMA once bullishbar passes EMA
        //	if(CountIf(() => High[1] < EMA(21)[1], MajorTrendRange) > MajorTrendRange * 0.7 && Close[0] > EMA(21)[0] && Open[0] < EMA(21)[0]){
        //		// Check if reaching local minima
        //		if(MIN(Low, LocalExtremaRange)[0] == MIN(Low, LocalExtremaRange)[1])
        //              {
        //			currentLocalLow = MIN(Low, LocalExtremaRange)[1]; // previous value is the minimum
        //			EnterLong(); // Price likely to pull back to EMA
        //						 //Print(Time[0] + " Enter Long");
        //			trades++;
        //			DrawTrendLines(trades);
        //		}
        //	}
        //	// Look for major up trend, pulling away from EMA once bearish passes EMA
        //	else if (CountIf(() => Low[1] > EMA(21)[1], MajorTrendRange) > MajorTrendRange * 0.7 && Close[0] < EMA(21)[0] && Open[0] > EMA(21)[0])
        //	{
        //		// Check if reaching local minima
        //		if (MAX(High, LocalExtremaRange)[0] == MAX(High, LocalExtremaRange)[1])
        //		{
        //			currentLocalHigh= MAX(Low, LocalExtremaRange)[1]; // previous value is the minimum
        //			EnterShort(); // Price likely to pull back to EMA
        //			//Print(Time[0] + " Enter Short");
        //		}
        //	}
        //}

        //private void DrawTrendLines(int trades)
        //      {
        //	//Draw.ExtendedLine(this, "tag" + trades.ToString(), 10, High[10], 0, Close[0] > Open[0] ? Close[0]:Open[0], Brushes.Aqua, DashStyleHelper.Solid, 2);
        //	//Draw.Line(this, "tag" + trades.ToString(), false,  10, High[10], 0, Close[0] > Open[0] ? Close[0] : Open[0], Brushes.Aqua, DashStyleHelper.Solid, 2);
        //	//double value = Pivots(PivotRange.Daily, HLCCalculationMode.CalcFromIntradayData, 0, 0, 0, 20).S1[0];
        //	//Draw.HorizontalLine(this, "tag" + trades.ToString(), value, Brushes.Aquamarine, DashStyleHelper.Solid, 2);
        //	List<double> maxima = new List<double>();
        //	if(barcountdown == 0)
        //          {
        //		maxima = FindLocalMaxima(20, 5);
        //		barcountdown = BARCOUNT;
        //          }
        //}

        //private List<double> FindLocalMaxima(int lookbackbars, int comparerange)
        //      {
        //	List<double> maxima = new List<double>();
        //	if (Bars.BarsSinceNewTradingDay < lookbackbars)
        //		return maxima;

        //	// Find the maxima
        //	for (int i = lookbackbars-comparerange; i >= 0; i--)
        //          {
        //		//HighestBar(High, comparerange);
        //		maxima.Add(MAX(High, comparerange)[i]);
        //          }
        //	return maxima;
        //}

        //private void DoubleBotEnterLong()
        //      {
        //	if(Low[1] < MIN(Low, 20)[2] && Open[0] < Close[0] && Math.Abs(Low[0] - Low[1]) < 0.25 && Open[0] > (High[0]+Low[0])/2 && Close[0] > (High[0] + Low[0]) / 2)
        //          {
        //		EnterLong();
        //          }
        //      }
        #endregion
        private void SecondEntry()
        {
			if (CandlestickPattern(ChartPattern.BullishHarami, 4)[0] == 1)
				EnterLong();
        }

		#region
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public int StopLoss
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Profit Target", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int ProfitTarget
		{ get; set; }

		//[Range(1, int.MaxValue), NinjaScriptProperty]
		//[Display(ResourceType = typeof(Custom.Resource), Name = "Local Extrema Bar Range", GroupName = "NinjaScriptStrategyParameters", Order = 2)]
		//public int LocalExtremaRange
		//{ get; set; }

		//[Range(1, int.MaxValue), NinjaScriptProperty]
		//[Display(ResourceType = typeof(Custom.Resource), Name = "Major Trend Bar Range", GroupName = "NinjaScriptStrategyParameters", Order = 3)]
		//public int MajorTrendRange
		//{ get; set; }
		#endregion
	}	
	
}
