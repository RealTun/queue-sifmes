using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSifmes
{
    internal class APIClient
    {
        public APIClient() { }
        public static string read_string(Plc PLC, int dbnumber, int startAddr)
        {
            try
            {
                byte[] bytes = PLC.ReadBytes(DataType.DataBlock, dbnumber, startAddr, 2);
                int StringLength = bytes[1];
                byte[] read_bytes = PLC.ReadBytes(DataType.DataBlock, dbnumber, startAddr, 256);
                bytes = PLC.ReadBytes(DataType.DataBlock, dbnumber, startAddr + 2, StringLength);
                return System.Text.Encoding.ASCII.GetString(bytes).TrimEnd('\0');
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public static bool read_bool(Plc PLC, int dbnumber, int bit_index)
        {
            try
            {
                return S7.Net.Types.Boolean.GetValue(PLC.ReadBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.bool_start_byte, 1)[0], bit_index);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tại PLC: {PLC.IP} {nameof(read_bool)}: {ex.Message}");
                return false;
            }
        }
        public static byte read_bool_all(Plc PLC, int dbnumber)
        {
            return PLC.ReadBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.bool_start_byte, 1)[0];
        }
        public static byte read_byte(Plc PLC, int dbnumber, int byte_index)
        {
            return PLC.ReadBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.byte_start_byte + byte_index, 1)[0];
        }
        public static byte_struct read_byte_all(Plc PLC, int dbnumber)
        {
            var result = new byte_struct();
            PLC.ReadClass(result, dbnumber, start_byte_for_struct.byte_start_byte);
            return result;
        }
        public static short read_int(Plc PLC, int dbnumber, int byte_index)
        {
            return S7.Net.Types.Int.FromByteArray(PLC.ReadBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.int_start_byte + byte_index * 2, 2));
        }
        public static int_struct read_int_all(Plc PLC, int dbnumber)
        {
            var result = new int_struct();
            PLC.ReadClass(result, dbnumber, start_byte_for_struct.int_start_byte);
            return result;
        }
        public static float read_float(Plc PLC, int dbnumber, int byte_index)
        {
            var t = S7.Net.Types.Double.FromByteArray(PLC.ReadBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.float_start_byte + byte_index * 4, 4));
            return (float)t;
        }
        public static float_struct read_float_all(Plc PLC, int dbnumber)
        {
            var result = new float_struct();
            PLC.ReadClass(result, dbnumber, start_byte_for_struct.float_start_byte);
            return result;
        }
        public static void sendString(Plc PLC, int dbnumber, int startAddr, string str_input)
        {
            try
            {
                byte[] dataBytes = S7.Net.Types.String.ToByteArray(str_input, 38);
                List<byte> values = new List<byte>();
                byte string_length = (byte)str_input.Length;
                values.Add(254);
                values.Add(string_length);
                values.AddRange(dataBytes);
                PLC.WriteBytes(DataType.DataBlock, dbnumber, startAddr, values.ToArray());
            }
            catch (PlcException ex)
            {
                Console.WriteLine($"Lỗi tại PLC: {PLC.IP} {nameof(sendString)}: {ex.Message}");
            }
        }

        public static void sendBool(Plc PLC, int dbnumber, int bit_index, bool value)
        {
            byte[] t = PLC.ReadBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.bool_start_byte, 1);
            byte tt = t[0];
            if (value == true)
            {
                t[0] = (byte)(t[0] | (1 << bit_index));
            }
            else if (value == false)
            {
                t[0] = (byte)(t[0] & (~(1 << bit_index)));
            };
            PLC.WriteBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.bool_start_byte, t);
        }
        public static void sendByte(Plc PLC, int dbnumber, int byte_index, byte value)
        {
            try
            {
                byte[] t = S7.Net.Types.Byte.ToByteArray(value);
                PLC.WriteBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.byte_start_byte + byte_index, t);
            }
            catch (PlcException ex)
            {
                Console.WriteLine($"Lỗi tại PLC: {PLC.IP} {nameof(sendByte)}: {ex.Message}");
            }
        }
        public static void sendInt(Plc PLC, int dbnumber, int int_index, int value)
        {
            try
            {
                byte[] t = S7.Net.Types.Int.ToByteArray((short)value);
                PLC.WriteBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.int_start_byte + int_index * 2, t);
            }
            catch (PlcException ex)
            {
                Console.WriteLine($"Lỗi tại PLC: {PLC.IP} {nameof(sendFloat)} với (DB: {dbnumber}, Index: {int_index}, Value: {value}): {ex.Message}");
            }
        }
        public static void sendFloat(Plc PLC, int dbnumber, int float_index, float value)
        {
            try
            {
                byte[] t = S7.Net.Types.Double.ToByteArray((double)value);
                PLC.WriteBytes(DataType.DataBlock, dbnumber, start_byte_for_struct.float_start_byte + float_index * 4, t);
            }
            catch (PlcException ex)
            {
                Console.WriteLine($"Lỗi tại PLC: {PLC.IP} {nameof(sendFloat)}: {ex.Message}");
            }
        }
    }
}
