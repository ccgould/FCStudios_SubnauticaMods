using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCS_AlterraHub.Core.Services;

public static class NotificationService
{
    //public static Action<TechType,TechType,int> OnAddToCartClicked;
    public static void CSVLog(Component component1, string value = null)
    {
        _logs.Add(new FCSLog(component1.name,component1.GetType(),value));

        String separator = ",";
        String[] headings = { "Date/Time", "Sender Type","Sender Name", "Value" };

        using (TextWriter sw = new StreamWriter(Path.Combine(FileSystemHelper.ModDirLocation, "Log.csv")))
        {
            sw.WriteLine(string.Join(separator, headings));
            foreach (FCSLog log in _logs)
            {
                String[] newLine = { log.DateTimeString, log.SenderType,log.SenderName, log.Value};
                sw.WriteLine(string.Join(separator, newLine));
            }
        }


        //using (var writer = new StreamWriter(Path.Combine(FileSystemHelper.ModDirLocation, "Log.csv")))
        //using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        //{
        //    csv.WriteRecords(_logs);
        //}
    }

    private static List<FCSLog> _logs = new();

}
