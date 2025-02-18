using GData.Spreadsheets;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;
using static NOBApp.MainWindow;

namespace NOBApp.GoogleData
{
    internal class GoogleSheet
    {

        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static String spreadsheetId = "1oc2j5S41K2AB9Ny-qPd0E-g89VCNpY6PWn74p1DU6QM";
        static string sheetName = "AC";

        static SheetsService? Service { get; set; }
        static SpreadsheetsResource.ValuesResource? _googleSheetValues;
        const string APPLICATION_NAME = "MyNOBDATA";

        static string GDKey => "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";

        static public void GoogleSheetInit()
        {
            var dStr = Encoder.AesDecrypt(GDKey);
            var credential = GoogleCredential.FromJson(dStr).CreateScoped(Scopes);

            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = APPLICATION_NAME
            });
            _googleSheetValues = Service.Spreadsheets.Values;
            Get();
        }

        public static void Get()
        {
            if (_googleSheetValues != null)
            {
                var range = $"Get!A:C";
                var request = _googleSheetValues.Get(spreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;
                if (values != null && values.Count > 0)
                {
                    var v = ItemsMapper.MapFromRangeData_UseTime(values);
                }
            }
        }

        public static void CheckDonate(NOBDATA nob)
        {
            if (_googleSheetValues != null)
            {
                var range = $"Get!A:D";
                var request = _googleSheetValues.Get(spreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;
                if (values != null && values.Count > 0)
                {
                    var v = ItemsMapper.MapFromRangeData_UseTime(values);
                    if (v.Count > 0)
                    {
                        var p = v.Find(v => v.FromName.Contains(nob.Account));
                        if (p != null)
                        {
                            PNobUserData cvtryTime = new PNobUserData();
                            cvtryTime.Acc = nob.Account;

                            if (DateTime.TryParse(p.EndTimer, out 到期日) && p.ISEND.Contains("NO"))
                            {
                                nob.贊助者 = true;
                                cvtryTime.StartTimer = 到期日.ToString();
                                MainWindow.儲存認證訊息(cvtryTime);
                            }
                            if (p.CheckC.Contains("1"))
                            {
                                nob.特殊者 = true;
                                cvtryTime.StartTimer = 到期日.ToString();
                                MainWindow.儲存認證訊息(cvtryTime);
                            }
                        }
                        else
                        {
                            CheckTryTime(nob);
                        }
                    }
                }
            }
        }

        public static void CheckTryTime(NOBDATA nob)
        {
            if (_googleSheetValues != null)
            {
                var range = $"ACC!A:C";
                var request = _googleSheetValues.Get(spreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;
                if (values != null && values.Count > 0)
                {
                    var v = ItemsMapper.MapFromRangeData_TryUseTime(values);
                    if (v.Count > 0)
                    {
                        var p = v.Find(v => v.Acc!.Contains(nob.Account));
                        if (p != null)
                        {
                            if (DateTime.TryParse(p.StartTimer, out 到期日) && p.ISEND!.Contains("NO"))
                            {
                                p.CheckC = "1";
                                nob.特殊者 = true;

                            }
                            MainWindow.儲存認證訊息(p);
                        }
                    }
                }
            }
        }

        static public void Get(int rowId)
        {
            if (_googleSheetValues != null)
            {
                var range = $"{sheetName}!A{rowId}:D{rowId}";
                var request = _googleSheetValues.Get(spreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;
            }
        }

        static public void Post(FPDATA item)
        {
            if (_googleSheetValues != null)
            {
                var range = $"AC!A:E";
                var valueRange = new ValueRange
                {
                    MajorDimension = "ROWS",
                    Values = ItemsMapper.MapToRangeData(item)
                };

                if (valueRange != null)
                {
                    foreach (var i in ItemsMapper.MapFromRangeData(valueRange.Values))
                    {
                        Debug.WriteLine($"{i.UserName}, {i.Acc}, {i.Pww}, {i.SerialNumber}, {i.LoginTimer}");
                    }
                    var appendRequest = _googleSheetValues.Append(valueRange, spreadsheetId, range);
                    appendRequest.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
                    appendRequest.Execute();
                }

            }
        }

        static public void Post(List<FPDATA> items)
        {
            if (_googleSheetValues != null)
            {
                foreach (var item in items)
                {
                    Post(item);
                }
            }
        }

        public void Put(int rowId, FPDATA item)
        {
            if (_googleSheetValues != null)
            {
                var range = $"{sheetName}!A{rowId}:D{rowId}";
                var valueRange = new ValueRange
                {
                    Values = ItemsMapper.MapToRangeData(item)
                };
                var updateRequest = _googleSheetValues.Update(valueRange, spreadsheetId, range);
                updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
                updateRequest.Execute();
            }
        }
    }

    public class UseTime
    {
        public string FromName { get; set; }
        public string StartTimer { get; set; }
        public string EndTimer { get; set; }
        public string ISEND { get; set; }
        public string CheckC { get; set; }
    }

    public class PNobUserData
    {
        public string? Acc { get; set; }
        public string? StartTimer { get; set; }
        public string? ISEND { get; set; }
        public string? CheckC { get; set; }
        public override string ToString()
        {
            return $"{Acc} {StartTimer} {ISEND}";
        }
    }

    public class FPDATA
    {
        public string UserName { get; set; }
        public string Acc { get; set; }
        public string Pww { get; set; }
        public string SerialNumber { get; set; }
        public string LoginTimer { get; set; }
    }
    public static class ItemsMapper
    {
        public static List<UseTime> MapFromRangeData_UseTime(IList<IList<object>> values)
        {
            var items = new List<UseTime>();
            foreach (var value in values)
            {
                UseTime item = new()
                {
                    FromName = value[0].ToString(),
                    StartTimer = value[1].ToString(),
                    EndTimer = value[2].ToString(),
                    ISEND = value.Count > 3 ? value[3].ToString() : "YES",
                    CheckC = value.Count > 4 ? value[4].ToString() : "0",
                };
                items.Add(item);
            }
            return items;
        }

        public static List<PNobUserData> MapFromRangeData_TryUseTime(IList<IList<object>> values)
        {
            var items = new List<PNobUserData>();
            foreach (var value in values)
            {
                PNobUserData item = new()
                {
                    Acc = value[0].ToString(),
                    StartTimer = value[1].ToString(),
                    ISEND = value.Count > 2 ? value[2].ToString() : "YES",
                };
                items.Add(item);
            }
            return items;
        }

        public static List<FPDATA> MapFromRangeData(IList<IList<object>> values)
        {
            var items = new List<FPDATA>();
            foreach (var value in values)
            {
                FPDATA item = new()
                {
                    UserName = value[0].ToString(),
                    Acc = value[1].ToString(),
                    Pww = value[2].ToString(),
                    SerialNumber = value[3].ToString(),
                    LoginTimer = value[4].ToString(),
                };
                items.Add(item);
            }
            return items;
        }
        public static IList<IList<object>> MapToRangeData(FPDATA i)
        {
            var objectList = new List<object>() { i.UserName, i.Acc, i.Pww, i.SerialNumber, i.LoginTimer };
            var rangeData = new List<IList<object>> { objectList };
            return rangeData;
        }

        public static IList<IList<object>> MapToRangeData(List<FPDATA> item)
        {
            var objectList = new List<object>() { };
            foreach (var i in item)
            {
                objectList.Add(new List<object>() { i.UserName, i.Acc, i.Pww, i.SerialNumber, i.LoginTimer });
            }

            var rangeData = new List<IList<object>> { objectList };
            return rangeData;
        }
    }
}
