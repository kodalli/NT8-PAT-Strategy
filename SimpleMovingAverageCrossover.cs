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
	public class SimpleMovingAverageCrossover : Strategy
	{
		private SMA fastSMA, slowSMA;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"This strategy will trade when the 5 SMA crosses the 20 SMA";
				Name										= "SimpleMovingAverageCrossover";
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
				Fast = 5;
				Slow = 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
				fastSMA = SMA(Fast);
				slowSMA = SMA(Slow);
				//AddDataSeries("ES 12-20", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);

				// Anytime we are in a live position, we have a 100$ take profit
				SetProfitTarget(CalculationMode.Currency, 100);

				// Set a stop loss of 50$
				SetStopLoss(CalculationMode.Currency, 25);
			} else if(State == State.DataLoaded)
            {
				// Draw SMA values on chart
				fastSMA.Plots[0].Brush = Brushes.Goldenrod;
				slowSMA.Plots[0].Brush = Brushes.SeaGreen;

				AddChartIndicator(fastSMA);
				AddChartIndicator(slowSMA);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			//Enter a long or short trade when the 5 SMA crosses the 20 SMA.
			if(CrossAbove(fastSMA, slowSMA, 1))
            {
				EnterLong();
            } else if(CrossBelow(fastSMA, slowSMA, 1))
            {
				EnterShort();
            }

			if(Position.MarketPosition == MarketPosition.Long)
            {
				if (CrossBelow(fastSMA, slowSMA, 1))
					ExitLong();
			}
			else if(Position.MarketPosition == MarketPosition.Short)
            {
				if (CrossAbove(fastSMA, slowSMA, 1))
                {
					ExitShort();
                }
            }



		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public int Fast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int Slow
		{ get; set; }
		#endregion
	}
}
