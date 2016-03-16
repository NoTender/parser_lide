using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Test
{
    public class Parser
    {
        private const int TYPE_MAGIC = 25;
        private const int TYPE_CALL = 13;
        private const int TYPE_RESPONSE = 14;
        private const int TYPE_FAULT = 15;

        private const int TYPE_INT = 1;
        private const int TYPE_BOOL = 2;
        private const int TYPE_DOUBLE = 3;
        private const int TYPE_STRING = 4;
        private const int TYPE_DATETIME = 5;
        private const int TYPE_BINARY = 6;
        private const int TYPE_INT8P = 7;
        private const int TYPE_INT8N = 8;
        private const int TYPE_STRUCT = 10;
        private const int TYPE_ARRAY = 11;
        private const int TYPE_NULL = 12;

        private byte[] data = null;
        private int pointer = 0;

        public Parser(byte[] array) //Konstruktor, jenz vyplni tridni promennou data vstupnimi daty
        {
            data = array;
        }

        public string parse()
        {
            string result = null;
            var magic1 = getByte();
            var magic2 = getByte();

            if (magic1 != 0xCA || magic2 != 0x11)
            {
                data = null;
                throw new MyException("Missing FRPC magic");
            }
            //Zahozeni nasledujicich dvou bytu z hlavicky:
            getByte();
            getByte();

            int first = getInt(1);
            int type = first >> 3;

            if (type == TYPE_FAULT)
            {
                throw new MyException("FRPC/" + parseValue() + ": " +parseValue());
            }

            switch (type)
            {
                case TYPE_RESPONSE:
                    result = parseValue();
                    if (pointer  < data.Length)
                    {
                        data = null;
                        throw new MyException("Garbage after FRPC data");
                    }
                    break;

                case TYPE_CALL:
                    return null;

                default:
                    data = null;
                    throw new MyException("Unsupported FRPC type " + type);
            }
            data = null;
            return result;
        }

        private byte getByte() //Metoda, ktera vrati hodnotu nasledujiciho bytu
        {
            if ((pointer + 1) > data.Length)
            {
                throw new IndexOutOfRangeException("Value of pointer is out of range of array length.");
            }
            return data[pointer++];
        }

        private int getInt(int bytes)
        {
            int result = 0;
            int factor = 1;

            for (int i = 0; i < bytes; i++)
            {
                result += factor*getByte();
                factor *= 256;
            }

            return result;
        }

        private string parseValue()
        {
            int first = getInt(1);
            int type = first >> 3;

            int lengthBytes;
            int length;
            int members;
            string result;

            switch (type)
            {

                case TYPE_STRING:
                    lengthBytes = (first & 7) + 1;
                    length = getInt(lengthBytes);
                    result = decodeUTF8(length);
                    result = Regex.Replace(result, "\"", "\\\"");
                    return "\"" + result + "\"";

                case TYPE_STRUCT:
                    result = "";
                    lengthBytes = (first & 7) + 1;
                    members = getInt(lengthBytes);
                    while (members != 0)
                    {
                        members--;
                        parseMember(ref result);
                        if (members != 0) //Pridani carky, pokud se nejedna o posledni prvek struktury
                            result += ",";
                    }
                    return "{" + result + "}";

                case TYPE_ARRAY:
                    result = "";
                    lengthBytes = (first & 7) + 1;
                    members = getInt(lengthBytes);
                    while (members != 0)
                    {
                        members--;
                        result += parseValue();
                        if (members != 0)
                            result += ",";
                    }
                    return "[" + result + "]";

                case TYPE_BOOL:
                    if ((first & 1) == 1)
                        return "true";
                    else
                        return "false";

                case TYPE_INT:
                    length = first & 7;
                    int max = Convert.ToInt32(Math.Pow(2, 8*length));
                    int res = getInt(length);
                    if (res >= max/2)
                        res -= max;
                    return res.ToString();

                case TYPE_DATETIME:
                    getByte();
                    long ts = getInt(4);
                    for (var i = 0; i < 5; i++)
                        getByte();
                    DateTime date = TimeStampToDateTime(ts);
                    return "\"" + date.ToString() + " GMT+1" + "\"";

                case TYPE_DOUBLE:
                    return getDouble().ToString(CultureInfo.InvariantCulture);
                case TYPE_BINARY:
                    result = "";
                    lengthBytes = (first & 7) + 1;
                    length = getInt(lengthBytes);
                    while (length != 0)
                    {
                        length--;
                        result += getByte().ToString();
                        if (length != 0)
                            result += ",";
                    }
                    return "[" + result + "]";

                case TYPE_INT8P:
                    length = (first & 7) + 1;
                    return getInt(length).ToString();

                case TYPE_INT8N:
                    length = (first & 7) + 1;
                    return (-getInt(length)).ToString();

                case TYPE_NULL:
                    return "null";

                default:
                    throw new MyException("Uknown FRPC type: " + type.ToString());
            }

        }

        private string parseMember(ref string result)
        {
            int nameLength = getInt(1);
            string name = decodeUTF8(nameLength);
            string value = parseValue();
            if (name != "contributing_users")
                result += "\"" + name + "\":" + value;
            else
                result += "\"" + name + "\":" + structToArray(value);



            return result;
        }

        private string decodeUTF8(int length)
        {
            int remain = length;
            string result = "";

            if (length == 0)
                return result;

            var c = 0;
            var c1 = 0;
            var c2 = 0;
            byte[] data = this.data;
            int pointer = this.pointer;

            while (true)
            {
                remain --;
                c = data[pointer];
                pointer += 1;

                if (c < 128)
                {
                    result += Convert.ToChar(c);
                }
                else if ((c > 191) && (c < 224))
                {
                    c1 = data[pointer];
                    pointer += 1;
                    result += Convert.ToChar(((c & 31) << 6) | (c1 & 63));
                    remain -= 1;
                }
                else if (c < 240)
                {
                    c1 = data[pointer++];
                    c2 = data[pointer++];
                    result += Convert.ToChar(((c & 15) << 12) | ((c1 & 63) << 6) | (c2 & 63));
                    remain -= 2;
                }
                else if (c < 248)
                {
                    pointer += 3;
                    remain -= 3;
                }
                else if (c < 252)
                {
                    pointer += 4;
                    remain -= 4;
                }
                else
                {
                    pointer += 5;
                    remain -= 5;
                }

                if (remain <= 0)
                    break;
            }

            this.pointer = pointer + remain;
            return result;
        }

        double getDouble()
        {
            byte [] bytes = new byte[8];
            var index = 8;
            while (index != 0)
            {
                index--;
                bytes[index] = getByte();
            }

            var sign = ((bytes[0] & 0x80) != 0 ? 1 : 0);
            var exponent = (bytes[0] & 127) << 4;
            exponent += bytes[1] >> 4;

            if (exponent == 0)
                return Math.Pow(-1, sign) * 0;

            int mantissa = 0;
            var byteIndex = 1;
            var bitIndex = 3;
                index = 1;
            do
            {
                var bitValue = ((bytes[byteIndex] & (1 << bitIndex)) != 0 ? 1 : 0);
                mantissa += bitValue * Convert.ToInt32(Math.Pow(2, -index));

                index++;
                bitIndex--;
                if (bitIndex < 0)
                {
                    bitIndex = 7;
                    byteIndex++;
                }
            } while (byteIndex < bytes.Length);

            if (exponent == 0x7ff)
            {
                if (mantissa != 0)
                {
                    return Double.NaN;
                }
                else {
                    return Math.Pow(-1, sign) * Double.PositiveInfinity;
                }
            }
            exponent -= (1 << 10) - 1;
            return Math.Pow(-1, sign) * Math.Pow(2, exponent) * (1 + mantissa);
        }
        private DateTime TimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private string structToArray(string structure)
        {
            structure = structure.Trim();
            if (structure.Length < 1 || structure[0] != '{' || structure[structure.Length-1] != '}') //Pokud se jedna o prazdny retezec, nebo hodnota promenne contributing users neni struktura
                return structure;

            if (Regex.Replace(structure, @"\s+", "") == "{}") //Pokud se jedna o prazdnou strukturu, vrati se prazdne pole.
                return "[]";

            structure = structure.Remove(0, 1); //Odstraneni prvni a posledni slozene zavorky, aby se odstranily obalujici zavorky
            structure = structure.Remove(structure.Length - 1);

            var startIndex = structure.IndexOf('{'); //Pozice, na ktere zacina prvni vnorena struktura
            var position = startIndex;
            var numberOfBrackets = 0;
            List<string> members = new List<string>();

            if (startIndex == -1) //Pokud struktura neobsahuje vnorene struktury, vrati se to co tam bylo puvodne, stat by se to nemelo, ale pro jistotu
                return '{' + structure + '}';

            while (position < structure.Length)
            {
                if (structure[position] == '{')
                    numberOfBrackets++;
                else if (structure[position] == '}')
                    numberOfBrackets--;

                position++;
                if (numberOfBrackets == 0) //Pokud byla nalezena odpovidajici zavorka
                {
                    members.Add(structure.Substring(startIndex, position-startIndex)); //Pridani cele struktury do seznamu
                    startIndex = structure.IndexOf('{', position);
                    if (startIndex != -1) //Pokud se ve vstupu nachazi jeste dalsi struktura
                        position = startIndex;
                    else
                        break;
                }
            }

            string result = "";
            for (var i = 0; i < members.Count; i++)
            {
                result += members[i];
                if (i != members.Count - 1)
                    result += ',';
            }
            result = '[' + result + ']';

            return result;
        }
    }


    [Serializable]
    public class MyException : Exception
    {
        public MyException()
        {
        }

        public MyException(string message) : base(message)
        {
        }

        public MyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}