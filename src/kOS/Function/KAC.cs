﻿using System;
using System.Linq;
using kOS.Safe.Encapsulation;
using kOS.Safe.Function;
using kOS.Safe.Utilities;
using kOS.Safe.Persistence;
using kOS.Suffixed;
using kOS.Utilities;
using kOS_KACWrapper;

namespace kOS.Function
{
    [Function("addAlarm")]
    public class FunctionAddAlarm : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            string alarmNotes = shared.Cpu.PopValue().ToString();
            string alarmName = shared.Cpu.PopValue().ToString();
            double alarmUT = GetDouble(shared.Cpu.PopValue());

            if (KACWrapper.APIReady)
            {
                String aID = KACWrapper.KAC.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.Raw, alarmName, alarmUT);

                SafeHouse.Logger.Log (string.Format ("Trying to create KAC Alarm, UT={0}, Name={1}", alarmUT.ToString (), alarmName));

                if (aID !="") 
                {
                    //if the alarm was made get the object so we can update it
                    KACWrapper.KACAPI.KACAlarm a = KACWrapper.KAC.Alarms.First(z=>z.ID==aID);

                    //Now update some of the other properties
                    a.Notes = alarmNotes;
                    a.AlarmAction = KACWrapper.KACAPI.AlarmActionEnum.PauseGame;
                    a.VesselID = shared.Vessel.id.ToString();

                    KACAlarmWrapper result = new KACAlarmWrapper (a);

                    shared.Cpu.PushStack(result);
                }
                else
                {
                    //failed creating node
                    shared.Cpu.PushStack("");
                }

            }
            else
            {
                //KAC integration not present.
                shared.Cpu.PushStack("");
            }
        }
    }

    [Function("listAlarms")]
    public class FunctionListAlarms : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            var list = new ListValue();

            string alarmTypes = shared.Cpu.PopValue().ToString();

            if (KACWrapper.APIReady) 
            {
                //Get the list of alarms from the KAC Object
                KACWrapper.KACAPI.KACAlarmList alarms = KACWrapper.KAC.Alarms;

                foreach (KACWrapper.KACAPI.KACAlarm a in alarms) 
                {
                    list.Add (new KACAlarmWrapper(a));
                }
            }
            shared.Cpu.PushStack(list);
        }
    }
    
    [Function("deleteAlarm")]
    public class FunctionDeleteAlarm : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            string alarmID = shared.Cpu.PopValue().ToString();
            Boolean result = false;

            if (KACWrapper.APIReady)
            {
                //Delete the Alarm using its ID and get the result
                result = KACWrapper.KAC.DeleteAlarm(alarmID);

            }

            shared.Cpu.PushStack(result);
        }
    }
}
