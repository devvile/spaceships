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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Security.Cryptography;
using System.ComponentModel;


namespace NinjaTrader.NinjaScript.Strategies
{
    public class Apollo_REBORN : Strategy
    {
        #region Declarations
        private int LotSize1;
        private int LotSize2;
        private int LotSize3;
        private int _userLotSize1;
        private int _userLotSize2;
        private int _userLotSize3;
        private Indicator _ha;
        private Indicator _aroon;
        private Indicator _stoch;
        private Indicator _iczimoku;

        Random rnd = new Random();
        private int _aroonPeriod;
        private bool _useAroon;
        private bool _showHA;
        private bool _useRsi;
        private bool _makeTrades;
        private bool _useLongs;
        private bool _useShorts;
        private bool _useIchimoku;
        private double _stochFast;
        private bool _useStrongSignals;
        private bool _useWeakSignals;

        private double _longEntryPrice1;
        private double _stopLossBaseLong;
        private int _profitTargetLong1;
        private int _profitTargetLong2;
        private int _profitTargetLong3;
        private Order _longOneOrder;
        private Order _longTwoOrder;
        private Order _longThreeOrder;
        private int _baseStopMarginLong;


        private double _shortEntryPrice1;
        private double _stopLossBaseShort;
        private int _profitTargetShort1;
        private int _profitTargetShort2;
        private int _profitTargetShort3;
        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private Order _shortThreeOrder;
        private int _baseStopMarginShort;

        private int _extraSize;
        #endregion

        #region Parameters

        private int _rsiEntryShort;
        private int _rsiEntryLong;

        private int _stochRsiPeriod;
        private int _fastMAPeriod;
        private int _slowMAPeriod;
        private int _lookBack;
        private string status = "Flat";
        private bool _canTrade = false;
        private string signal = "No Signal";

        //   private int _minuteIndex =0 ;
        #endregion


        #region Config

        [Display(Name = "Aroon Period", GroupName = "Config", Order = 4)]
        public int AroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }

        [Display(Name = "Show HA", GroupName = "Config", Order = 1)]
        public bool ShowHA
        {
            get { return _showHA; }
            set { _showHA = value; }
        }


        [Display(Name = "Use Aroon", GroupName = "Config", Order = 3)]
        public bool UseAroon
        {
            get { return _useAroon; }
            set { _useAroon = value; }
        }

        [Display(Name = "Use RSI", GroupName = "Config", Order = 2)]
        public bool UseRsi
        {
            get { return _useRsi; }
            set { _useRsi = value; }
        }


        [Display(Name = "Trade", GroupName = "Config", Order = 0)]
        public bool makeTrades
        {
            get { return _makeTrades; }
            set { _makeTrades = value; }
        }


        #endregion

        #region Ichimoku
        [Display(Name = "Use Ichimoku", GroupName = "Ichimoku", Order = 0)]
        public bool UseIchimoku
        {
            get { return _useIchimoku; }
            set { _useIchimoku = value; }
        }

        [Display(Name = "Use Strong Signals", GroupName = "Ichimoku", Order = 0)]
        public bool UseStrongSignals
        {
            get { return _useStrongSignals; }
            set { _useStrongSignals = value; }
        }

        [Display(Name = "Use Weak Signals", GroupName = "Ichimoku", Order = 0)]
        public bool UseWeakSignals
        {
            get { return _useWeakSignals; }
            set { _useWeakSignals = value; }
        }
        #endregion

        #region Position Management

        [Display(Name = "Entry Size 1", GroupName = "Position Management", Order = 0)]
        public int UserLotSize1
        {
            get { return _userLotSize1; }
            set { _userLotSize1 = value; }
        }

        [Display(Name = "Entry Size 2", GroupName = "Position Management", Order = 0)]
        public int UserLotSize2
        {
            get { return _userLotSize2; }
            set { _userLotSize2 = value; }
        }

        [Display(Name = "Entry Size 3", GroupName = "Position Management", Order = 0)]
        public int UserLotSize3
        {
            get { return _userLotSize3; }
            set { _userLotSize3 = value; }
        }

