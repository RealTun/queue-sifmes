using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSifmes
{
    public class bool_struct
    {
        public bool bool_1 { get; set; }
        public bool bool_2 { get; set; }
        public bool bool_3 { get; set; }
        public bool bool_4 { get; set; }
        public bool bool_5 { get; set; }
        public bool bool_6 { get; set; }
        public bool bool_7 { get; set; }
        public bool bool_8 { get; set; }
    }
    public class byte_struct
    {
        public byte byte_1 { get; set; }
        public byte byte_2 { get; set; }
        public byte byte_3 { get; set; }
        public byte byte_4 { get; set; }
        public byte byte_5 { get; set; }
    }
    public class int_struct
    {
        public short int_1 { get; set; }
        public short int_2 { get; set; }
        public short int_3 { get; set; }
        public short int_4 { get; set; }
        public short int_5 { get; set; }
    }
    public class float_struct
    {
        public double float_1 { get; set; }
        public double float_2 { get; set; }
        public double float_3 { get; set; }
        public double float_4 { get; set; }
        public double float_5 { get; set; }
    }
    public class string_struct
    {
        public string string_plc { get; set; }
    }
    public class start_byte_for_struct
    {
        public static int bool_start_byte = 0;
        public static int byte_start_byte = 26;
        public static int int_start_byte = 126;
        public static int float_start_byte = 226;
        public static int string_start_byte = 426;
    }
}
