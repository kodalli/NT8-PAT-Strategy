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
	public class SimpleShortStrategy : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Short on very bearish bars that cross below the EMA";
				Name										= "SimpleShortStrategy";
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
				Stop = 16; //$200
				Goal = 8; //$100
                Threshold = 0.7;
				SlopeRange2 = SlopeRange = 5;
				MinTickDiff = 3;
				// MaxSlope = 0.2;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= false;
			}
			else if (State == State.Configure)
			{
				AddDataSeries("ES 09-19", Data.BarsPeriodType.Tick, 2000, Data.MarketDataType.Last);
				SetStopLoss(CalculationMode.Ticks, Stop);
				SetProfitTarget(CalculationMode.Ticks, Goal);
			}
		}
		//private bool first_test_short = true;
		//private bool first_test_long = true;
		private bool secondentry = false;
		private int barattempts = 20;
		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0)
				return;
			#region
			//// Calculate bearishness/bull from 0:1 
			//double _Bearishness = (Open[0] - Close[0]) / (High[0] - Low[0]);
			//if (_Bearishness > Threshold && Open[0] > EMA(21)[0] && Close[0] < EMA(21)[0] && Bars.BarsSinceNewTradingDay >= 10)
			//         {
			//	//if(Open[0] > EMA(21)[0] && Close[0] < EMA(21)[0])
			//	//            {
			//	//	if(CountIf(() => Low[0] > EMA(21)[0], 15) > 12)
			//	//                {
			//	//		EnterShort();
			//	//                }
			//	//	//if (LinRegSlope(21)[2] < LinRegSlope(21)[1] && LinRegSlope(21)[0] < LinRegSlope(21)[1])
			//	//	//{
			//	//	//	// Check if bearishness exceeds threshold
			//	//	//	if (Slope(EMA(21), 5, 0) > 0.2)
			//	//	//		EnterShort();
			//	//	//	else if (CurrentDayOHL().CurrentHigh[10] > CurrentDayOHL().CurrentHigh[0])
			//	//	//		EnterShort();
			//	//	//}
			//	//}

			//	//Slope(EMA(21), 5, 0) > 0.05 &&
			//	// dojis Close[1] - Open[1] == 0.0

			//	// Parabola check

			//	if (EMA(14)[0] > EMA(21)[0] && (CrossAbove(SMA(21), EMA(21), 10) || CrossBelow(SMA(21), EMA(21), 10)))
			//		EnterShort();
			//	//if (EMA(14)[0] > EMA(21)[0])
			// //            {
			//	//	EnterShort();
			// //            }
			//         }
			#endregion
			// Check for uptrend
			if (Bars.BarsSinceNewTradingDay >= 10 && (CountIf(() => Low[1] > EMA(21)[1], 10) > 7 || CountIf(() => Close[1]-Open[1] > 1.5, 5) > 2 || secondentry))
			{
				secondentry = ShortSrat(secondentry);
			}
		}

        private bool ShortSrat(bool check)
        {
            
            // Look for first entry reversal
            if (check)
            {
				if (MAX(High, 20)[0] > MAX(High, 15)[5] || MAX(Open, 20)[0] > MAX(Open, 15)[10])
				{
					// New High reached
					EnterShort();
					barattempts = 20;
					return false;
				}
				else if (barattempts > 0)
				{
					barattempts--;
					return true;
				}
                else
                {
					barattempts = 20;
					return false;
                }
				
            }
			
			if (Open[0]-Close[0] > Threshold && Close[0]-Close[5] > MinTickDiff)
            {
				// See if we just hit a new high
				if(MAX(High, 15)[0] > MAX(High, 15)[5])//CurrentDayOHL().CurrentHigh[0] > CurrentDayOHL().CurrentHigh[SlopeRange])
                {
					// Print(Time[0]);
					return true;
					//Wait for new high to be reached on previous bar then short
                }

			}

			return false;
        }

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public int Stop
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Profit Target", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int Goal
		{ get; set; }

        [Range(0.0, double.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Threshold", GroupName = "NinjaScriptStrategyParameters", Order = 2)]
        public double Threshold
        { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "EMA slope range", GroupName = "NinjaScriptStrategyParameters", Order = 3)]
        public int SlopeRange
        { get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EMA slope range2", GroupName = "NinjaScriptStrategyParameters", Order = 4)]
		public int SlopeRange2
		{ get; set; }

		[Range(1, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Min Tick Diff on up trend", GroupName = "NinjaScriptStrategyParameters", Order = 5)]
		public double MinTickDiff
		{ get; set; }
		#endregion
	}
}