        #endregion

        #region Long

        [Display(Name = "Use Longs", GroupName = "Long", Order = 0)]
        public bool UseLongs
        {
            get { return _useLongs; }
            set { _useLongs = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Long", Order = 0)]
        public int RsiEntryLong
        {
            get { return _rsiEntryLong; }
            set { _rsiEntryLong = value; }
        }

        [Display(Name = "Profit Target 1", GroupName = "Long", Order = 0)]
        public int ProfitTargetLong1
        {
            get { return _profitTargetLong1; }
            set { _profitTargetLong1 = value; }
        }

        [Display(Name = "Profit Target 2", GroupName = "Long", Order = 0)]
        public int ProfitTargetLong2
        {
            get { return _profitTargetLong2; }
            set { _profitTargetLong2 = value; }
        }

        [Display(Name = "Profit Target 3", GroupName = "Long", Order = 0)]
        public int ProfitTargetLong3
        {
            get { return _profitTargetLong3; }
            set { _profitTargetLong3 = value; }
        }

        [Display(Name = "Base Stop Margin", GroupName = "Long", Order = 0)]
        public int BaseStopMarginLong
        {
            get { return _baseStopMarginLong; }
            set { _baseStopMarginLong = value; }
        }

        #endregion

        #region Short

        [Display(Name = "Use Shorts", GroupName = "Short", Order = 0)]
        public bool UseShorts
        {
            get { return _useShorts; }
            set { _useShorts = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Short", Order = 0)]
        public int RsiEntryShort
        {
            get { return _rsiEntryShort; }
            set { _rsiEntryShort = value; }
        }
        [Display(Name = "Profit Target 1", GroupName = "Short", Order = 0)]
        public int ProfitTargetShort1
        {
            get { return _profitTargetShort1; }
            set { _profitTargetShort1 = value; }
        }

        [Display(Name = "Profit Target 2", GroupName = "Short", Order = 0)]
        public int ProfitTargetShort2
        {
            get { return _profitTargetShort2; }
            set { _profitTargetShort2 = value; }
        }

        [Display(Name = "Profit Target 3", GroupName = "Short", Order = 0)]
        public int ProfitTargetShort3
        {
            get { return _profitTargetShort3; }
            set { _profitTargetShort3 = value; }
        }

        [Display(Name = "Base Stop Margin", GroupName = "Short", Order = 0)]
        public int BaseStopMarginShort
        {
            get { return _baseStopMarginShort; }
            set { _baseStopMarginShort = value; }
        }


        #endregion

        #region STOCH

        [Display(Name = "RSI Period ", GroupName = "STOCH", Order = 0)]
        public int StochRsiPeriod
        {
            get { return _stochRsiPeriod; }
            set { _stochRsiPeriod = value; }
        }

        [Display(Name = "FastMAPeriod", GroupName = "STOCH", Order = 0)]
        public int FastMAPeriod
        {
            get { return _fastMAPeriod; }
            set { _fastMAPeriod = value; }
        }

        [Display(Name = "SlowMAPeriod", GroupName = "STOCH", Order = 0)]
        public int SlowMAPeriod
        {
            get { return _slowMAPeriod; }
            set { _slowMAPeriod = value; }
        }

        [Display(Name = "Look Back", GroupName = "STOCH", Order = 0)]
        public int LookBack
        {
            get { return _lookBack; }
            set { _lookBack = value; }
        }
        #endregion


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sputnik Refacatored";
                Name = "Apollo Reborn";
                Calculate = Calculate.OnBarClose;
                _userLotSize1 = 1;
                _userLotSize2 = 1;
                _userLotSize3 = 1;
                _profitTargetLong1 = 12;
                _profitTargetLong2 = 12;
                _profitTargetLong3 = 12;
                _baseStopMarginLong = 18;
                _baseStopMarginShort = 18;
                _profitTargetShort1 = 12;
                _profitTargetShort2 = 12;
                _profitTargetShort3 = 12;
                _stochRsiPeriod = 9;
                _fastMAPeriod = 3;
                _slowMAPeriod = 3;
                _lookBack = 14;

                _showHA = true;
                _useRsi = true;
                _makeTrades = false;
                _useLongs = true;
                _useShorts = true;
                _useIchimoku = true;
                _useStrongSignals = true;
                _useWeakSignals = true;

                _aroonPeriod = 25;
                _rsiEntryLong = 10;
                _rsiEntryShort = 90;
            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;
                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                addIndicators();
                Calculate = Calculate.OnBarClose;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar <= BarsRequiredToTrade) return;

            CalculateTradeTime();

            if (_canTrade)
            {
                TradeLikeAKing();

            }
        }
        private void TradeLikeAKing()
        {
            if (UseIchimoku)
            {
           //     Print(UseIchimoku);
                IchimokuMode();
            }
            else
            {
                NoIchimokuMode();
            }
        }

        private void IchimokuMode()
        {
            signal = calculateSignal();
            if (signal!="No Singal")
            {
                if (UseLongs && signal != "Strong Short" && signal != "Weak Short" && noPositions() && previousCandleGreen() && stochRsiEntry(RsiEntryLong, "Long"))
                {
                    if(UseStrongSignals && signal =="Strong Long")
                    {
                        if (makeTrades)
                        {
                            setUserPoisitonSizes();
                            _longOneOrder = EnterLong(LotSize1, "Long1");
                            _longTwoOrder = EnterLong(LotSize2, "Long2");
                            _longThreeOrder = EnterLong(LotSize3, "Long3");
                            ShowEntryDetails(signal);
                        }
                        else
                        {
                            ShowEntryDetails(signal);
                            Mark(signal);
                        }

                    }else if(UseWeakSignals && signal == "Weak Long")
                    {
                        if (makeTrades)
                        {
                            setSmallPositionSizes();
                            _longOneOrder = EnterLong(LotSize1, "Long1");
                            _longTwoOrder = EnterLong(LotSize2, "Long2");
                        }
                        else
                        {
                            ShowEntryDetails(signal);
                            Mark(signal);
                        }

                    }

                }
                else if(UseShorts && signal != "Strong Long" && signal != "Weak Long" && noPositions() && previousCandleRed() && stochRsiEntry(RsiEntryShort, "Short"))
                {
                    if (UseStrongSignals && signal == "Strong Short")
                    {
                        if (makeTrades)
                        {
                            setUserPoisitonSizes();
                            _shortOneOrder = EnterShort(LotSize1, "Short1");
                            _shortTwoOrder = EnterShort(LotSize2, "Short2");
                            _shortThreeOrder = EnterShort(LotSize3, "Short3");
                            ShowEntryDetails(signal);
                        }
                        else
                        {
                            ShowEntryDetails(signal);
                            Mark(signal);
                        }
                    }
                    else if (UseWeakSignals && signal == "Weak Short")
                    {
                        if (makeTrades)
                        {
                            setSmallPositionSizes();
                            _shortOneOrder = EnterShort(LotSize1, "Short1");
                            _shortTwoOrder = EnterShort(LotSize2, "Short2");
                            ShowEntryDetails(signal);
                        }
                        else
                        {
                            ShowEntryDetails(signal);
                            Mark(signal);
                        }
                    }
                }
            }
        }  

        private void setUserPoisitonSizes()
        {
            LotSize1 = UserLotSize1;
            LotSize2 = UserLotSize2;
            LotSize3 = UserLotSize3;
        }

        private void setSmallPositionSizes()
        {
            LotSize1 = 1;
            LotSize2 = 1;
            LotSize3 = 1;
        }

        private string calculateSignal()
        {
            string signalType = "No Signal";
            double recentSpanA = ApolloIchimoku(9, 26, 52, 26).SpanALine[26];
            double recentSpanB = ApolloIchimoku(9, 26, 52, 26).SpanBLine[26];
            double conversionLine = ApolloIchimoku(9, 26, 52, 26).ConversionLine[0];
            double baseline =  ApolloIchimoku(9, 26, 52, 26).BaseLine[0];
            string cloud = calculateCloud(recentSpanA, recentSpanB);

            if(conversionLine > baseline && cloud == "Green" && IsAroonUptrend())
            {
                signalType = "Strong Long";
            }else if(conversionLine > baseline && cloud =="Red" && IsAroonUptrend())
            {
                signalType = "Weak Long";
            }
            else if (conversionLine > baseline && cloud == "Green" && IsAroonDowntrend())
            {
                signalType = "Weak Long";
            }
            else if (conversionLine < baseline && cloud == "Green" && IsAroonUptrend())
            {
                signalType = "Weak Long";
            }
            else if (conversionLine < baseline && cloud == "Red" && IsAroonDowntrend())
            {
                signalType = "Strong Short";
            }
            else if (conversionLine < baseline && cloud == "Red" && IsAroonUptrend())
            {
                signalType = "Weak Short";
            }
            else if (conversionLine < baseline && cloud == "Green" && IsAroonDowntrend())
            {
                signalType = "Weak Short";
            }
            else if (conversionLine > baseline && cloud == "Red" && IsAroonDowntrend())
            {
                signalType = "Weak Short";
            }


            return signalType;

            /*When conversion line above base line and cloud green +Aroon green = strong up trend
            When conversion line above base line and cloud red+Aroon green = weak up trend
            When conversion line above base line and cloud green+Aroon red = weak up trend
            When conversion line below base line and cloud green+Aroon green = weak up trend

            ------------------------------------------------------

            When conversion line below base line and cloud red+Aroon red = strong down trend
            When conversion line below base line and cloud red+Aroon green = week down trend

            When conversion line below base line and cloud green+Aroon red = week down trend

            When conversion line above base line and cloud red+Aroon red = week down trend */
        }

        private string calculateCloud(double recentSpanA, double recentSpanB)
        {

            if (recentSpanA > recentSpanB)
            {
                return "Green";
            }
            else
            {
                return "Red";
            }
        }

        private void NoIchimokuMode()
            {
            if (UseLongs)
                {
                if (LongConditions())
                {
                    if (makeTrades)
                    {
                         _longOneOrder = EnterLong(LotSize1, "Long1");
                         _longTwoOrder = EnterLong(LotSize2, "Long2");
                         _longThreeOrder = EnterLong(LotSize3, "Long3");
                    }
                    else
                    {
                           Mark("Long");
                    }
                }
            }
            if (UseShorts)
             {
                    if (ShortConditions())
                    {
                        if (makeTrades)
                        {
                            _shortOneOrder = EnterShort(LotSize1, "Short1");
                            _shortTwoOrder = EnterShort(LotSize2, "Short2");
                            _shortThreeOrder = EnterShort(LotSize3, "Short3");
                        }
                        else
                        {
                            Mark("Short");
                        }
                    }
            }
        }

    #region Entry Positions

    private bool LongConditions()
    {
    return noPositions() && previousCandleGreen() && IsAroonUptrend() && stochRsiEntry(RsiEntryLong, "Long"); 
    }


    private bool ShortConditions()
    {
    return noPositions() && previousCandleRed() && IsAroonDowntrend() && stochRsiEntry(RsiEntryShort, "Short");
    }


    #endregion  

private bool previousCandleRed()
{
return HeikenAshi8().HAOpen[0] > HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] < HeikenAshi8().HAClose[1];
}

private bool previousCandleGreen()
{
return HeikenAshi8().HAOpen[0] < HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] > HeikenAshi8().HAClose[1];
}

private void Mark(string positionType)
{
    int _nr = rnd.Next();
    string rando = Convert.ToString(_nr);
    string name = "tag " + rando;
    string name2 = "tag2 " + rando;
    if (positionType == "Short")
    {
        Draw.ArrowDown(this, name, true, 0, High[0] + 4 * TickSize, Brushes.Red);
    }
    else if (positionType == "Extra Short")
    {
        Draw.ArrowDown(this, name, true, 0, High[0] + 4 * TickSize, Brushes.Yellow);
    }
    else if (positionType == "Long")
    {
        Draw.ArrowUp(this, name, true, 0, Low[0] - 4 * TickSize, Brushes.Blue);
    }
    else if (positionType == "Extra Long")
    {
        Draw.ArrowUp(this, name, true, 0, Low[0] -  4 * TickSize, Brushes.Yellow);
    }
    else if (positionType == "Strong Short")
    {
        Draw.ArrowDown(this, name, true, 0, High[0] + TickSize, Brushes.Red);
        Draw.Text(this, name2, "SS", 0, High[0] + 12);
    }

       else if (positionType == "Weak Short")
    {
       Draw.ArrowDown(this, name, true, 0, High[0] + TickSize, Brushes.Yellow);
       Draw.Text(this, name2, "WS",0, High[0] + 12);
    }
        else if (positionType == "Strong Long")
    {
        Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Blue);
        Draw.Text(this, name2, "SLONG", 0, Low[0] - 12 * TickSize);
    }

