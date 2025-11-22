using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RegisterDmSoftConsoleApp.DmSoft
{
    public class DmSoftCustomClassName
    {
        private Type? obj = null;
        private object? obj_object = null;
        public bool IsAvailable { get; private set; }
        public string? InitError { get; private set; }

        public DmSoftCustomClassName()
        {
            try
            {
                obj = Type.GetTypeFromProgID("dm.dmsoft");
                if (obj == null)
                {
                    InitError = "ProgID dm.dmsoft 未註冊或載入失敗";
                    LogInit("ProgID not found");
                    IsAvailable = false;
                    return;
                }
                obj_object = Activator.CreateInstance(obj);
                IsAvailable = obj_object != null;
                if (!IsAvailable)
                {
                    InitError = "無法建立 COM 實例";
                    LogInit("CreateInstance returned null");
                }
            }
            catch (Exception ex)
            {
                InitError = ex.Message;
                LogInit("Exception: " + ex);
                IsAvailable = false;
            }
        }

        private void LogInit(string msg)
        {
            try
            {
                File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "dm_init.log"),
 $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\n");
            }
            catch { }
        }

        public void ReleaseObj()
        {
            if (obj_object != null)
            {
                Marshal.ReleaseComObject(obj_object);
                obj_object = null;
            }
        }

        ~DmSoftCustomClassName()
        {
            ReleaseObj();
        }

        public int SetRowGapNoDict(int row_gap)
        {
            object[] args = new object[1];
            object result;
            args[0] = row_gap;

            result = obj!.InvokeMember("SetRowGapNoDict", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string FindStrEx(int x1,int y1,int x2,int y2,string str,string color,double sim)
        {
            object[] args = new object[7];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = str;
            args[5] = color;
            args[6] = sim;

            result = obj.InvokeMember("FindStrEx", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string FindStrE(int x1,int y1,int x2,int y2,string str,string color,double sim)
        {
            object[] args = new object[7];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = str;
            args[5] = color;
            args[6] = sim;

            result = obj.InvokeMember("FindStrE", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string DisAssemble(string asm_code,long base_addr,int is_64bit)
        {
            object[] args = new object[3];
            object result;
            args[0] = asm_code;
            args[1] = base_addr;
            args[2] = is_64bit;

            result = obj.InvokeMember("DisAssemble", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string FindPicExS(int x1,int y1,int x2,int y2,string pic_name,string delta_color,double sim,int dir)
        {
            object[] args = new object[8];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = pic_name;
            args[5] = delta_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindPicExS", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string OcrEx(int x1,int y1,int x2,int y2,string color,double sim)
        {
            object[] args = new object[6];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = color;
            args[5] = sim;

            result = obj.InvokeMember("OcrEx", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string GetColor(int x,int y)
        {
            object[] args = new object[2];
            object result;
            args[0] = x;
            args[1] = y;

            result = obj.InvokeMember("GetColor", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int EnableFindPicMultithread(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("EnableFindPicMultithread", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetEnv(int index,string name,string value)
        {
            object[] args = new object[3];
            object result;
            args[0] = index;
            args[1] = name;
            args[2] = value;

            result = obj.InvokeMember("SetEnv", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FindPic(int x1,int y1,int x2,int y2,string pic_name,string delta_color,double sim,int dir,out int x,out int y)
        {
            object[] args = new object[10];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(10);
            mods[0][8] = true;
            mods[0][9] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = pic_name;
            args[5] = delta_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindPic", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[8];
            y = (int)args[9];
            return (int)result;
        }

        public int SetMinColGap(int col_gap)
        {
            object[] args = new object[1];
            object result;
            args[0] = col_gap;

            result = obj.InvokeMember("SetMinColGap", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string StringToData(string string_value,int tpe)
        {
            object[] args = new object[2];
            object result;
            args[0] = string_value;
            args[1] = tpe;

            result = obj.InvokeMember("StringToData", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string GetCommandLine(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("GetCommandLine", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int MiddleUp()
        {
            object result;
            result = obj.InvokeMember("MiddleUp", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int EnableFontSmooth()
        {
            object result;
            result = obj.InvokeMember("EnableFontSmooth", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int FreePic(string pic_name)
        {
            object[] args = new object[1];
            object result;
            args[0] = pic_name;

            result = obj.InvokeMember("FreePic", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetDisplayAcceler(int level)
        {
            object[] args = new object[1];
            object result;
            args[0] = level;

            result = obj.InvokeMember("SetDisplayAcceler", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string Ocr(int x1,int y1,int x2,int y2,string color,double sim)
        {
            object[] args = new object[6];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = color;
            args[5] = sim;

            result = obj.InvokeMember("Ocr", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string GetWindowTitle(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("GetWindowTitle", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int LockDisplay(int locks)
        {
            object[] args = new object[1];
            object result;
            args[0] = locks;

            result = obj.InvokeMember("LockDisplay", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetPath(string path)
        {
            // 防止未註冊時 obj 為 null造成崩潰
            if (obj == null || obj_object == null)
            {
                LogInit("SetPath skipped, COM not available");
                return -1;
            }
            object[] args = new object[1];
            object result;
            args[0] = path;

            result = obj.InvokeMember("SetPath", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string BGR2RGB(string bgr_color)
        {
            object[] args = new object[1];
            object result;
            args[0] = bgr_color;

            result = obj.InvokeMember("BGR2RGB", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int UnLoadDriver()
        {
            object result;
            result = obj.InvokeMember("UnLoadDriver", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int GetOsType()
        {
            object result;
            result = obj.InvokeMember("GetOsType", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int SetEnumWindowDelay(int delay)
        {
            object[] args = new object[1];
            object result;
            args[0] = delay;

            result = obj.InvokeMember("SetEnumWindowDelay", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FindMulColor(int x1,int y1,int x2,int y2,string color,double sim)
        {
            object[] args = new object[6];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = color;
            args[5] = sim;

            result = obj.InvokeMember("FindMulColor", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string ReadDataAddr(int hwnd,long addr,int length)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = length;

            result = obj.InvokeMember("ReadDataAddr", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int SetKeypadDelay(string tpe,int delay)
        {
            object[] args = new object[2];
            object result;
            args[0] = tpe;
            args[1] = delay;

            result = obj.InvokeMember("SetKeypadDelay", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetDiskModel(int index)
        {
            object[] args = new object[1];
            object result;
            args[0] = index;

            result = obj.InvokeMember("GetDiskModel", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string AppendPicAddr(string pic_info,int addr,int size)
        {
            object[] args = new object[3];
            object result;
            args[0] = pic_info;
            args[1] = addr;
            args[2] = size;

            result = obj.InvokeMember("AppendPicAddr", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int GetSpecialWindow(int flag)
        {
            object[] args = new object[1];
            object result;
            args[0] = flag;

            result = obj.InvokeMember("GetSpecialWindow", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int EnableMouseSync(int en,int time_out)
        {
            object[] args = new object[2];
            object result;
            args[0] = en;
            args[1] = time_out;

            result = obj.InvokeMember("EnableMouseSync", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int KeyUpChar(string key_str)
        {
            object[] args = new object[1];
            object result;
            args[0] = key_str;

            result = obj.InvokeMember("KeyUpChar", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FaqCapture(int x1,int y1,int x2,int y2,int quality,int delay,int time)
        {
            object[] args = new object[7];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = quality;
            args[5] = delay;
            args[6] = time;

            result = obj.InvokeMember("FaqCapture", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int IsFolderExist(string folder)
        {
            object[] args = new object[1];
            object result;
            args[0] = folder;

            result = obj.InvokeMember("IsFolderExist", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetCursorShape()
        {
            object result;
            result = obj.InvokeMember("GetCursorShape", BindingFlags.InvokeMethod, null, obj_object, null);
            return result.ToString();
        }

        public int EnableIme(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("EnableIme", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int EnableKeypadPatch(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("EnableKeypadPatch", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FoobarLock(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("FoobarLock", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string Assemble(long base_addr,int is_64bit)
        {
            object[] args = new object[2];
            object result;
            args[0] = base_addr;
            args[1] = is_64bit;

            result = obj.InvokeMember("Assemble", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string FindPicMemE(int x1,int y1,int x2,int y2,string pic_info,string delta_color,double sim,int dir)
        {
            object[] args = new object[8];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = pic_info;
            args[5] = delta_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindPicMemE", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string FloatToData(float float_value)
        {
            object[] args = new object[1];
            object result;
            args[0] = float_value;

            result = obj.InvokeMember("FloatToData", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int SetShowAsmErrorMsg(int show)
        {
            object[] args = new object[1];
            object result;
            args[0] = show;

            result = obj.InvokeMember("SetShowAsmErrorMsg", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FindPicMem(int x1,int y1,int x2,int y2,string pic_info,string delta_color,double sim,int dir,out int x,out int y)
        {
            object[] args = new object[10];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(10);
            mods[0][8] = true;
            mods[0][9] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = pic_info;
            args[5] = delta_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindPicMem", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[8];
            y = (int)args[9];
            return (int)result;
        }

        public string FindStrFastS(int x1,int y1,int x2,int y2,string str,string color,double sim,out int x,out int y)
        {
            object[] args = new object[9];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(9);
            mods[0][7] = true;
            mods[0][8] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = str;
            args[5] = color;
            args[6] = sim;

            result = obj.InvokeMember("FindStrFastS", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[7];
            y = (int)args[8];
            return result.ToString();
        }

        public int FoobarDrawLine(int hwnd,int x1,int y1,int x2,int y2,string color,int style,int width)
        {
            object[] args = new object[8];
            object result;
            args[0] = hwnd;
            args[1] = x1;
            args[2] = y1;
            args[3] = x2;
            args[4] = y2;
            args[5] = color;
            args[6] = style;
            args[7] = width;

            result = obj.InvokeMember("FoobarDrawLine", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int KeyDown(int vk)
        {
            object[] args = new object[1];
            object result;
            args[0] = vk;

            result = obj.InvokeMember("KeyDown", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetDiskReversion(int index)
        {
            object[] args = new object[1];
            object result;
            args[0] = index;

            result = obj.InvokeMember("GetDiskReversion", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string FindPicSimEx(int x1,int y1,int x2,int y2,string pic_name,string delta_color,int sim,int dir)
        {
            object[] args = new object[8];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = pic_name;
            args[5] = delta_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindPicSimEx", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string ReadFileData(string file_name,int start_pos,int end_pos)
        {
            object[] args = new object[3];
            object result;
            args[0] = file_name;
            args[1] = start_pos;
            args[2] = end_pos;

            result = obj.InvokeMember("ReadFileData", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string FindPicSimE(int x1,int y1,int x2,int y2,string pic_name,string delta_color,int sim,int dir)
        {
            object[] args = new object[8];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = pic_name;
            args[5] = delta_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindPicSimE", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string GetRealPath(string path)
        {
            object[] args = new object[1];
            object result;
            args[0] = path;

            result = obj.InvokeMember("GetRealPath", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int DmGuardLoadCustom(string tpe,string path)
        {
            object[] args = new object[2];
            object result;
            args[0] = tpe;
            args[1] = path;

            result = obj.InvokeMember("DmGuardLoadCustom", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetClipboard()
        {
            object result;
            result = obj.InvokeMember("GetClipboard", BindingFlags.InvokeMethod, null, obj_object, null);
            return result.ToString();
        }

        public int GetLastError()
        {
            object result;
            result = obj.InvokeMember("GetLastError", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int WaitKey(int key_code,int time_out)
        {
            object[] args = new object[2];
            object result;
            args[0] = key_code;
            args[1] = time_out;

            result = obj.InvokeMember("WaitKey", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int LeaveCri()
        {
            object result;
            result = obj.InvokeMember("LeaveCri", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int Play(string file_name)
        {
            object[] args = new object[1];
            object result;
            args[0] = file_name;

            result = obj.InvokeMember("Play", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetWordResultStr(string str,int index)
        {
            object[] args = new object[2];
            object result;
            args[0] = str;
            args[1] = index;

            result = obj.InvokeMember("GetWordResultStr", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int FoobarDrawPic(int hwnd,int x,int y,string pic,string trans_color)
        {
            object[] args = new object[5];
            object result;
            args[0] = hwnd;
            args[1] = x;
            args[2] = y;
            args[3] = pic;
            args[4] = trans_color;

            result = obj.InvokeMember("FoobarDrawPic", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int GetKeyState(int vk)
        {
            object[] args = new object[1];
            object result;
            args[0] = vk;

            result = obj.InvokeMember("GetKeyState", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int KeyDownChar(string key_str)
        {
            object[] args = new object[1];
            object result;
            args[0] = key_str;

            result = obj.InvokeMember("KeyDownChar", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int LoadPicByte(int addr,int size,string name)
        {
            object[] args = new object[3];
            object result;
            args[0] = addr;
            args[1] = size;
            args[2] = name;

            result = obj.InvokeMember("LoadPicByte", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int WheelUp()
        {
            object result;
            result = obj.InvokeMember("WheelUp", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int OpenProcess(int pid)
        {
            object[] args = new object[1];
            object result;
            args[0] = pid;

            result = obj.InvokeMember("OpenProcess", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int UseDict(int index)
        {
            object[] args = new object[1];
            object result;
            args[0] = index;

            result = obj.InvokeMember("UseDict", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int ReadDataAddrToBin(int hwnd,long addr,int length)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = length;

            result = obj.InvokeMember("ReadDataAddrToBin", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int IsDisplayDead(int x1,int y1,int x2,int y2,int t)
        {
            object[] args = new object[5];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = t;

            result = obj.InvokeMember("IsDisplayDead", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetUAC(int uac)
        {
            object[] args = new object[1];
            object result;
            args[0] = uac;

            result = obj.InvokeMember("SetUAC", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetPicSize(string pic_name)
        {
            object[] args = new object[1];
            object result;
            args[0] = pic_name;

            result = obj.InvokeMember("GetPicSize", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int WriteFile(string file_name,string content)
        {
            object[] args = new object[2];
            object result;
            args[0] = file_name;
            args[1] = content;

            result = obj.InvokeMember("WriteFile", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FoobarPrintText(int hwnd,string text,string color)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = text;
            args[2] = color;

            result = obj.InvokeMember("FoobarPrintText", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int WriteData(int hwnd,string addr,string data)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = data;

            result = obj.InvokeMember("WriteData", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string DoubleToData(double double_value)
        {
            object[] args = new object[1];
            object result;
            args[0] = double_value;

            result = obj.InvokeMember("DoubleToData", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int ImageToBmp(string pic_name,string bmp_name)
        {
            object[] args = new object[2];
            object result;
            args[0] = pic_name;
            args[1] = bmp_name;

            result = obj.InvokeMember("ImageToBmp", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int GetPointWindow(int x,int y)
        {
            object[] args = new object[2];
            object result;
            args[0] = x;
            args[1] = y;

            result = obj.InvokeMember("GetPointWindow", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public double ReadDouble(int hwnd,string addr)
        {
            object[] args = new object[2];
            object result;
            args[0] = hwnd;
            args[1] = addr;

            result = obj.InvokeMember("ReadDouble", BindingFlags.InvokeMethod, null, obj_object, args);
            return (double)result;
        }

        public int SendCommand(string cmd)
        {
            object[] args = new object[1];
            object result;
            args[0] = cmd;

            result = obj.InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int GetCursorPos(out int x,out int y)
        {
            object[] args = new object[2];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(2);
            mods[0][0] = true;
            mods[0][1] = true;


            result = obj.InvokeMember("GetCursorPos", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[0];
            y = (int)args[1];
            return (int)result;
        }

        public string ReadIniPwd(string section,string key,string file_name,string pwd)
        {
            object[] args = new object[4];
            object result;
            args[0] = section;
            args[1] = key;
            args[2] = file_name;
            args[3] = pwd;

            result = obj.InvokeMember("ReadIniPwd", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string FindDataEx(int hwnd,string addr_range,string data,int steps,int multi_thread,int mode)
        {
            object[] args = new object[6];
            object result;
            args[0] = hwnd;
            args[1] = addr_range;
            args[2] = data;
            args[3] = steps;
            args[4] = multi_thread;
            args[5] = mode;

            result = obj.InvokeMember("FindDataEx", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int WriteDataAddr(int hwnd,long addr,string data)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = data;

            result = obj.InvokeMember("WriteDataAddr", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string ReadData(int hwnd, string addr, int length)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = length;

            result = obj.InvokeMember("ReadData", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }


        public int SetWordLineHeightNoDict(int line_height)
        {
            object[] args = new object[1];
            object result;
            args[0] = line_height;

            result = obj.InvokeMember("SetWordLineHeightNoDict", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FaqCancel()
        {
            object result;
            result = obj.InvokeMember("FaqCancel", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int EnableShareDict(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("EnableShareDict", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetWordLineHeight(int line_height)
        {
            object[] args = new object[1];
            object result;
            args[0] = line_height;

            result = obj.InvokeMember("SetWordLineHeight", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int GetNowDict()
        {
            object result;
            result = obj.InvokeMember("GetNowDict", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int SendStringIme(string str)
        {
            object[] args = new object[1];
            object result;
            args[0] = str;

            result = obj.InvokeMember("SendStringIme", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int DeleteFolder(string folder_name)
        {
            object[] args = new object[1];
            object result;
            args[0] = folder_name;

            result = obj.InvokeMember("DeleteFolder", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int GetForegroundWindow()
        {
            object result;
            result = obj.InvokeMember("GetForegroundWindow", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int GetClientSize(int hwnd,out int width,out int height)
        {
            object[] args = new object[3];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(3);
            mods[0][1] = true;
            mods[0][2] = true;

            args[0] = hwnd;

            result = obj.InvokeMember("GetClientSize", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            width = (int)args[1];
            height = (int)args[2];
            return (int)result;
        }

        public int DelEnv(int index,string name)
        {
            object[] args = new object[2];
            object result;
            args[0] = index;
            args[1] = name;

            result = obj.InvokeMember("DelEnv", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int CreateFoobarRect(int hwnd,int x,int y,int w,int h)
        {
            object[] args = new object[5];
            object result;
            args[0] = hwnd;
            args[1] = x;
            args[2] = y;
            args[3] = w;
            args[4] = h;

            result = obj.InvokeMember("CreateFoobarRect", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetClientSize(int hwnd,int width,int height)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = width;
            args[2] = height;

            result = obj.InvokeMember("SetClientSize", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FindMultiColor(int x1,int y1,int x2,int y2,string first_color,string offset_color,double sim,int dir,out int x,out int y)
        {
            object[] args = new object[10];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(10);
            mods[0][8] = true;
            mods[0][9] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = first_color;
            args[5] = offset_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindMultiColor", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[8];
            y = (int)args[9];
            return (int)result;
        }

        public int delay(int mis)
        {
            object[] args = new object[1];
            object result;
            args[0] = mis;

            result = obj.InvokeMember("delay", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetDictMem(int index,int addr,int size)
        {
            object[] args = new object[3];
            object result;
            args[0] = index;
            args[1] = addr;
            args[2] = size;

            result = obj.InvokeMember("SetDictMem", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string FindMultiColorE(int x1,int y1,int x2,int y2,string first_color,string offset_color,double sim,int dir)
        {
            object[] args = new object[8];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = first_color;
            args[5] = offset_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindMultiColorE", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int GetTime()
        {
            object result;
            result = obj.InvokeMember("GetTime", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int SetDisplayInput(string mode)
        {
            object[] args = new object[1];
            object result;
            args[0] = mode;

            result = obj.InvokeMember("SetDisplayInput", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FindColor(int x1,int y1,int x2,int y2,string color,double sim,int dir,out int x,out int y)
        {
            object[] args = new object[9];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(9);
            mods[0][7] = true;
            mods[0][8] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = color;
            args[5] = sim;
            args[6] = dir;

            result = obj.InvokeMember("FindColor", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[7];
            y = (int)args[8];
            return (int)result;
        }

        public int SwitchBindWindow(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("SwitchBindWindow", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int RightUp()
        {
            object result;
            result = obj.InvokeMember("RightUp", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int FindStr(int x1,int y1,int x2,int y2,string str,string color,double sim,out int x,out int y)
        {
            object[] args = new object[9];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(9);
            mods[0][7] = true;
            mods[0][8] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = str;
            args[5] = color;
            args[6] = sim;

            result = obj.InvokeMember("FindStr", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[7];
            y = (int)args[8];
            return (int)result;
        }

        public int LeftClick()
        {
            object result;
            result = obj.InvokeMember("LeftClick", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int SendString(int hwnd,string str)
        {
            object[] args = new object[2];
            object result;
            args[0] = hwnd;
            args[1] = str;

            result = obj.InvokeMember("SendString", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetNetTime()
        {
            object result;
            result = obj.InvokeMember("GetNetTime", BindingFlags.InvokeMethod, null, obj_object, null);
            return result.ToString();
        }

        public int FoobarFillRect(int hwnd,int x1,int y1,int x2,int y2,string color)
        {
            object[] args = new object[6];
            object result;
            args[0] = hwnd;
            args[1] = x1;
            args[2] = y1;
            args[3] = x2;
            args[4] = y2;
            args[5] = color;

            result = obj.InvokeMember("FoobarFillRect", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetWindowState(int hwnd,int flag)
        {
            object[] args = new object[2];
            object result;
            args[0] = hwnd;
            args[1] = flag;

            result = obj.InvokeMember("SetWindowState", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int RegEx(string code,string Ver,string ip)
        {
            object[] args = new object[3];
            object result;
            args[0] = code;
            args[1] = Ver;
            args[2] = ip;

            result = obj.InvokeMember("RegEx", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int DmGuard(int en,string tpe)
        {
            object[] args = new object[2];
            object result;
            args[0] = en;
            args[1] = tpe;

            result = obj.InvokeMember("DmGuard", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetMouseSpeed(int speed)
        {
            object[] args = new object[1];
            object result;
            args[0] = speed;

            result = obj.InvokeMember("SetMouseSpeed", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int GetWindowRect(int hwnd,out int x1,out int y1,out int x2,out int y2)
        {
            object[] args = new object[5];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(5);
            mods[0][1] = true;
            mods[0][2] = true;
            mods[0][3] = true;
            mods[0][4] = true;

            args[0] = hwnd;

            result = obj.InvokeMember("GetWindowRect", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x1 = (int)args[1];
            y1 = (int)args[2];
            x2 = (int)args[3];
            y2 = (int)args[4];
            return (int)result;
        }

        public string GetDisplayInfo()
        {
            object result;
            result = obj.InvokeMember("GetDisplayInfo", BindingFlags.InvokeMethod, null, obj_object, null);
            return result.ToString();
        }

        public int GetWindowProcessId(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("GetWindowProcessId", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int EnableKeypadSync(int en,int time_out)
        {
            object[] args = new object[2];
            object result;
            args[0] = en;
            args[1] = time_out;

            result = obj.InvokeMember("EnableKeypadSync", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string FindString(int hwnd,string addr_range,string string_value,int tpe)
        {
            object[] args = new object[4];
            object result;
            args[0] = hwnd;
            args[1] = addr_range;
            args[2] = string_value;
            args[3] = tpe;

            result = obj.InvokeMember("FindString", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int CreateFoobarEllipse(int hwnd,int x,int y,int w,int h)
        {
            object[] args = new object[5];
            object result;
            args[0] = hwnd;
            args[1] = x;
            args[2] = y;
            args[3] = w;
            args[4] = h;

            result = obj.InvokeMember("CreateFoobarEllipse", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int WriteDoubleAddr(int hwnd,long addr,double v)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = v;

            result = obj.InvokeMember("WriteDoubleAddr", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetPicPwd(string pwd)
        {
            object[] args = new object[1];
            object result;
            args[0] = pwd;

            result = obj.InvokeMember("SetPicPwd", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int MoveDD(int dx,int dy)
        {
            object[] args = new object[2];
            object result;
            args[0] = dx;
            args[1] = dy;

            result = obj.InvokeMember("MoveDD", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int AddDict(int index,string dict_info)
        {
            object[] args = new object[2];
            object result;
            args[0] = index;
            args[1] = dict_info;

            result = obj.InvokeMember("AddDict", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SendStringIme2(int hwnd,string str,int mode)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = str;
            args[2] = mode;

            result = obj.InvokeMember("SendStringIme2", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int InitCri()
        {
            object result;
            result = obj.InvokeMember("InitCri", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public string FetchWord(int x1,int y1,int x2,int y2,string color,string word)
        {
            object[] args = new object[6];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = color;
            args[5] = word;

            result = obj.InvokeMember("FetchWord", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int VirtualProtectEx(int hwnd,long addr,int size,int tpe,int old_protect)
        {
            object[] args = new object[5];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = size;
            args[3] = tpe;
            args[4] = old_protect;

            result = obj.InvokeMember("VirtualProtectEx", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int LeftDown()
        {
            object result;
            result = obj.InvokeMember("LeftDown", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int GetWindowState(int hwnd,int flag)
        {
            object[] args = new object[2];
            object result;
            args[0] = hwnd;
            args[1] = flag;

            result = obj.InvokeMember("GetWindowState", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string Hex64(long v)
        {
            object[] args = new object[1];
            object result;
            args[0] = v;

            result = obj.InvokeMember("Hex64", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int RightDown()
        {
            object result;
            result = obj.InvokeMember("RightDown", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int SetExcludeRegion(int tpe,string info)
        {
            object[] args = new object[2];
            object result;
            args[0] = tpe;
            args[1] = info;

            result = obj.InvokeMember("SetExcludeRegion", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int RightClick()
        {
            object result;
            result = obj.InvokeMember("RightClick", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int EnableSpeedDx(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("EnableSpeedDx", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public long GetModuleBaseAddr(int hwnd,string module_name)
        {
            object[] args = new object[2];
            object result;
            args[0] = hwnd;
            args[1] = module_name;

            result = obj.InvokeMember("GetModuleBaseAddr", BindingFlags.InvokeMethod, null, obj_object, args);
            return Convert.ToInt64(result);
        }

        public int FindWindowByProcessId(int process_id,string class_name,string title_name)
        {
            object[] args = new object[3];
            object result;
            args[0] = process_id;
            args[1] = class_name;
            args[2] = title_name;

            result = obj.InvokeMember("FindWindowByProcessId", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string FindShapeE(int x1,int y1,int x2,int y2,string offset_color,double sim,int dir)
        {
            object[] args = new object[7];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = offset_color;
            args[5] = sim;
            args[6] = dir;

            result = obj.InvokeMember("FindShapeE", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int GetResultPos(string str,int index,out int x,out int y)
        {
            object[] args = new object[4];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(4);
            mods[0][2] = true;
            mods[0][3] = true;

            args[0] = str;
            args[1] = index;

            result = obj.InvokeMember("GetResultPos", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[2];
            y = (int)args[3];
            return (int)result;
        }

        public string FindColorEx(int x1,int y1,int x2,int y2,string color,double sim,int dir)
        {
            object[] args = new object[7];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = color;
            args[5] = sim;
            args[6] = dir;

            result = obj.InvokeMember("FindColorEx", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int WheelDown()
        {
            object result;
            result = obj.InvokeMember("WheelDown", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int FaqIsPosted()
        {
            object result;
            result = obj.InvokeMember("FaqIsPosted", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int LockMouseRect(int x1,int y1,int x2,int y2)
        {
            object[] args = new object[4];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;

            result = obj.InvokeMember("LockMouseRect", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FoobarClearText(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("FoobarClearText", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int CreateFoobarRoundRect(int hwnd,int x,int y,int w,int h,int rw,int rh)
        {
            object[] args = new object[7];
            object result;
            args[0] = hwnd;
            args[1] = x;
            args[2] = y;
            args[3] = w;
            args[4] = h;
            args[5] = rw;
            args[6] = rh;

            result = obj.InvokeMember("CreateFoobarRoundRect", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetInputDm(int input_dm,int rx,int ry)
        {
            object[] args = new object[3];
            object result;
            args[0] = input_dm;
            args[1] = rx;
            args[2] = ry;

            result = obj.InvokeMember("SetInputDm", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetDict(int index,int font_index)
        {
            object[] args = new object[2];
            object result;
            args[0] = index;
            args[1] = font_index;

            result = obj.InvokeMember("GetDict", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string GetDictInfo(string str,string font_name,int font_size,int flag)
        {
            object[] args = new object[4];
            object result;
            args[0] = str;
            args[1] = font_name;
            args[2] = font_size;
            args[3] = flag;

            result = obj.InvokeMember("GetDictInfo", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string EnumWindowSuper(string spec1,int flag1,int type1,string spec2,int flag2,int type2,int sort)
        {
            object[] args = new object[7];
            object result;
            args[0] = spec1;
            args[1] = flag1;
            args[2] = type1;
            args[3] = spec2;
            args[4] = flag2;
            args[5] = type2;
            args[6] = sort;

            result = obj.InvokeMember("EnumWindowSuper", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int WriteDataFromBin(int hwnd,string addr,int data,int length)
        {
            object[] args = new object[4];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = data;
            args[3] = length;

            result = obj.InvokeMember("WriteDataFromBin", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string FindInt(int hwnd,string addr_range,long int_value_min,long int_value_max,int tpe)
        {
            object[] args = new object[5];
            object result;
            args[0] = hwnd;
            args[1] = addr_range;
            args[2] = int_value_min;
            args[3] = int_value_max;
            args[4] = tpe;

            result = obj.InvokeMember("FindInt", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int FindWindowByProcess(string process_name,string class_name,string title_name)
        {
            object[] args = new object[3];
            object result;
            args[0] = process_name;
            args[1] = class_name;
            args[2] = title_name;

            result = obj.InvokeMember("FindWindowByProcess", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int GetDmCount()
        {
            object result;
            result = obj.InvokeMember("GetDmCount", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int RegExNoMac(string code,string Ver,string ip)
        {
            object[] args = new object[3];
            object result;
            args[0] = code;
            args[1] = Ver;
            args[2] = ip;

            result = obj.InvokeMember("RegExNoMac", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetParam64ToPointer()
        {
            object result;
            result = obj.InvokeMember("SetParam64ToPointer", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public string FaqSend(string server,int handle,int request_type,int time_out)
        {
            object[] args = new object[4];
            object result;
            args[0] = server;
            args[1] = handle;
            args[2] = request_type;
            args[3] = time_out;

            result = obj.InvokeMember("FaqSend", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string EnumWindowByProcess(string process_name,string title,string class_name,int filter)
        {
            object[] args = new object[4];
            object result;
            args[0] = process_name;
            args[1] = title;
            args[2] = class_name;
            args[3] = filter;

            result = obj.InvokeMember("EnumWindowByProcess", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int GetScreenHeight()
        {
            object result;
            result = obj.InvokeMember("GetScreenHeight", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int GetResultCount(string str)
        {
            object[] args = new object[1];
            object result;
            args[0] = str;

            result = obj.InvokeMember("GetResultCount", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int Is64Bit()
        {
            object result;
            result = obj.InvokeMember("Is64Bit", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int AsmClear()
        {
            object result;
            result = obj.InvokeMember("AsmClear", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int LeftDoubleClick()
        {
            object result;
            result = obj.InvokeMember("LeftDoubleClick", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int FoobarClose(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("FoobarClose", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FindWindowSuper(string spec1,int flag1,int type1,string spec2,int flag2,int type2)
        {
            object[] args = new object[6];
            object result;
            args[0] = spec1;
            args[1] = flag1;
            args[2] = type1;
            args[3] = spec2;
            args[4] = flag2;
            args[5] = type2;

            result = obj.InvokeMember("FindWindowSuper", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FaqCaptureString(string str)
        {
            object[] args = new object[1];
            object result;
            args[0] = str;

            result = obj.InvokeMember("FaqCaptureString", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int HackSpeed(double rate)
        {
            object[] args = new object[1];
            object result;
            args[0] = rate;

            result = obj.InvokeMember("HackSpeed", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string EnumIniKey(string section,string file_name)
        {
            object[] args = new object[2];
            object result;
            args[0] = section;
            args[1] = file_name;

            result = obj.InvokeMember("EnumIniKey", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public string FindColorE(int x1,int y1,int x2,int y2,string color,double sim,int dir)
        {
            object[] args = new object[7];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = color;
            args[5] = sim;
            args[6] = dir;

            result = obj.InvokeMember("FindColorE", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int DisableScreenSave()
        {
            object result;
            result = obj.InvokeMember("DisableScreenSave", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int FindPicSim(int x1,int y1,int x2,int y2,string pic_name,string delta_color,int sim,int dir,out int x,out int y)
        {
            object[] args = new object[10];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(10);
            mods[0][8] = true;
            mods[0][9] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = pic_name;
            args[5] = delta_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindPicSim", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[8];
            y = (int)args[9];
            return (int)result;
        }

        public int StrStr(string s,string str)
        {
            object[] args = new object[2];
            object result;
            args[0] = s;
            args[1] = str;

            result = obj.InvokeMember("StrStr", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int DownCpu(int tpe,int rate)
        {
            object[] args = new object[2];
            object result;
            args[0] = tpe;
            args[1] = rate;

            result = obj.InvokeMember("DownCpu", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int IsBind(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("IsBind", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int DecodeFile(string file_name,string pwd)
        {
            object[] args = new object[2];
            object result;
            args[0] = file_name;
            args[1] = pwd;

            result = obj.InvokeMember("DecodeFile", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetMinRowGap(int row_gap)
        {
            object[] args = new object[1];
            object result;
            args[0] = row_gap;

            result = obj.InvokeMember("SetMinRowGap", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int GetWordResultPos(string str,int index,out int x,out int y)
        {
            object[] args = new object[4];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(4);
            mods[0][2] = true;
            mods[0][3] = true;

            args[0] = str;
            args[1] = index;

            result = obj.InvokeMember("GetWordResultPos", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[2];
            y = (int)args[3];
            return (int)result;
        }

        public int CapturePng(int x1,int y1,int x2,int y2,string file_name)
        {
            object[] args = new object[5];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = file_name;

            result = obj.InvokeMember("CapturePng", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int CheckUAC()
        {
            object result;
            result = obj.InvokeMember("CheckUAC", BindingFlags.InvokeMethod, null, obj_object, null);
            return (int)result;
        }

        public int FindPicSimMem(int x1,int y1,int x2,int y2,string pic_info,string delta_color,int sim,int dir,out int x,out int y)
        {
            object[] args = new object[10];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(10);
            mods[0][8] = true;
            mods[0][9] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = pic_info;
            args[5] = delta_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindPicSimMem", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[8];
            y = (int)args[9];
            return (int)result;
        }

        public int FreeProcessMemory(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("FreeProcessMemory", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string FindDouble(int hwnd,string addr_range,double double_value_min,double double_value_max)
        {
            object[] args = new object[4];
            object result;
            args[0] = hwnd;
            args[1] = addr_range;
            args[2] = double_value_min;
            args[3] = double_value_max;

            result = obj.InvokeMember("FindDouble", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public float ReadFloat(int hwnd,string addr)
        {
            object[] args = new object[2];
            object result;
            args[0] = hwnd;
            args[1] = addr;

            result = obj.InvokeMember("ReadFloat", BindingFlags.InvokeMethod, null, obj_object, args);
            return (float)result;
        }

        public long ReadInt(int hwnd,string addr,int tpe)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = tpe;

            result = obj.InvokeMember("ReadInt", BindingFlags.InvokeMethod, null, obj_object, args);
            return Convert.ToInt64(result);
        }

        public int GetClientRect(int hwnd,out int x1,out int y1,out int x2,out int y2)
        {
            object[] args = new object[5];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(5);
            mods[0][1] = true;
            mods[0][2] = true;
            mods[0][3] = true;
            mods[0][4] = true;

            args[0] = hwnd;

            result = obj.InvokeMember("GetClientRect", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x1 = (int)args[1];
            y1 = (int)args[2];
            x2 = (int)args[3];
            y2 = (int)args[4];
            return (int)result;
        }

        public string GetDir(int tpe)
        {
            object[] args = new object[1];
            object result;
            args[0] = tpe;

            result = obj.InvokeMember("GetDir", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int EnableGetColorByCapture(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("EnableGetColorByCapture", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string EnumWindow(int parent,string title,string class_name,int filter)
        {
            object[] args = new object[4];
            object result;
            args[0] = parent;
            args[1] = title;
            args[2] = class_name;
            args[3] = filter;

            result = obj.InvokeMember("EnumWindow", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int EnableFakeActive(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("EnableFakeActive", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int EnableMouseAccuracy(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("EnableMouseAccuracy", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetDisplayDelay(int t)
        {
            object[] args = new object[1];
            object result;
            args[0] = t;

            result = obj.InvokeMember("SetDisplayDelay", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int WriteIni(string section,string key,string v,string file_name)
        {
            object[] args = new object[4];
            object result;
            args[0] = section;
            args[1] = key;
            args[2] = v;
            args[3] = file_name;

            result = obj.InvokeMember("WriteIni", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int KeyPressChar(string key_str)
        {
            object[] args = new object[1];
            object result;
            args[0] = key_str;

            result = obj.InvokeMember("KeyPressChar", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string FindMultiColorEx(int x1,int y1,int x2,int y2,string first_color,string offset_color,double sim,int dir)
        {
            object[] args = new object[8];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = first_color;
            args[5] = offset_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindMultiColorEx", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int FindShape(int x1,int y1,int x2,int y2,string offset_color,double sim,int dir,out int x,out int y)
        {
            object[] args = new object[9];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(9);
            mods[0][7] = true;
            mods[0][8] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = offset_color;
            args[5] = sim;
            args[6] = dir;

            result = obj.InvokeMember("FindShape", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[7];
            y = (int)args[8];
            return (int)result;
        }

        public string GetBasePath()
        {
            object result;
            result = obj.InvokeMember("GetBasePath", BindingFlags.InvokeMethod, null, obj_object, null);
            return result.ToString();
        }

        public int BindWindow(int hwnd,string display,string mouse,string keypad,int mode)
        {
            object[] args = new object[5];
            object result;
            args[0] = hwnd;
            args[1] = display;
            args[2] = mouse;
            args[3] = keypad;
            args[4] = mode;

            result = obj.InvokeMember("BindWindow", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetScreen(int width,int height,int depth)
        {
            object[] args = new object[3];
            object result;
            args[0] = width;
            args[1] = height;
            args[2] = depth;

            result = obj.InvokeMember("SetScreen", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int GetWindow(int hwnd,int flag)
        {
            object[] args = new object[2];
            object result;
            args[0] = hwnd;
            args[1] = flag;

            result = obj.InvokeMember("GetWindow", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetColorBGR(int x,int y)
        {
            object[] args = new object[2];
            object result;
            args[0] = x;
            args[1] = y;

            result = obj.InvokeMember("GetColorBGR", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int EnableRealKeypad(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("EnableRealKeypad", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int ClearDict(int index)
        {
            object[] args = new object[1];
            object result;
            args[0] = index;

            result = obj.InvokeMember("ClearDict", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetExitThread(int en)
        {
            object[] args = new object[1];
            object result;
            args[0] = en;

            result = obj.InvokeMember("SetExitThread", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string GetDiskSerial(int index)
        {
            object[] args = new object[1];
            object result;
            args[0] = index;

            result = obj.InvokeMember("GetDiskSerial", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int SendPaste(int hwnd)
        {
            object[] args = new object[1];
            object result;
            args[0] = hwnd;

            result = obj.InvokeMember("SendPaste", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string FindPicSimMemE(int x1,int y1,int x2,int y2,string pic_info,string delta_color,int sim,int dir)
        {
            object[] args = new object[8];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = pic_info;
            args[5] = delta_color;
            args[6] = sim;
            args[7] = dir;

            result = obj.InvokeMember("FindPicSimMemE", BindingFlags.InvokeMethod, null, obj_object, args);
            return result.ToString();
        }

        public int MoveTo(int x,int y)
        {
            object[] args = new object[2];
            object result;
            args[0] = x;
            args[1] = y;

            result = obj.InvokeMember("MoveTo", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int Reg(string code,string Ver)
        {
            object[] args = new object[2];
            object result;
            args[0] = code;
            args[1] = Ver;

            result = obj.InvokeMember("Reg", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int CaptureGif(int x1,int y1,int x2,int y2,string file_name,int quality)
        {
            object[] args = new object[6];
            object result;
            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = file_name;
            args[5] = quality;

            result = obj.InvokeMember("CaptureGif", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int SetSimMode(int mode)
        {
            object[] args = new object[1];
            object result;
            args[0] = mode;

            result = obj.InvokeMember("SetSimMode", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int DownloadFile(string url,string save_file,int timeout)
        {
            object[] args = new object[3];
            object result;
            args[0] = url;
            args[1] = save_file;
            args[2] = timeout;

            result = obj.InvokeMember("DownloadFile", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int CapturePre(string file_name)
        {
            object[] args = new object[1];
            object result;
            args[0] = file_name;

            result = obj.InvokeMember("CapturePre", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int CreateFolder(string folder_name)
        {
            object[] args = new object[1];
            object result;
            args[0] = folder_name;

            result = obj.InvokeMember("CreateFolder", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int VirtualFreeEx(int hwnd,long addr)
        {
            object[] args = new object[2];
            object result;
            args[0] = hwnd;
            args[1] = addr;

            result = obj.InvokeMember("VirtualFreeEx", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int WriteFloat(int hwnd,string addr,float v)
        {
            object[] args = new object[3];
            object result;
            args[0] = hwnd;
            args[1] = addr;
            args[2] = v;

            result = obj.InvokeMember("WriteFloat", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int FindColorBlock(int x1,int y1,int x2,int y2,string color,double sim,int count,int width,int height,out int x,out int y)
        {
            object[] args = new object[11];
            object result;
            ParameterModifier[] mods = new ParameterModifier[1];

            mods[0] = new ParameterModifier(11);
            mods[0][9] = true;
            mods[0][10] = true;

            args[0] = x1;
            args[1] = y1;
            args[2] = x2;
            args[3] = y2;
            args[4] = color;
            args[5] = sim;
            args[6] = count;
            args[7] = width;
            args[8] = height;

            result = obj.InvokeMember("FindColorBlock", BindingFlags.InvokeMethod, null, obj_object, args, mods, null, null);
            x = (int)args[9];
            y = (int)args[10];
            return (int)result;
        }

        public int KeyUp(int vk)
        {
            object[] args = new object[1];
            object result;
            args[0] = vk;

            result = obj.InvokeMember("KeyUp", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public int WriteInt(int hwnd,string addr,int tpe,long v)
        {
            if (!IsAvailable || obj_object == null || obj == null) return -1;
            object[] args = new object[4];
            args[0] = hwnd;
            args[1] = addr;
            args[2] = tpe;
            args[3] = v;
            var result = obj.InvokeMember("WriteInt", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }

        public string ReadString(int hwnd,string addr,int tpe,int length)
        {
            if (!IsAvailable || obj_object == null || obj == null) return string.Empty;
            object[] args = new object[4];
            args[0] = hwnd;
            args[1] = addr;
            args[2] = tpe;
            args[3] = length;
            var result = obj.InvokeMember("ReadString", BindingFlags.InvokeMethod, null, obj_object, args);
            return result?.ToString() ?? string.Empty;
        }

        public int WriteString(int hwnd,string addr,int tpe,string v)
        {
            if (!IsAvailable || obj_object == null || obj == null) return -1;
            object[] args = new object[4];
            args[0] = hwnd;
            args[1] = addr;
            args[2] = tpe;
            args[3] = v;
            var result = obj.InvokeMember("WriteString", BindingFlags.InvokeMethod, null, obj_object, args);
            return (int)result;
        }
    }
}
