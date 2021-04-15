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
	public class scalpingUpTrend : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "scalpingUpTrend";
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
				BarsRequiredToTrade							= 20;
				Stop = 8;
				Goal = 4;
				Fast = 5;
				Slow = 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= false;
			}
			else if (State == State.Configure)
			{
				// ES 09-20
				AddDataSeries("ES 09-19", Data.BarsPeriodType.Tick, 2000, Data.MarketDataType.Last);
				SetStopLoss(CalculationMode.Ticks, Stop);
				SetProfitTarget(CalculationMode.Ticks, Goal);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;
			
			bool currentTrend = testUpTrend();

			if (currentTrend)
			{
				double currentPrice = Close[0];
				bool belowEMA = testBelowEMA(currentPrice);

				if (belowEMA) 
				{
					EnterLong();
				}
			}

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

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptStrategyParameters", Order = 2)]
		public int Fast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptStrategyParameters", Order = 3)]
		public int Slow
		{ get; set; }
		#endregion
		protected bool testUpTrend()
		{
			bool upTrend = false; //initialize uptrend
			
			double fastCurrent = SMA(Fast)[0]; //initialize fastCurrent to current value of fast SMA
			double slowCurrent = SMA(Slow)[0]; //initialize slowCurrent to current value of slow SMA

			//sets upTrend to true if fast SMA is currently above slow SMA 
			if (fastCurrent > slowCurrent)
				upTrend = true;
			
			return upTrend;

		}
		protected bool testBelowEMA(double price) 
		{
			bool isBelowEMA = false; //initialize isBelowEMA
			double currentEMA = EMA(21)[0]; //initialize currentEMA

			//test if current price is below 21 EMA and set isBelowEMA accordingly
			if (price < currentEMA) 
			{
				isBelowEMA = true;
			}

			return isBelowEMA;

		}
	}
}