       else if (positionType == "Weak Long")
    {
       Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Yellow);
       Draw.Text(this, name2, "WL",0, Low[0] - 12 * TickSize);
    }

}


private bool IsAroonUptrend()
{
if (UseAroon)
{
    return Aroon(AroonPeriod).Up[0] > Aroon(AroonPeriod).Down[0];
}
else
{
    return true;
}

}

private bool IsAroonDowntrend()
{
if (UseAroon)
{
    return Aroon(AroonPeriod).Up[0] < Aroon(AroonPeriod).Down[0];
}
else
{
    return true;
}

}


private bool noPositions()
{
return Position.MarketPosition == MarketPosition.Flat;
}

private bool stochRsiEntry(int entryValue, string positionType)
{
    if (UseRsi)
    {
        _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
        if (positionType == "Long")
        {
            return _stochFast <= entryValue;
        }
        else if (positionType == "Short")
        {
            return _stochFast >= entryValue;
        }
        else
        {
            return false;
        }
    }
    else
    {
        return true;
    }

}

protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
{
if (OrderFilled(order) && IsLongOrder1(order))
{
    _longEntryPrice1 = averageFillPrice;
    _stopLossBaseLong = calculateStopLong();
    SetStopLoss("Long1", CalculationMode.Price, _stopLossBaseLong, false);
    SetProfitTarget("Long1", CalculationMode.Ticks, ProfitTargetLong1);
    status = "Long1";
}
else if (OrderFilled(order) && IsLongOrder2(order))
{
    SetStopLoss("Long2", CalculationMode.Price, _stopLossBaseLong, false);
    SetProfitTarget("Long2", CalculationMode.Ticks, ProfitTargetLong2);
}
else if (OrderFilled(order) && IsLongOrder3(order))
{
    SetStopLoss("Long3", CalculationMode.Price, _stopLossBaseLong, false);
    SetProfitTarget("Long3", CalculationMode.Ticks, ProfitTargetLong3);
}

else if (OrderFilled(order) && IsShortOrder1(order))
{
    _shortEntryPrice1 = averageFillPrice;
    _stopLossBaseShort = calculateStopShort();
    SetStopLoss("Short1", CalculationMode.Price, _stopLossBaseShort, false);
    SetProfitTarget("Short1", CalculationMode.Ticks, ProfitTargetShort1);
    status = "Short 1 Default";
}
else if (OrderFilled(order) && IsShortOrder2(order))
{
    SetStopLoss("Short2", CalculationMode.Price, _stopLossBaseShort, false);
    SetProfitTarget("Short2", CalculationMode.Ticks, ProfitTargetShort2);
}
else if (OrderFilled(order) && IsShortOrder3(order))
{
    SetStopLoss("Short3", CalculationMode.Price, _stopLossBaseShort, false);
    SetProfitTarget("Short3", CalculationMode.Ticks, ProfitTargetShort3);
}

}

