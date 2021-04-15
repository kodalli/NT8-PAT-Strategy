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
	public class futurePullbackScalping : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"determines trend and scalps accordingly on pullback to 21 EMA";
				Name										= "futurePullbackScalping";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 2;
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
				BarsRequiredToTrade							= 50;
				StopLoss									= 8; //sets distance to stop loss from entry
				ProfitTarget								= 4; //sets distance to profit target from entry
				Fast										= 5; //sets bars used to calculate fast SMA
				Slow										= 20; //sets bars used to calculate slow SMA
				BarLookBackForTrend							= 40; //sets bars used to calculate trend direction
				PercentBarsRequiredAboveEMA					= 0.8; //sets percent bars required above or below EMA to confirm trend
				SlopeForTrend								= 1; //sets minimum slope required to establish trend
				TicksForPullback							= 2; //sets minimum number of ticks price has to cross EMA by to confirm pullback
				BullishThreshhold							= 3; //sets minimum number of ticks difference between open and close to confirm bars bullishness

				IsInstantiatedOnEachOptimizationIteration	= false;
			}
			else if (State == State.Configure)
			{
				AddDataSeries("ES 06-20", Data.BarsPeriodType.Tick, 2000, Data.MarketDataType.Last);
				
				SetStopLoss(CalculationMode.Ticks, StopLoss); //submits stop loss order on entry
				SetProfitTarget(CalculationMode.Ticks, ProfitTarget); //submits profit target order on entry
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (testTrendUp()) 
			{
				double currentPrice = Close[0];
				if (testBelowEMA(currentPrice) & testLastBarBullish())
					EnterLong();
			}

		}

		protected bool testTrendUp() //returns true if market in up trend
		{
			bool inUpTrend = false; //initialize bool describing whether market is in uptrend (true) or not (false)
			bool fastAboveSlow = false; //initialize bool describing whether fast SMA is 5 or more ticks above slow SMA (true) or not (false)
			int barsAboveEMA = 0; //intialize counter for number of bars above EMA
			bool percentBarsAboveEMA = false; //initialize bool describing whether the percent of bars in the BarLookBackForTrend period is above PercentBarsRequiredAboceEMA (true) or below (false)
			bool slopeAboveSlopeForTrend = false; //initialize bool describing whether slope of regression is above SlopeForTrend (true) or below SlopeForTrend (false)

			if (SMA(Fast)[0] - SMA(Slow)[0] > 4) //tests if fast SMA is 5 or more ticks above slow SMA and sets fastAboveSlow to true if conditions are met
				fastAboveSlow = true;
			
			//for(int i = 0; i < BarLookBackForTrend; i++) //counts number of bars in past 40 above EMA
			//{
			//	if (EMA(21)[0] < Close[0])
			//		barsAboveEMA++;
			//}
			

			if (Convert.ToDouble(barsAboveEMA) / Convert.ToDouble(BarLookBackForTrend) > PercentBarsRequiredAboveEMA) //test if percent of bars in past BarLookBackForTrend is greater than PercentBarsRequiredAboveEMA and set percentBarsAboveEMA to true if conditions are met
				percentBarsAboveEMA = true;


			if (LinRegSlope(BarLookBackForTrend)[0] >= SlopeForTrend) //test if slope for linear regression of last BarLookBackForTrend bars is above SlopeForTrend and sets slopeAboveSlopeForTrend to true if conditions are met
				slopeAboveSlopeForTrend = true;

			if (fastAboveSlow & percentBarsAboveEMA) //tests if all three conditions of uptrend are true and sets inUpTrend to true if conditions are met
				inUpTrend = true;

			return inUpTrend;
		}
		protected bool testBelowEMA(double price) // returns true if current price is at least TicksForPullBack ticks below 21 EMA
		{
			bool isBelowEMA = false; //initialize bool describing whether current price is at least TicksForPullBack ticks below 21 EMA (true) or not (false)

			if (EMA(21)[0] - price >= TicksForPullback) //test if current price is at least TicksForPullback ticks below 21 EMA and sets isBelowEMA to true if conditions are met
				isBelowEMA = true;

			return isBelowEMA;
		}

		protected bool testLastBarBullish() //returns true if last bar meets BullishThreshhold
		{
			bool isBull = false; //initialize bool describing whether last bar meets BullishThreshhold (True) or not (False)

			
			if (Close[1] - Open[1] >= BullishThreshhold) //Tests if last bar meets BullishThreshold and sets isBull to true if conditions are met
				isBull = true;
			

			return isBull;
		}
		#region Properties
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slope For Determining Trend", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public double SlopeForTrend
		{ get; set; }

		[Range(0.0, 1.0), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "% Bars Above or Below EMA Required For Trend", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public double PercentBarsRequiredAboveEMA
		{ get; set; }

		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Difference Between Open and Close Required to Determine Bullish", GroupName = "NinjaScriptStrategyParameters", Order = 2)]
		public int BullishThreshhold
		{ get; set; }

		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Ticks Required to Confirm Pullback", GroupName = "NinjaScriptStrategyParameters", Order = 3)]
		public int TicksForPullback
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bar Look Back For Trend", GroupName = "NinjaScriptStrategyParameters", Order = 4)]
		public int BarLookBackForTrend
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss", GroupName = "NinjaScriptStrategyParameters", Order = 5)]
		public int StopLoss
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Profit Target", GroupName = "NinjaScriptStrategyParameters", Order = 6)]
		public int ProfitTarget
		{ get; set; }

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptStrategyParameters", Order = 7)]
		public int Fast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptStrategyParameters", Order = 8)]
		public int Slow
		{ get; set; }
		#endregion
		#endregion
	}
}
