﻿using LambdaUI.Utilities;

namespace LambdaUI.Models.Simply
{
    internal class JustJumpMapTimeModel
    {
        public int UniqueId { get; set; }
        public string SteamId { get; set; }
        public string Map { get; set; }
        public string Name { get; set; }
        public double RunTime { get; set; }
        public int Class { get; set; }
        public double TimeStamp { get; set; }

        public override string ToString() =>
            $"{SimplyHelper.ClassToShortString(Class)} | {Name} | {TempusHelper.TimeSpanToFormattedTime(SimplyHelper.GetTimeSpan(RunTime))}";
    }
}