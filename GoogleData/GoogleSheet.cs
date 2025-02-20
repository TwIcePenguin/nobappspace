using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace NOBApp.GoogleData
{
    internal class GoogleSheet
    {
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string SpreadsheetId = "1oc2j5S41K2AB9Ny-qPd0E-g89VCNpY6PWn74p1DU6QM";
        static string SheetName = "AC";

        static SheetsService? Service { get; set; }
        static SpreadsheetsResource.ValuesResource? GoogleSheetValues;
        const string ApplicationName = "MyNOBDATA";

        static string GDKey => "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";

        static public void GoogleSheetInit()
        {
            var decryptedKey = Encoder.AesDecrypt(GDKey);
            var credential = GoogleCredential.FromJson(decryptedKey).CreateScoped(Scopes);

            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
            GoogleSheetValues = Service.Spreadsheets.Values;
            Get();
        }

        public static void Get()
        {
            if (GoogleSheetValues != null)
            {
                var range = $"Get!A:C";
                var request = GoogleSheetValues.Get(SpreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;
                if (values != null && values.Count > 0)
                {
                    var mappedValues = ItemsMapper.MapFromRangeData_UseTime(values);
                }
            }
        }

        public static void CheckDonate(NOBDATA nobData)
        {
            if (GoogleSheetValues != null)
            {
                var range = $"Get!A:D";
                var request = GoogleSheetValues.Get(SpreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;
                if (values != null && values.Count > 0)
                {
                    var mappedValues = ItemsMapper.MapFromRangeData_UseTime(values);
                    if (mappedValues.Count > 0)
                    {
                        var foundValue = mappedValues.Find(v => v.FromName.Contains(nobData.Account));
                        if (foundValue != null)
                        {
                            PNobUserData userData = new PNobUserData();
                            userData.Acc = nobData.Account;

                            if (DateTime.TryParse(foundValue.EndTimer, out DateTime expirationDate) && foundValue.ISEND.Contains("NO"))
                            {
                                nobData.贊助者 = true;
                                userData.StartTimer = expirationDate.ToString();
                                Authentication.儲存認證訊息(userData);
                            }
                            if (foundValue.CheckC.Contains("1"))
                            {
                                nobData.特殊者 = true;
                                userData.StartTimer = expirationDate.ToString();
                                Authentication.儲存認證訊息(userData);
                            }
                        }
                        else
                        {
                            CheckTryTime(nobData);
                        }
                    }
                }
            }
        }

        public static void CheckTryTime(NOBDATA nobData)
        {
            if (GoogleSheetValues != null)
            {
                var range = $"ACC!A:C";
                var request = GoogleSheetValues.Get(SpreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;
                if (values != null && values.Count > 0)
                {
                    var mappedValues = ItemsMapper.MapFromRangeData_TryUseTime(values);
                    if (mappedValues.Count > 0)
                    {
                        var foundValue = mappedValues.Find(v => v.Acc!.Contains(nobData.Account));
                        if (foundValue != null)
                        {
                            if (DateTime.TryParse(foundValue.StartTimer, out DateTime expirationDate) && foundValue.ISEND!.Contains("NO"))
                            {
                                foundValue.CheckC = "1";
                                nobData.特殊者 = true;
                            }
                            Authentication.儲存認證訊息(foundValue);
                        }
                    }
                }
            }
        }

        static public void Get(int rowId)
        {
            if (GoogleSheetValues != null)
            {
                var range = $"{SheetName}!A{rowId}:D{rowId}";
                var request = GoogleSheetValues.Get(SpreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;
            }
        }

        static public void Post(FPDATA dataItem)
        {
            if (GoogleSheetValues != null)
            {
                var range = $"AC!A:E";
                var valueRange = new ValueRange
                {
                    MajorDimension = "ROWS",
                    Values = ItemsMapper.MapToRangeData(dataItem)
                };

                if (valueRange != null)
                {
                    foreach (var item in ItemsMapper.MapFromRangeData(valueRange.Values))
                    {
                        Debug.WriteLine($"{item.UserName}, {item.Acc}, {item.Pww}, {item.SerialNumber}, {item.LoginTimer}");
                    }
                    var appendRequest = GoogleSheetValues.Append(valueRange, SpreadsheetId, range);
                    appendRequest.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
                    appendRequest.Execute();
                }
            }
        }

        static public void Post(List<FPDATA> dataItems)
        {
            if (GoogleSheetValues != null)
            {
                foreach (var dataItem in dataItems)
                {
                    Post(dataItem);
                }
            }
        }

        public void Put(int rowId, FPDATA dataItem)
        {
            if (GoogleSheetValues != null)
            {
                var range = $"{SheetName}!A{rowId}:D{rowId}";
                var valueRange = new ValueRange
                {
                    Values = ItemsMapper.MapToRangeData(dataItem)
                };
                var updateRequest = GoogleSheetValues.Update(valueRange, SpreadsheetId, range);
                updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
                updateRequest.Execute();
            }
        }
    }

    public class UseTime
    {
        public string FromName { get; set; } = string.Empty;
        public string StartTimer { get; set; } = string.Empty;
        public string EndTimer { get; set; } = string.Empty;
        public string ISEND { get; set; } = string.Empty;
        public string CheckC { get; set; } = string.Empty;
    }

    public class FPDATA
    {
        public string UserName { get; set; } = string.Empty;
        public string Acc { get; set; } = string.Empty;
        public string Pww { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string LoginTimer { get; set; } = string.Empty;
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
                    FromName = value[0]?.ToString() ?? string.Empty,
                    StartTimer = value[1]?.ToString() ?? string.Empty,
                    EndTimer = value[2]?.ToString() ?? string.Empty,
                    ISEND = value.Count > 3 ? value[3]?.ToString() ?? "YES" : "YES",
                    CheckC = value.Count > 4 ? value[4]?.ToString() ?? "0" : "0",
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
                    UserName = value[0]?.ToString() ?? string.Empty,
                    Acc = value[1]?.ToString() ?? string.Empty,
                    Pww = value[2]?.ToString() ?? string.Empty,
                    SerialNumber = value[3]?.ToString() ?? string.Empty,
                    LoginTimer = value[4]?.ToString() ?? string.Empty,
                };
                items.Add(item);
            }
            return items;
        }

        public static IList<IList<object>> MapToRangeData(FPDATA dataItem)
        {
            var objectList = new List<object>() { dataItem.UserName, dataItem.Acc, dataItem.Pww, dataItem.SerialNumber, dataItem.LoginTimer };
            var rangeData = new List<IList<object>> { objectList };
            return rangeData;
        }

        public static IList<IList<object>> MapToRangeData(List<FPDATA> dataItems)
        {
            var objectList = new List<object>() { };
            foreach (var dataItem in dataItems)
            {
                objectList.Add(new List<object>() { dataItem.UserName, dataItem.Acc, dataItem.Pww, dataItem.SerialNumber, dataItem.LoginTimer });
            }

            var rangeData = new List<IList<object>> { objectList };
            return rangeData;
        }
    }
}