/*
* 
* 
* Calculate the Conversion Line and the Base Line.
Calculate Leading Span A based on the prior calculations. Once calculated, this data point is plotted 26 periods into the future.
Calculate Leading Span B. Plot this data point 26 periods into the future.
For the Lagging Span, plot the closing price 26 periods into the past on the chart.
The difference between Leading Span A and Leading Span B is colored in to create the cloud.
When Leading Span A is above Leading Span B, color the cloud green. When Leading Span A is below Leading Span B, color the cloud red.
The above steps will create one data point. To create the lines, as each period comes to an end, go through the steps again to create new data points for that period. Connect the data points to each other to create the lines and cloud appearance.

*/

            #region tradeTime
            private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= 152900 && ToTime(Time[0]) < 210000))
            {
                _canTrade = true;
            }
            else
            {
                _canTrade = false;
            }
        }

        #endregion

        #region Orders Conditions

        private bool IsLongOrder1(Order order)
        {
            return order == _longOneOrder;
        }

        private bool IsLongOrder2(Order order)
        {
            return order == _longTwoOrder;
        }

        private bool IsLongOrder3(Order order)
        {
            return order == _longThreeOrder;
        }


        private bool IsShortOrder1(Order order)
        {
            return order == _shortOneOrder;
        }

        private bool IsShortOrder2(Order order)
        {
            return order == _shortTwoOrder;
        }

        private bool IsShortOrder3(Order order)
        {
            return order == _shortThreeOrder;
        }


        private bool OrderFilled(Order order)
        {
            return order.OrderState == OrderState.Filled;
        }

        #endregion

        #region Stop Calculation
        private double calculateStopLong()
        {
            List<double> lows = new List<double> { Low[0], Low[1], Low[2], Low[3], Low[4] };
            lows.Sort();
            double lowestLow = lows[0];
            double baseStopLoss = lowestLow - BaseStopMarginLong * TickSize;

            return baseStopLoss;
        }

        private double calculateStopShort()
        {
            List<double> highs = new List<double> { High[0], High[1], High[2], High[3], High[4] };
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];
            double baseStopLoss = highestHigh + BaseStopMarginShort * TickSize;

            return baseStopLoss;
        }
        #endregion

        private void ShowInfo()
        {
            double recentSpanA = ApolloIchimoku(9, 26, 52, 26).SpanALine[26];
            double recentSpanB = ApolloIchimoku(9, 26, 52, 26).SpanBLine[26];
            Print(Time[0]);
            Print("Price:");
            Print(Close[0]);
            Print("ICHIMOKU");
            Print("**************");
            Print("ConversionLine");
            Print(ApolloIchimoku(9, 26, 52, 26).ConversionLine[0]);
            Print("BaseLine");
            Print(ApolloIchimoku(9,26,52,26).BaseLine[0]);
            Print("SpanALine");
            Print(recentSpanA);
            Print("SpanBLine");
            Print(recentSpanB);

            if(recentSpanA> recentSpanB)
            {
                Print("Cloud is Green");
            }else if(recentSpanB> recentSpanA)
            { 
                Print("Cloud is Red");
            }
            Print("~~~~~~~~~~~~~~~~~~~~");
        }

        private void ShowEntryDetails(string signal)
        {
            Print("~~~~~~~~~~~~~~~~~~~~~~~~");
            Print("ENTRY DETAILS:");
            Print(signal);
            Print(Time[0]);
            Print("RSI ENTRY VALUE:");
            Print(StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1]);
            Print("~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        private void addIndicators()
        {
            _ha = HeikenAshi8();
            _aroon = Aroon(AroonPeriod);
            _stoch = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack);
            _iczimoku = ApolloIchimoku(9,26,52,26);
            if (ShowHA)
            {
                AddChartIndicator(_ha);
            }
            if (UseRsi)
            {
                AddChartIndicator(_stoch);
            }
            if (UseAroon)
            {
                AddChartIndicator(_aroon);
            }
            if(UseIchimoku)
            {
                AddChartIndicator(_iczimoku);
            }





            //       AddChartIndicator(IchimokuCloud());
        }
    }
}
